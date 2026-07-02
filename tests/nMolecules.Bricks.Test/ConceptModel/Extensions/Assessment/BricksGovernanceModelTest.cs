using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test.ConceptModel
{
    public class BricksGovernanceModelTest
    {
        [Fact]
        public void GovernanceRequirementCapturesAreaRequirementAndRationale()
        {
            var requirement = new BrickGovernanceRequirement(
                "policy-owner",
                BrickGovernanceArea.PolicyOwnership,
                "Policy owner",
                true,
                "Every policy needs an accountable owner.");

            Assert.Equal("policy-owner", requirement.Id);
            Assert.Equal(BrickGovernanceArea.PolicyOwnership, requirement.Area);
            Assert.Equal("Policy owner", requirement.DisplayName);
            Assert.True(requirement.Required);
            Assert.Equal("Every policy needs an accountable owner.", requirement.Rationale);
        }

        [Fact]
        public void GovernanceRequirementNormalizesNullText()
        {
            var requirement = new BrickGovernanceRequirement(null, BrickGovernanceArea.ExceptionHandling, null, false, null);

            Assert.Equal(string.Empty, requirement.Id);
            Assert.Equal(string.Empty, requirement.DisplayName);
            Assert.False(requirement.Required);
            Assert.Equal(string.Empty, requirement.Rationale);
        }

        [Fact]
        public void GovernanceAreaDefinitionSortsRequirementsAndNormalizesText()
        {
            var later = Requirement("z", BrickGovernanceArea.PolicyOwnership, true);
            var earlier = Requirement("a", BrickGovernanceArea.PolicyOwnership, true);

            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.PolicyOwnership,
                null,
                null,
                new[] { later, null, earlier });

            Assert.Equal(BrickGovernanceArea.PolicyOwnership, definition.Area);
            Assert.Equal(string.Empty, definition.DisplayName);
            Assert.Equal(string.Empty, definition.Description);
            Assert.Equal(new[] { "a", "z" }, definition.Requirements.Select(requirement => requirement.Id).ToArray());
        }

        [Fact]
        public void GovernanceAreaDefinitionNormalizesNullRequirements()
        {
            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.CompatibilityExpectations,
                "Compatibility",
                "Compatibility expectations are explicit.",
                null);

            Assert.Empty(definition.Requirements);
        }

        [Fact]
        public void GovernanceRequirementResultCapturesStatusAndEvidence()
        {
            var requirement = Requirement("review-process", BrickGovernanceArea.PolicyOwnership, true);

            var result = new BrickGovernanceRequirementResult(
                requirement,
                BrickGovernanceRequirementStatus.Satisfied,
                "Changes are reviewed.");

            Assert.Equal(requirement, result.Requirement);
            Assert.Equal(BrickGovernanceRequirementStatus.Satisfied, result.Status);
            Assert.Equal("Changes are reviewed.", result.Evidence);
            Assert.True(result.SatisfiesRequirement);
        }

        [Fact]
        public void GovernanceRequirementResultNormalizesEvidenceAndRequiresRequirement()
        {
            var optional = Requirement("advisory-process", BrickGovernanceArea.RolePackEvolution, false);
            var result = new BrickGovernanceRequirementResult(optional, BrickGovernanceRequirementStatus.NotApplicable, null);

            Assert.Equal(string.Empty, result.Evidence);
            Assert.True(result.SatisfiesRequirement);
            Assert.Throws<ArgumentNullException>(() => new BrickGovernanceRequirementResult(null, BrickGovernanceRequirementStatus.Satisfied));
        }

        [Fact]
        public void BuiltInGovernanceAreasExposeV22OperationalGovernanceInStableOrder()
        {
            var areas = BrickBuiltInGovernanceAreas.All.ToArray();

            Assert.Equal(
                new[]
                {
                    BrickGovernanceArea.PolicyOwnership,
                    BrickGovernanceArea.ExceptionHandling,
                    BrickGovernanceArea.RolePackEvolution,
                    BrickGovernanceArea.CompatibilityExpectations
                },
                areas.Select(area => area.Area).ToArray());
            Assert.Contains(areas[0].Requirements, requirement => requirement.Id == "policy-owner");
            Assert.Contains(areas[1].Requirements, requirement => requirement.Id == "suppression-justification");
            Assert.Contains(areas[2].Requirements, requirement => requirement.Id == "role-pack-deprecation-path");
            Assert.Contains(areas[3].Requirements, requirement => requirement.Id == "stable-diagnostic-ids");
            Assert.All(areas, area => Assert.False(string.IsNullOrWhiteSpace(area.DisplayName)));
        }

        [Fact]
        public void GovernanceAssessmentReportsCompliantArea()
        {
            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.PolicyOwnership,
                "Policy ownership",
                "Policies have accountable owners.",
                new[] { Requirement("policy-owner", BrickGovernanceArea.PolicyOwnership, true), Requirement("advisory-review", BrickGovernanceArea.PolicyOwnership, false) });
            var required = definition.Requirements.Single(requirement => requirement.Id == "policy-owner");
            var optional = definition.Requirements.Single(requirement => requirement.Id == "advisory-review");

            var assessment = new BrickGovernanceAreaAssessment(
                definition,
                new[]
                {
                    Result(required, BrickGovernanceRequirementStatus.Satisfied),
                    Result(optional, BrickGovernanceRequirementStatus.NotApplicable)
                });

            Assert.Equal(BrickGovernanceAreaStatus.Compliant, assessment.Status);
            Assert.Equal(1, assessment.RequiredRequirementCount);
            Assert.Equal(1, assessment.SatisfiedRequiredRequirementCount);
            Assert.Equal(0, assessment.MissingRequiredRequirementCount);
            Assert.Equal(1.0, assessment.ComplianceRatio);
        }

        [Fact]
        public void GovernanceAssessmentReportsPartialArea()
        {
            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.ExceptionHandling,
                "Exception handling",
                "Exceptions are intentional.",
                new[] { Requirement("suppression-justification", BrickGovernanceArea.ExceptionHandling, true), Requirement("suppression-owner", BrickGovernanceArea.ExceptionHandling, true) });

            var assessment = new BrickGovernanceAreaAssessment(
                definition,
                new[] { Result(definition.Requirements[0], BrickGovernanceRequirementStatus.Satisfied) });

            Assert.Equal(BrickGovernanceAreaStatus.Partial, assessment.Status);
            Assert.Equal(2, assessment.RequiredRequirementCount);
            Assert.Equal(1, assessment.SatisfiedRequiredRequirementCount);
            Assert.Equal(1, assessment.MissingRequiredRequirementCount);
            Assert.Equal(0.5, assessment.ComplianceRatio);
        }

        [Fact]
        public void GovernanceAssessmentReportsNonCompliantArea()
        {
            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.CompatibilityExpectations,
                "Compatibility",
                "Compatibility expectations are explicit.",
                new[] { Requirement("stable-diagnostic-ids", BrickGovernanceArea.CompatibilityExpectations, true) });

            var assessment = new BrickGovernanceAreaAssessment(
                definition,
                new[] { Result(definition.Requirements[0], BrickGovernanceRequirementStatus.Missing) });

            Assert.Equal(BrickGovernanceAreaStatus.NonCompliant, assessment.Status);
            Assert.Equal(0, assessment.SatisfiedRequiredRequirementCount);
            Assert.Equal(1, assessment.MissingRequiredRequirementCount);
            Assert.Equal(0, assessment.ComplianceRatio);
        }

        [Fact]
        public void GovernanceAssessmentIgnoresNullResultsAndHandlesNoRequiredRequirements()
        {
            var optional = Requirement("advisory-process", BrickGovernanceArea.RolePackEvolution, false);
            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.RolePackEvolution,
                "Role-pack evolution",
                "Optional helper governance.",
                new[] { optional });

            var assessment = new BrickGovernanceAreaAssessment(
                definition,
                new BrickGovernanceRequirementResult[] { null, Result(optional, BrickGovernanceRequirementStatus.NotApplicable) });

            Assert.Single(assessment.Results);
            Assert.Equal(0, assessment.RequiredRequirementCount);
            Assert.Equal(0, assessment.MissingRequiredRequirementCount);
            Assert.Equal(1.0, assessment.ComplianceRatio);
            Assert.Equal(BrickGovernanceAreaStatus.Compliant, assessment.Status);
        }

        [Fact]
        public void GovernanceAssessmentNormalizesNullResultsAndRequiresDefinition()
        {
            var definition = new BrickGovernanceAreaDefinition(
                BrickGovernanceArea.PolicyOwnership,
                "Policy ownership",
                "Policies have accountable owners.",
                new[] { Requirement("policy-owner", BrickGovernanceArea.PolicyOwnership, true) });

            var assessment = new BrickGovernanceAreaAssessment(definition, null);

            Assert.Empty(assessment.Results);
            Assert.Equal(BrickGovernanceAreaStatus.NonCompliant, assessment.Status);
            Assert.Throws<ArgumentNullException>(() => new BrickGovernanceAreaAssessment(null, null));
        }

        [Fact]
        public void GovernanceSummaryCountsAreasAndMissingRequirements()
        {
            var compliant = Assessment(BrickGovernanceArea.PolicyOwnership, BrickGovernanceAreaStatus.Compliant);
            var partial = Assessment(BrickGovernanceArea.ExceptionHandling, BrickGovernanceAreaStatus.Partial);
            var nonCompliant = Assessment(BrickGovernanceArea.RolePackEvolution, BrickGovernanceAreaStatus.NonCompliant);

            var summary = BrickGovernanceSummary.FromAssessments(new[] { partial, nonCompliant, compliant });

            Assert.Equal(3, summary.TotalAreas);
            Assert.Equal(1, summary.CompliantAreas);
            Assert.Equal(1, summary.PartialAreas);
            Assert.Equal(1, summary.NonCompliantAreas);
            Assert.Equal(2, summary.MissingRequiredRequirements);
            Assert.False(summary.IsFullyCompliant);
        }

        [Fact]
        public void GovernanceSummaryHandlesNullAndFullyCompliantAssessments()
        {
            var empty = BrickGovernanceSummary.FromAssessments(null);
            var compliant = BrickGovernanceSummary.FromAssessments(new[] { Assessment(BrickGovernanceArea.PolicyOwnership, BrickGovernanceAreaStatus.Compliant) });

            Assert.Equal(0, empty.TotalAreas);
            Assert.False(empty.IsFullyCompliant);
            Assert.True(compliant.IsFullyCompliant);
        }

        [Fact]
        public void GovernanceReportSortsAssessmentsAndUsesCurrentSchema()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 14, 0, 0, TimeSpan.Zero);
            var report = new BrickGovernanceReport(
                generatedAt,
                new[]
                {
                    Assessment(BrickGovernanceArea.CompatibilityExpectations, BrickGovernanceAreaStatus.NonCompliant),
                    Assessment(BrickGovernanceArea.PolicyOwnership, BrickGovernanceAreaStatus.Compliant)
                });

            Assert.Equal(BrickGovernanceReport.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Equal(
                new[] { BrickGovernanceArea.PolicyOwnership, BrickGovernanceArea.CompatibilityExpectations },
                report.Assessments.Select(assessment => assessment.Definition.Area).ToArray());
            Assert.Equal(2, report.Summary.TotalAreas);
        }

        [Fact]
        public void GovernanceReportNormalizesNullAssessmentsAndSchema()
        {
            var report = new BrickGovernanceReport(DateTimeOffset.UnixEpoch, null, null);

            Assert.Empty(report.Assessments);
            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Equal(0, report.Summary.TotalAreas);
        }

        [Fact]
        public void GovernanceJsonSerializerWritesVersionedMachineReadableReport()
        {
            var report = new BrickGovernanceReport(
                new DateTimeOffset(2026, 6, 23, 14, 0, 0, TimeSpan.Zero),
                new[]
                {
                    Assessment(BrickGovernanceArea.ExceptionHandling, BrickGovernanceAreaStatus.Partial),
                    Assessment(BrickGovernanceArea.PolicyOwnership, BrickGovernanceAreaStatus.Compliant)
                });

            var json = BrickGovernanceReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var assessments = root.GetProperty("assessments");

            Assert.Equal(BrickGovernanceReport.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(2, root.GetProperty("summary").GetProperty("totalAreas").GetInt32());
            Assert.False(root.GetProperty("summary").GetProperty("isFullyCompliant").GetBoolean());
            Assert.Equal("PolicyOwnership", assessments[0].GetProperty("area").GetString());
            Assert.Equal("Compliant", assessments[0].GetProperty("status").GetString());
            Assert.Equal("ExceptionHandling", assessments[1].GetProperty("area").GetString());
            Assert.Equal("Partial", assessments[1].GetProperty("status").GetString());
            Assert.Equal("requirement", assessments[1].GetProperty("results")[0].GetProperty("requirementId").GetString());
        }

        [Fact]
        public void GovernanceJsonSerializerOmitsEmptyEvidence()
        {
            var requirement = Requirement("policy-owner", BrickGovernanceArea.PolicyOwnership, true);
            var definition = new BrickGovernanceAreaDefinition(BrickGovernanceArea.PolicyOwnership, "Policy", "Policy", new[] { requirement });
            var report = new BrickGovernanceReport(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickGovernanceAreaAssessment(
                        definition,
                        new[] { new BrickGovernanceRequirementResult(requirement, BrickGovernanceRequirementStatus.Missing, null) })
                });

            var json = BrickGovernanceReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);
            var result = document.RootElement.GetProperty("assessments")[0].GetProperty("results")[0];

            Assert.False(result.TryGetProperty("evidence", out _));
        }

        [Fact]
        public void GovernanceJsonSerializerRequiresReport()
        {
            Assert.Throws<ArgumentNullException>(() => BrickGovernanceReportJsonSerializer.Serialize(null));
        }

        private static BrickGovernanceRequirement Requirement(string id, BrickGovernanceArea area, bool required) =>
            new BrickGovernanceRequirement(id, area, id, required, id);

        private static BrickGovernanceRequirementResult Result(
            BrickGovernanceRequirement requirement,
            BrickGovernanceRequirementStatus status) =>
            new BrickGovernanceRequirementResult(requirement, status, status.ToString());

        private static BrickGovernanceAreaAssessment Assessment(
            BrickGovernanceArea area,
            BrickGovernanceAreaStatus status)
        {
            var requirement = Requirement("requirement", area, true);
            var secondRequirement = Requirement("second-requirement", area, true);

            if (status == BrickGovernanceAreaStatus.Partial)
            {
                var partialDefinition = new BrickGovernanceAreaDefinition(area, area.ToString(), area.ToString(), new[] { requirement, secondRequirement });
                return new BrickGovernanceAreaAssessment(
                    partialDefinition,
                    new[]
                    {
                        Result(requirement, BrickGovernanceRequirementStatus.Satisfied),
                        Result(secondRequirement, BrickGovernanceRequirementStatus.Missing)
                    });
            }

            var definition = new BrickGovernanceAreaDefinition(area, area.ToString(), area.ToString(), new[] { requirement });
            var resultStatus = status == BrickGovernanceAreaStatus.Compliant
                ? BrickGovernanceRequirementStatus.Satisfied
                : BrickGovernanceRequirementStatus.Missing;

            return new BrickGovernanceAreaAssessment(definition, new[] { Result(requirement, resultStatus) });
        }
    }
}
