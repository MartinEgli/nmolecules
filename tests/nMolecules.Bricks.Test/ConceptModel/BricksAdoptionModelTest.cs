using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksAdoptionModelTest
    {
        [Fact]
        public void BrickBaselineEntryRecordsKnownExistingViolation()
        {
            var expiresAt = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var baseline = new BrickBaselineEntry(
                RuleId.From("BRK-001"),
                "Billing.Domain.*",
                "Billing.Infrastructure.*",
                "Accepted migration debt.",
                "Architecture Team",
                expiresAt);

            Assert.Equal(RuleId.From("BRK-001"), baseline.RuleId);
            Assert.Equal("Billing.Domain.*", baseline.SourcePattern);
            Assert.Equal("Billing.Infrastructure.*", baseline.TargetPattern);
            Assert.Equal("Accepted migration debt.", baseline.Justification);
            Assert.Equal("Architecture Team", baseline.Owner);
            Assert.Equal(expiresAt, baseline.ExpiresAt);
            Assert.False(baseline.IsExpired(expiresAt.AddDays(-1)));
            Assert.True(baseline.IsExpired(expiresAt.AddTicks(1)));
        }

        [Fact]
        public void BrickSuppressionRecordsIntentionalException()
        {
            var expiresAt = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
            var selector = new BrickElementSelector(
                BrickElementKind.Type,
                "Billing.Domain.LegacyOrderPolicy",
                "Billing");
            var suppression = new BrickSuppression(
                RuleId.From("BRK-002"),
                selector,
                "Legacy adapter until migration is complete.",
                "Team Billing",
                expiresAt);

            Assert.Equal(RuleId.From("BRK-002"), suppression.RuleId);
            Assert.Equal(selector, suppression.Selector);
            Assert.Equal("Legacy adapter until migration is complete.", suppression.Justification);
            Assert.Equal("Team Billing", suppression.Owner);
            Assert.Equal(expiresAt, suppression.ExpiresAt);
            Assert.False(suppression.IsExpired(expiresAt));
            Assert.True(suppression.IsExpired(expiresAt.AddDays(1)));
        }

        [Fact]
        public void AdoptionModelsNormalizeNullValues()
        {
            var selector = new BrickElementSelector(BrickElementKind.Unknown, null, null);
            var baseline = new BrickBaselineEntry(default, null, null, null, null, null);
            var suppression = new BrickSuppression(default, null, null, null, null);

            Assert.Equal(BrickElementKind.Unknown, selector.Kind);
            Assert.Equal(string.Empty, selector.Pattern);
            Assert.Null(selector.AssemblyName);
            Assert.Equal(string.Empty, baseline.SourcePattern);
            Assert.Equal(string.Empty, baseline.TargetPattern);
            Assert.Null(baseline.Justification);
            Assert.Null(baseline.Owner);
            Assert.False(baseline.IsExpired(DateTimeOffset.MaxValue));
            Assert.Equal(string.Empty, suppression.Justification);
            Assert.False(suppression.IsExpired(DateTimeOffset.MaxValue));
            Assert.Equal(0, default(BrickElementSelector).GetHashCode());
        }

        [Fact]
        public void BrickAdoptionDocumentCarriesCurrentSchemaAndSortsEntries()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 16, 0, 0, TimeSpan.Zero);
            var laterBaseline = new BrickBaselineEntry(RuleId.From("BRK-002"), "Z.Source", "Target");
            var earlierBaseline = new BrickBaselineEntry(RuleId.From("BRK-001"), "A.Source", "Target");
            var laterSuppression = new BrickSuppression(RuleId.From("BRK-002"), new BrickElementSelector(BrickElementKind.Type, "Z.Type"), "Justified.");
            var earlierSuppression = new BrickSuppression(RuleId.From("BRK-001"), new BrickElementSelector(BrickElementKind.Type, "A.Type"), "Justified.");
            var baselines = new[] { laterBaseline, earlierBaseline };
            var suppressions = new[] { laterSuppression, earlierSuppression };

            var document = new BrickAdoptionDocument(generatedAt, baselines, suppressions);
            baselines[0] = new BrickBaselineEntry(RuleId.From("BRK-999"), "Mutated", "Mutated");
            suppressions[0] = new BrickSuppression(RuleId.From("BRK-999"), new BrickElementSelector(BrickElementKind.Type, "Mutated"), "Mutated");

            Assert.Equal(BrickAdoptionDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.True(document.HasEntries);
            Assert.Equal(generatedAt, document.GeneratedAt);
            Assert.Equal(new[] { earlierBaseline, laterBaseline }, document.Baselines.ToArray());
            Assert.Equal(new[] { earlierSuppression, laterSuppression }, document.Suppressions.ToArray());
        }

        [Fact]
        public void BrickAdoptionDocumentNormalizesNullInputs()
        {
            var document = new BrickAdoptionDocument(DateTimeOffset.UnixEpoch, null, null, null);

            Assert.Equal(string.Empty, document.Schema);
            Assert.False(document.IsCurrentSchema);
            Assert.False(document.HasEntries);
            Assert.Empty(document.Baselines);
            Assert.Empty(document.Suppressions);
        }

        [Fact]
        public void BrickElementSelectorExposesValueSemantics()
        {
            var selector = new BrickElementSelector(BrickElementKind.Type, "Billing.*", "Billing");

            Assert.True(selector.Equals((object)new BrickElementSelector(BrickElementKind.Type, "Billing.*", "Billing")));
            Assert.False(selector.Equals("Billing.*"));
            Assert.Equal(new BrickElementSelector(BrickElementKind.Type, "Billing.*", "Billing").GetHashCode(), selector.GetHashCode());
            Assert.True(selector == new BrickElementSelector(BrickElementKind.Type, "Billing.*", "Billing"));
            Assert.True(selector != new BrickElementSelector(BrickElementKind.Namespace, "Billing.*", "Billing"));
        }

        [Fact]
        public void BrickElementSelectorMatchesElementKindAssemblyAndPattern()
        {
            var element = Element("type:Billing.Domain.OrderPolicy", "OrderPolicy", "Billing", "Billing.Domain.OrderPolicy");

            Assert.True(new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.*", "Billing").Matches(element));
            Assert.True(new BrickElementSelector(BrickElementKind.Unknown, "*").Matches(element));
            Assert.True(new BrickElementSelector(BrickElementKind.Type, "OrderPolicy").Matches(element));
            Assert.False(new BrickElementSelector(BrickElementKind.Type, string.Empty).Matches(element));
            Assert.False(new BrickElementSelector(BrickElementKind.Member, "Billing.Domain.*", "Billing").Matches(element));
            Assert.False(new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.*", "Other").Matches(element));
            Assert.False(new BrickElementSelector(BrickElementKind.Type, "Other.*", "Billing").Matches(element));
            Assert.False(new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.*", "Billing").Matches(null));
        }

        [Fact]
        public void ProjectSuppressionMarksMatchingViolationWithoutDestroyingOriginal()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var suppression = new BrickSuppression(
                RuleId.From("BRK-001"),
                new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.*", "Billing"),
                "Known legacy exception.",
                "Billing Team",
                now.AddDays(1));

            var projected = BrickViolationStateProjector.Project(new[] { violation }, new[] { suppression }, null, now).Single();

            Assert.Equal(BrickViolationState.Active, violation.State);
            Assert.Equal(BrickViolationState.Suppressed, projected.State);
            Assert.Equal("Known legacy exception.", projected.StateReason);
            Assert.Equal(violation.Source, projected.Source);
            Assert.Equal(violation.Target, projected.Target);
        }

        [Fact]
        public void ProjectSuppressionMarksExpiredSuppression()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var suppression = new BrickSuppression(
                RuleId.From("BRK-001"),
                new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.*", "Billing"),
                "Known legacy exception.",
                expiresAt: now.AddTicks(-1));

            var projected = BrickViolationStateProjector.Project(new[] { violation }, new[] { suppression }, null, now).Single();

            Assert.Equal(BrickViolationState.ExpiredSuppression, projected.State);
            Assert.Contains("Suppression expired", projected.StateReason);
        }

        [Fact]
        public void ProjectBaselineMarksMatchingViolation()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var baseline = new BrickBaselineEntry(
                RuleId.From("BRK-001"),
                "Billing.Domain.*",
                "Billing.Infrastructure.*",
                "Accepted existing debt.",
                expiresAt: now.AddDays(1));

            var projected = BrickViolationStateProjector.Project(new[] { violation }, null, new[] { baseline }, now).Single();

            Assert.Equal(BrickViolationState.Baselined, projected.State);
            Assert.Equal("Accepted existing debt.", projected.StateReason);
        }

        [Fact]
        public void ProjectBaselineMatchesWildcardSourceAndExactTarget()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var baseline = new BrickBaselineEntry(
                RuleId.From("BRK-001"),
                "*",
                "SqlGateway",
                "Accepted existing debt.");

            var projected = BrickViolationStateProjector.Project(new[] { violation }, null, new[] { baseline }, now).Single();

            Assert.Equal(BrickViolationState.Baselined, projected.State);
        }

        [Fact]
        public void ProjectBaselineDoesNotMatchEmptyPatternOrMissingTarget()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var noTargetViolation = new BrickViolation(
                BrickViolationKind.RequiredDependency,
                violation.Source,
                "Missing dependency.",
                BrickSeverity.Error,
                BrickViolationState.Active,
                RuleId.From("BRK-001"),
                "Missing dependency");
            var emptyTargetPattern = new BrickBaselineEntry(RuleId.From("BRK-001"), "Billing.Domain.*", string.Empty, "Accepted existing debt.");
            var wildcardTargetPattern = new BrickBaselineEntry(RuleId.From("BRK-001"), "Billing.Domain.*", "*", "Accepted existing debt.");

            var emptyPatternProjection = BrickViolationStateProjector.Project(new[] { violation }, null, new[] { emptyTargetPattern }, now).Single();
            var missingTargetProjection = BrickViolationStateProjector.Project(new[] { noTargetViolation }, null, new[] { wildcardTargetPattern }, now).Single();

            Assert.Equal(BrickViolationState.Active, emptyPatternProjection.State);
            Assert.Equal(BrickViolationState.Active, missingTargetProjection.State);
        }

        [Fact]
        public void ProjectBaselineMarksExpiredBaseline()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var baseline = new BrickBaselineEntry(
                RuleId.From("BRK-001"),
                "Billing.Domain.*",
                "Billing.Infrastructure.*",
                "Accepted existing debt.",
                expiresAt: now.AddTicks(-1));

            var projected = BrickViolationStateProjector.Project(new[] { violation }, null, new[] { baseline }, now).Single();

            Assert.Equal(BrickViolationState.ExpiredBaseline, projected.State);
            Assert.Contains("Baseline expired", projected.StateReason);
        }

        [Fact]
        public void ProjectKeepsUnmatchedViolationsAndNormalizesNullCollections()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var suppression = new BrickSuppression(RuleId.From("BRK-002"), new BrickElementSelector(BrickElementKind.Type, "Other"), "Other");
            var baseline = new BrickBaselineEntry(RuleId.From("BRK-002"), "Other", "Other", "Other");

            var projected = BrickViolationStateProjector.Project(new[] { violation }, new[] { suppression }, new[] { baseline }, now).Single();

            Assert.Equal(BrickViolationState.Active, projected.State);
            Assert.Null(projected.StateReason);
            Assert.Empty(BrickViolationStateProjector.Project(null, null, null, now));
        }

        [Fact]
        public void ProjectSuppressionTakesPrecedenceOverBaseline()
        {
            var now = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
            var violation = Violation("BRK-001");
            var suppression = new BrickSuppression(
                RuleId.From("BRK-001"),
                new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.*", "Billing"),
                "Intentional exception.");
            var baseline = new BrickBaselineEntry(
                RuleId.From("BRK-001"),
                "Billing.Domain.*",
                "Billing.Infrastructure.*",
                "Accepted existing debt.");

            var projected = BrickViolationStateProjector.Project(new[] { violation }, new[] { suppression }, new[] { baseline }, now).Single();

            Assert.Equal(BrickViolationState.Suppressed, projected.State);
            Assert.Equal("Intentional exception.", projected.StateReason);
        }

        [Fact]
        public void BrickViolationCarriesStateReasonAndExpiredStates()
        {
            var element = new BrickElement(default, BrickElementKind.Type, "OrderPolicy");
            var suppressed = new BrickViolation(
                BrickViolationKind.Suppression,
                element,
                "Suppressed by policy.",
                BrickSeverity.Info,
                BrickViolationState.ExpiredSuppression,
                stateReason: "Suppression expired on 2026-10-01.");
            var baselined = new BrickViolation(
                BrickViolationKind.Baseline,
                element,
                "Baseline expired.",
                BrickSeverity.Warning,
                BrickViolationState.ExpiredBaseline,
                stateReason: "Baseline expired on 2026-12-31.");

            Assert.Equal(BrickViolationKind.Suppression, suppressed.Kind);
            Assert.Equal(BrickViolationState.ExpiredSuppression, suppressed.State);
            Assert.Equal("Suppression expired on 2026-10-01.", suppressed.StateReason);
            Assert.Equal(BrickViolationKind.Baseline, baselined.Kind);
            Assert.Equal(BrickViolationState.ExpiredBaseline, baselined.State);
            Assert.Equal("Baseline expired on 2026-12-31.", baselined.StateReason);
        }

        private static BrickViolation Violation(string ruleId)
        {
            var source = Element("type:Billing.Domain.OrderPolicy", "OrderPolicy", "Billing", "Billing.Domain.OrderPolicy");
            var target = Element("type:Billing.Infrastructure.SqlGateway", "SqlGateway", "Billing.Infrastructure", "Billing.Infrastructure.SqlGateway");

            return new BrickViolation(
                BrickViolationKind.DependencyRule,
                source,
                "Domain must not depend on infrastructure.",
                BrickSeverity.Error,
                BrickViolationState.Active,
                RuleId.From(ruleId),
                "No infrastructure",
                target,
                new[] { RoleId.From("Domain") },
                new[] { RoleId.From("Infrastructure") },
                BrickDependencyKindId.From("TypeReference"),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickEvidenceLevel.CompilerConfirmed);
        }

        private static BrickElement Element(string id, string displayName, string assemblyName, string fullName) =>
            new BrickElement(
                BrickElementId.From(id),
                BrickElementKind.Type,
                displayName,
                assemblyName: assemblyName,
                namespaceName: "Billing.Domain",
                fullName: fullName,
                origin: BrickElementOrigin.Source,
                source: BrickElementSource.Code);
    }
}
