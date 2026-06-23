using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
    public enum BrickDependencyCoverageStatus
    {
        Covered = 0,
        PartiallyObservable = 1,
        NotObservable = 2,
        InsufficientEvidence = 3
    }

    public sealed class BrickDependencyCoverageTarget
    {
        public BrickDependencyCoverageTarget(
            BrickDependencyKindId kindId,
            BrickDependencyLayer layer,
            BrickEvidenceLevel minimumEvidenceLevel,
            bool required,
            string rationale = null)
        {
            KindId = kindId;
            Layer = layer;
            MinimumEvidenceLevel = minimumEvidenceLevel;
            Required = required;
            Rationale = rationale ?? string.Empty;
        }

        public BrickDependencyKindId KindId { get; }
        public BrickDependencyLayer Layer { get; }
        public BrickEvidenceLevel MinimumEvidenceLevel { get; }
        public bool Required { get; }
        public string Rationale { get; }

        public bool IsSatisfiedBy(BrickEvidenceLevel observedEvidenceLevel) =>
            EvidenceRank(observedEvidenceLevel) <= EvidenceRank(MinimumEvidenceLevel);

        private static int EvidenceRank(BrickEvidenceLevel evidenceLevel)
        {
            switch (evidenceLevel)
            {
                case BrickEvidenceLevel.CompilerConfirmed:
                    return 0;
                case BrickEvidenceLevel.AnalyzerInferred:
                    return 1;
                case BrickEvidenceLevel.ConfigurationDeclared:
                    return 2;
                case BrickEvidenceLevel.RuntimeInferred:
                    return 3;
                default:
                    return 4;
            }
        }
    }

    public sealed class BrickDependencyCoverageResult
    {
        public BrickDependencyCoverageResult(
            BrickDependencyCoverageTarget target,
            int analyzedDependencies,
            int observableDependencies,
            BrickEvidenceLevel observedEvidenceLevel,
            string notes = null)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            AnalyzedDependencies = Math.Max(0, analyzedDependencies);
            ObservableDependencies = Math.Min(AnalyzedDependencies, Math.Max(0, observableDependencies));
            UnobservableDependencies = Math.Max(0, AnalyzedDependencies - ObservableDependencies);
            CoverageRatio = AnalyzedDependencies == 0
                ? 0d
                : (double)ObservableDependencies / AnalyzedDependencies;
            ObservedEvidenceLevel = observedEvidenceLevel;
            Notes = notes ?? string.Empty;
            MeetsEvidenceRequirement = Target.IsSatisfiedBy(ObservedEvidenceLevel);
            Status = ResolveStatus();
        }

        public BrickDependencyCoverageTarget Target { get; }
        public int AnalyzedDependencies { get; }
        public int ObservableDependencies { get; }
        public int UnobservableDependencies { get; }
        public double CoverageRatio { get; }
        public BrickEvidenceLevel ObservedEvidenceLevel { get; }
        public string Notes { get; }
        public bool MeetsEvidenceRequirement { get; }
        public BrickDependencyCoverageStatus Status { get; }

        private BrickDependencyCoverageStatus ResolveStatus()
        {
            if (AnalyzedDependencies == 0 || ObservableDependencies == 0)
            {
                return BrickDependencyCoverageStatus.NotObservable;
            }

            if (!MeetsEvidenceRequirement)
            {
                return BrickDependencyCoverageStatus.InsufficientEvidence;
            }

            return ObservableDependencies >= AnalyzedDependencies
                ? BrickDependencyCoverageStatus.Covered
                : BrickDependencyCoverageStatus.PartiallyObservable;
        }
    }

    public static class BrickBuiltInDependencyCoverageTargets
    {
        public static BrickDependencyCoverageTarget DependencyRegistration => Target(
            BrickDependencyRegistration.DependencyKind,
            BrickDependencyLayer.Runtime,
            BrickEvidenceLevel.AnalyzerInferred,
            "Dependency registrations are only visible when analyzers or explicit configuration observe the composition root.");

        public static BrickDependencyCoverageTarget FriendAssembly => Target(
            BrickFriendAssemblyGrant.DependencyKind,
            BrickDependencyLayer.Visibility,
            BrickEvidenceLevel.CompilerConfirmed,
            "Friend assembly grants are compiler-visible visibility dependencies.");

        public static BrickDependencyCoverageTarget ReflectionAccess => Target(
            BrickReflectionAccess.DependencyKind,
            BrickDependencyLayer.Runtime,
            BrickEvidenceLevel.RuntimeInferred,
            "Reflection access can bypass static structure and may be only partially observable.");

        public static BrickDependencyCoverageTarget RuntimeActivation => Target(
            BrickRuntimeActivation.DependencyKind,
            BrickDependencyLayer.Runtime,
            BrickEvidenceLevel.RuntimeInferred,
            "Runtime activation edges depend on runtime or configuration evidence.");

        public static BrickDependencyCoverageTarget TypeReference => Target(
            "TypeReference",
            BrickDependencyLayer.Static,
            BrickEvidenceLevel.CompilerConfirmed,
            "Type references are the baseline compiler-confirmed static dependency kind.");

        public static IReadOnlyList<BrickDependencyCoverageTarget> All => new[]
        {
            DependencyRegistration,
            FriendAssembly,
            ReflectionAccess,
            RuntimeActivation,
            TypeReference
        };

        private static BrickDependencyCoverageTarget Target(
            string kindId,
            BrickDependencyLayer layer,
            BrickEvidenceLevel minimumEvidenceLevel,
            string rationale) =>
            new BrickDependencyCoverageTarget(
                BrickDependencyKindId.From(kindId),
                layer,
                minimumEvidenceLevel,
                true,
                rationale);
    }

    public sealed class BrickDependencyCoverageReport
    {
        public const string CurrentSchema = "NMolecules.Bricks.DependencyCoverage/1.0";

        public BrickDependencyCoverageReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickDependencyCoverageResult> results)
            : this(generatedAt, results, CurrentSchema)
        {
        }

        public BrickDependencyCoverageReport(
            DateTimeOffset generatedAt,
            IEnumerable<BrickDependencyCoverageResult> results,
            string schema)
        {
            GeneratedAt = generatedAt;
            Results = (results ?? Enumerable.Empty<BrickDependencyCoverageResult>())
                .OrderBy(result => result.Target.KindId.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
            Summary = BrickDependencyCoverageSummary.FromResults(Results);
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickDependencyCoverageResult> Results { get; }
        public BrickDependencyCoverageSummary Summary { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public sealed class BrickDependencyCoverageSummary
    {
        private BrickDependencyCoverageSummary(
            int total,
            int covered,
            int partiallyObservable,
            int notObservable,
            int insufficientEvidence,
            double averageCoverageRatio)
        {
            Total = total;
            Covered = covered;
            PartiallyObservable = partiallyObservable;
            NotObservable = notObservable;
            InsufficientEvidence = insufficientEvidence;
            AverageCoverageRatio = averageCoverageRatio;
        }

        public int Total { get; }
        public int Covered { get; }
        public int PartiallyObservable { get; }
        public int NotObservable { get; }
        public int InsufficientEvidence { get; }
        public double AverageCoverageRatio { get; }

        public static BrickDependencyCoverageSummary FromResults(
            IEnumerable<BrickDependencyCoverageResult> results)
        {
            var items = (results ?? Enumerable.Empty<BrickDependencyCoverageResult>()).ToArray();
            return new BrickDependencyCoverageSummary(
                items.Length,
                items.Count(result => result.Status == BrickDependencyCoverageStatus.Covered),
                items.Count(result => result.Status == BrickDependencyCoverageStatus.PartiallyObservable),
                items.Count(result => result.Status == BrickDependencyCoverageStatus.NotObservable),
                items.Count(result => result.Status == BrickDependencyCoverageStatus.InsufficientEvidence),
                items.Length == 0 ? 0d : items.Average(result => result.CoverageRatio));
        }
    }

    public static class BrickDependencyCoverageReportJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(BrickDependencyCoverageReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            return JsonSerializer.Serialize(ToDto(report), Options);
        }

        private static ReportDto ToDto(BrickDependencyCoverageReport report) =>
            new ReportDto
            {
                Schema = report.Schema,
                GeneratedAt = report.GeneratedAt,
                Summary = new SummaryDto
                {
                    Total = report.Summary.Total,
                    Covered = report.Summary.Covered,
                    PartiallyObservable = report.Summary.PartiallyObservable,
                    NotObservable = report.Summary.NotObservable,
                    InsufficientEvidence = report.Summary.InsufficientEvidence,
                    AverageCoverageRatio = report.Summary.AverageCoverageRatio
                },
                Results = report.Results.Select(ToDto).ToArray()
            };

        private static ResultDto ToDto(BrickDependencyCoverageResult result) =>
            new ResultDto
            {
                KindId = result.Target.KindId.Value,
                Layer = result.Target.Layer.ToString(),
                MinimumEvidenceLevel = result.Target.MinimumEvidenceLevel.ToString(),
                Required = result.Target.Required,
                Rationale = result.Target.Rationale,
                AnalyzedDependencies = result.AnalyzedDependencies,
                ObservableDependencies = result.ObservableDependencies,
                UnobservableDependencies = result.UnobservableDependencies,
                CoverageRatio = result.CoverageRatio,
                ObservedEvidenceLevel = result.ObservedEvidenceLevel.ToString(),
                MeetsEvidenceRequirement = result.MeetsEvidenceRequirement,
                Status = result.Status.ToString(),
                Notes = string.IsNullOrEmpty(result.Notes) ? null : result.Notes
            };

        private sealed class ReportDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public SummaryDto Summary { get; set; }
            public ResultDto[] Results { get; set; }
        }

        private sealed class SummaryDto
        {
            public int Total { get; set; }
            public int Covered { get; set; }
            public int PartiallyObservable { get; set; }
            public int NotObservable { get; set; }
            public int InsufficientEvidence { get; set; }
            public double AverageCoverageRatio { get; set; }
        }

        private sealed class ResultDto
        {
            public string KindId { get; set; }
            public string Layer { get; set; }
            public string MinimumEvidenceLevel { get; set; }
            public bool Required { get; set; }
            public string Rationale { get; set; }
            public int AnalyzedDependencies { get; set; }
            public int ObservableDependencies { get; set; }
            public int UnobservableDependencies { get; set; }
            public double CoverageRatio { get; set; }
            public string ObservedEvidenceLevel { get; set; }
            public bool MeetsEvidenceRequirement { get; set; }
            public string Status { get; set; }
            public string Notes { get; set; }
        }
    }
}
