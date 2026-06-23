using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test.ConceptModel
{
    public class BricksConformanceModelTest
    {
        [Fact]
        public void ConformanceCapabilityCapturesIdentityRequirementAndRationale()
        {
            var capability = new BrickConformanceCapability(
                "role-attributes",
                "Role attributes",
                true,
                "Roles can be expressed in code.");

            Assert.Equal("role-attributes", capability.Id);
            Assert.Equal("Role attributes", capability.DisplayName);
            Assert.True(capability.Required);
            Assert.Equal("Roles can be expressed in code.", capability.Rationale);
        }

        [Fact]
        public void ConformanceCapabilityNormalizesNullText()
        {
            var capability = new BrickConformanceCapability(null, null, false, null);

            Assert.Equal(string.Empty, capability.Id);
            Assert.Equal(string.Empty, capability.DisplayName);
            Assert.False(capability.Required);
            Assert.Equal(string.Empty, capability.Rationale);
        }

        [Fact]
        public void ConformanceLevelDefinitionSortsCapabilitiesAndNormalizesText()
        {
            var second = Capability("b", true);
            var first = Capability("a", true);

            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.StaticValidation,
                null,
                null,
                new[] { second, null, first });

            Assert.Equal(BrickConformanceLevel.StaticValidation, definition.Level);
            Assert.Equal(string.Empty, definition.DisplayName);
            Assert.Equal(string.Empty, definition.Description);
            Assert.Equal(new[] { "a", "b" }, definition.Capabilities.Select(capability => capability.Id).ToArray());
        }

        [Fact]
        public void ConformanceLevelDefinitionNormalizesNullCapabilities()
        {
            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.Marking,
                "Marking",
                "Roles can be marked.",
                null);

            Assert.Empty(definition.Capabilities);
        }

        [Fact]
        public void ConformanceCapabilityResultCapturesStatusAndEvidence()
        {
            var capability = Capability("role-attributes", true);

            var result = new BrickConformanceCapabilityResult(
                capability,
                BrickConformanceCapabilityStatus.Satisfied,
                "Attributes are shipped.");

            Assert.Equal(capability, result.Capability);
            Assert.Equal(BrickConformanceCapabilityStatus.Satisfied, result.Status);
            Assert.Equal("Attributes are shipped.", result.Evidence);
            Assert.True(result.SatisfiesRequirement);
        }

        [Fact]
        public void ConformanceCapabilityResultNormalizesEvidenceAndRequiresCapability()
        {
            var optional = Capability("ide-visualisation", false);
            var result = new BrickConformanceCapabilityResult(optional, BrickConformanceCapabilityStatus.NotApplicable, null);

            Assert.Equal(string.Empty, result.Evidence);
            Assert.True(result.SatisfiesRequirement);
            Assert.Throws<ArgumentNullException>(() => new BrickConformanceCapabilityResult(null, BrickConformanceCapabilityStatus.Satisfied));
        }

        [Fact]
        public void BuiltInConformanceLevelsExposeV22ProgressionInStableOrder()
        {
            var levels = BrickBuiltInConformanceLevels.All.ToArray();

            Assert.Equal(
                new[]
                {
                    BrickConformanceLevel.Marking,
                    BrickConformanceLevel.StaticValidation,
                    BrickConformanceLevel.Explainability,
                    BrickConformanceLevel.PolicyFiles,
                    BrickConformanceLevel.RuntimeAwareAnalysis,
                    BrickConformanceLevel.IntegrationAndAugmentation
                },
                levels.Select(level => level.Level).ToArray());

            Assert.Equal(new[] { "alias-attributes", "role-attributes", "typed-identifiers" }, levels[0].Capabilities.Select(capability => capability.Id).ToArray());
            Assert.Contains(levels[1].Capabilities, capability => capability.Id == "analyzer-diagnostics");
            Assert.Contains(levels[2].Capabilities, capability => capability.Id == "resolution-traces");
            Assert.Contains(levels[3].Capabilities, capability => capability.Id == "schema-versioned-policy-files");
            Assert.Contains(levels[4].Capabilities, capability => capability.Id == "reflection-access");
            Assert.Contains(levels[5].Capabilities, capability => capability.Id == "report-generation");
            Assert.All(levels, level => Assert.False(string.IsNullOrWhiteSpace(level.DisplayName)));
        }

        [Fact]
        public void ConformanceAssessmentReportsAchievedLevel()
        {
            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.Marking,
                "Marking",
                "Roles can be expressed.",
                new[] { Capability("role-attributes", true), Capability("ide-visualisation", false) });
            var required = definition.Capabilities.Single(capability => capability.Id == "role-attributes");
            var optional = definition.Capabilities.Single(capability => capability.Id == "ide-visualisation");
            var results = new[]
            {
                Result(required, BrickConformanceCapabilityStatus.Satisfied),
                Result(optional, BrickConformanceCapabilityStatus.NotApplicable)
            };

            var assessment = new BrickConformanceLevelAssessment(definition, results);

            Assert.Equal(BrickConformanceLevelStatus.Achieved, assessment.Status);
            Assert.Equal(1, assessment.RequiredCapabilityCount);
            Assert.Equal(1, assessment.SatisfiedRequiredCapabilityCount);
            Assert.Equal(0, assessment.MissingRequiredCapabilityCount);
            Assert.Equal(1.0, assessment.CompletionRatio);
        }

        [Fact]
        public void ConformanceAssessmentReportsPartialLevelAndMissingRequiredCapabilities()
        {
            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.StaticValidation,
                "Static Validation",
                "Static compiler-visible dependencies are analysed.",
                new[] { Capability("type-references", true), Capability("object-creation", true) });
            var results = new[]
            {
                Result(definition.Capabilities[0], BrickConformanceCapabilityStatus.Satisfied)
            };

            var assessment = new BrickConformanceLevelAssessment(definition, results);

            Assert.Equal(BrickConformanceLevelStatus.Partial, assessment.Status);
            Assert.Equal(2, assessment.RequiredCapabilityCount);
            Assert.Equal(1, assessment.SatisfiedRequiredCapabilityCount);
            Assert.Equal(1, assessment.MissingRequiredCapabilityCount);
            Assert.Equal(0.5, assessment.CompletionRatio);
        }

        [Fact]
        public void ConformanceAssessmentReportsNotStartedWhenNoRequiredCapabilityIsSatisfied()
        {
            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.RuntimeAwareAnalysis,
                "Runtime-aware",
                "Runtime relevant dependencies become analysable.",
                new[] { Capability("reflection-access", true) });
            var result = Result(definition.Capabilities[0], BrickConformanceCapabilityStatus.Missing);

            var assessment = new BrickConformanceLevelAssessment(definition, new[] { result });

            Assert.Equal(BrickConformanceLevelStatus.NotStarted, assessment.Status);
            Assert.Equal(0, assessment.SatisfiedRequiredCapabilityCount);
            Assert.Equal(1, assessment.MissingRequiredCapabilityCount);
            Assert.Equal(0, assessment.CompletionRatio);
        }

        [Fact]
        public void ConformanceAssessmentNormalizesNullResultsAndRequiresDefinition()
        {
            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.PolicyFiles,
                "Policy Files",
                "External policy configuration becomes supported.",
                new[] { Capability("policy-imports", true) });

            var assessment = new BrickConformanceLevelAssessment(definition, null);

            Assert.Empty(assessment.Results);
            Assert.Equal(BrickConformanceLevelStatus.NotStarted, assessment.Status);
            Assert.Throws<ArgumentNullException>(() => new BrickConformanceLevelAssessment(null, null));
        }

        [Fact]
        public void ConformanceAssessmentIgnoresNullResultsAndHandlesNoRequiredCapabilities()
        {
            var optional = Capability("ide-visualisation", false);
            var definition = new BrickConformanceLevelDefinition(
                BrickConformanceLevel.Explainability,
                "Explainability",
                "Optional helper capabilities only.",
                new[] { optional });

            var assessment = new BrickConformanceLevelAssessment(
                definition,
                new BrickConformanceCapabilityResult[] { null, Result(optional, BrickConformanceCapabilityStatus.NotApplicable) });

            Assert.Single(assessment.Results);
            Assert.Equal(0, assessment.RequiredCapabilityCount);
            Assert.Equal(0, assessment.MissingRequiredCapabilityCount);
            Assert.Equal(1.0, assessment.CompletionRatio);
            Assert.Equal(BrickConformanceLevelStatus.Achieved, assessment.Status);
        }

        [Fact]
        public void ConformanceSummaryCountsAssessmentsAndHighestContiguousAchievedLevel()
        {
            var achieved = Assessment(BrickConformanceLevel.Marking, BrickConformanceLevelStatus.Achieved);
            var partial = Assessment(BrickConformanceLevel.StaticValidation, BrickConformanceLevelStatus.Partial);
            var notStarted = Assessment(BrickConformanceLevel.Explainability, BrickConformanceLevelStatus.NotStarted);

            var summary = BrickConformanceSummary.FromAssessments(new[] { partial, notStarted, achieved });

            Assert.Equal(3, summary.TotalLevels);
            Assert.Equal(1, summary.AchievedLevels);
            Assert.Equal(1, summary.PartialLevels);
            Assert.Equal(1, summary.NotStartedLevels);
            Assert.Equal(2, summary.MissingRequiredCapabilities);
            Assert.Equal(BrickConformanceLevel.Marking, summary.HighestContiguousAchievedLevel);
        }

        [Fact]
        public void ConformanceSummaryHandlesNullOrNoAchievedAssessments()
        {
            var empty = BrickConformanceSummary.FromAssessments(null);
            var none = BrickConformanceSummary.FromAssessments(new[] { Assessment(BrickConformanceLevel.Marking, BrickConformanceLevelStatus.NotStarted) });

            Assert.Equal(0, empty.TotalLevels);
            Assert.Null(empty.HighestContiguousAchievedLevel);
            Assert.Null(none.HighestContiguousAchievedLevel);
        }

        [Fact]
        public void ConformanceReportSortsAssessmentsAndUsesCurrentSchema()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
            var report = new BrickConformanceReport(
                generatedAt,
                new[]
                {
                    Assessment(BrickConformanceLevel.PolicyFiles, BrickConformanceLevelStatus.NotStarted),
                    Assessment(BrickConformanceLevel.Marking, BrickConformanceLevelStatus.Achieved)
                });

            Assert.Equal(BrickConformanceReport.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Equal(
                new[] { BrickConformanceLevel.Marking, BrickConformanceLevel.PolicyFiles },
                report.Assessments.Select(assessment => assessment.Definition.Level).ToArray());
            Assert.Equal(2, report.Summary.TotalLevels);
        }

        [Fact]
        public void ConformanceReportNormalizesNullAssessmentsAndSchema()
        {
            var report = new BrickConformanceReport(DateTimeOffset.UnixEpoch, null, null);

            Assert.Empty(report.Assessments);
            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Equal(0, report.Summary.TotalLevels);
        }

        [Fact]
        public void ConformanceJsonSerializerWritesVersionedMachineReadableReport()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
            var report = new BrickConformanceReport(
                generatedAt,
                new[]
                {
                    Assessment(BrickConformanceLevel.StaticValidation, BrickConformanceLevelStatus.Partial),
                    Assessment(BrickConformanceLevel.Marking, BrickConformanceLevelStatus.Achieved)
                });

            var json = BrickConformanceReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var assessments = root.GetProperty("assessments");

            Assert.Equal(BrickConformanceReport.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(2, root.GetProperty("summary").GetProperty("totalLevels").GetInt32());
            Assert.Equal("Marking", root.GetProperty("summary").GetProperty("highestContiguousAchievedLevel").GetString());
            Assert.Equal("Marking", assessments[0].GetProperty("level").GetString());
            Assert.Equal("Achieved", assessments[0].GetProperty("status").GetString());
            Assert.Equal("StaticValidation", assessments[1].GetProperty("level").GetString());
            Assert.Equal("Partial", assessments[1].GetProperty("status").GetString());
            Assert.Equal("capability", assessments[1].GetProperty("results")[0].GetProperty("capabilityId").GetString());
        }

        [Fact]
        public void ConformanceJsonSerializerOmitsMissingHighestContiguousLevel()
        {
            var report = new BrickConformanceReport(
                DateTimeOffset.UnixEpoch,
                new[] { Assessment(BrickConformanceLevel.Marking, BrickConformanceLevelStatus.NotStarted) });

            var json = BrickConformanceReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);

            Assert.False(document.RootElement
                .GetProperty("summary")
                .TryGetProperty("highestContiguousAchievedLevel", out _));
        }

        [Fact]
        public void ConformanceJsonSerializerRequiresReport()
        {
            Assert.Throws<ArgumentNullException>(() => BrickConformanceReportJsonSerializer.Serialize(null));
        }

        private static BrickConformanceCapability Capability(string id, bool required) =>
            new BrickConformanceCapability(id, id, required, id);

        private static BrickConformanceCapabilityResult Result(
            BrickConformanceCapability capability,
            BrickConformanceCapabilityStatus status) =>
            new BrickConformanceCapabilityResult(capability, status, status.ToString());

        private static BrickConformanceLevelAssessment Assessment(
            BrickConformanceLevel level,
            BrickConformanceLevelStatus status)
        {
            var capability = Capability("capability", true);
            var secondCapability = Capability("second-capability", true);

            if (status == BrickConformanceLevelStatus.Partial)
            {
                var partialDefinition = new BrickConformanceLevelDefinition(level, level.ToString(), level.ToString(), new[] { capability, secondCapability });
                return new BrickConformanceLevelAssessment(
                    partialDefinition,
                    new[]
                    {
                        Result(capability, BrickConformanceCapabilityStatus.Satisfied),
                        Result(secondCapability, BrickConformanceCapabilityStatus.Missing)
                    });
            }

            var definition = new BrickConformanceLevelDefinition(level, level.ToString(), level.ToString(), new[] { capability });
            var resultStatus = status == BrickConformanceLevelStatus.Achieved
                ? BrickConformanceCapabilityStatus.Satisfied
                : BrickConformanceCapabilityStatus.Missing;
            var results = new[] { Result(capability, resultStatus) };

            return new BrickConformanceLevelAssessment(definition, results);
        }
    }
}
