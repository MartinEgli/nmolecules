using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksAiAssistedEnforcementModelTest
    {
        [Fact]
        public void AiViolationCommentRecordsDeterministicViolationAndRemediation()
        {
            var violation = Violation();
            var option = new BrickRemediationOption(
                "IntroduceContract",
                BrickRemediationKind.IntroduceContract,
                "Introduce IOrderRepository.",
                "Use when application code needs persistence.",
                BrickRemediationRisk.Low,
                true);

            var comment = new BrickAiViolationComment(
                violation,
                "Application depends on infrastructure.",
                "Keeps orchestration independent from persistence implementation.",
                new[] { option },
                option,
                new[] { "Prefer constructor injection." },
                "Suppress only with owner and expiry.");

            Assert.Equal(RuleId.From("XMoleculesBricks0001"), comment.RuleId);
            Assert.Same(violation, comment.Violation);
            Assert.Equal("No infrastructure dependency", comment.RuleName);
            Assert.Equal(BrickDecision.Deny, comment.Decision);
            Assert.Equal(BrickSeverity.Error, comment.Severity);
            Assert.Equal(BrickEvidenceLevel.CompilerConfirmed, comment.EvidenceLevel);
            Assert.Equal("Application depends on infrastructure.", comment.ProblemSummary);
            Assert.Equal("Keeps orchestration independent from persistence implementation.", comment.ArchitecturalReason);
            Assert.Same(option, comment.Options.Single());
            Assert.Same(option, comment.RecommendedOption);
            Assert.Equal("Prefer constructor injection.", comment.AiRepairHints.Single());
            Assert.Equal("Suppress only with owner and expiry.", comment.SuppressionGuidance);
            Assert.True(comment.IsTraceableToDeterministicEvidence);
        }

        [Fact]
        public void AiViolationCommentNormalizesOptionalTextCollectionsAndRecommendation()
        {
            var option = new BrickRemediationOption(
                null,
                BrickRemediationKind.AddSuppression,
                null,
                null,
                BrickRemediationRisk.High,
                false);
            var comment = new BrickAiViolationComment(Violation(), null, null, new[] { option }, null, null, null);

            Assert.Equal(string.Empty, comment.ProblemSummary);
            Assert.Equal(string.Empty, comment.ArchitecturalReason);
            Assert.Equal(string.Empty, comment.SuppressionGuidance);
            Assert.Empty(comment.AiRepairHints);
            Assert.Null(comment.RecommendedOption);
            Assert.Equal(string.Empty, option.Id);
            Assert.Equal(string.Empty, option.Description);
            Assert.Equal(string.Empty, option.WhenToUse);
        }

        [Fact]
        public void AiViolationCommentRequiresViolationAndRecommendedOptionMustBelongToOptions()
        {
            var option = Option("IntroduceContract", BrickRemediationKind.IntroduceContract, BrickRemediationRisk.Low, true);
            var outside = Option("MoveElement", BrickRemediationKind.MoveElement, BrickRemediationRisk.Medium, false);

            Assert.Throws<ArgumentNullException>(() => new BrickAiViolationComment(null, "problem", "reason", null, null, null, null));
            Assert.Throws<ArgumentException>(() => new BrickAiViolationComment(Violation(), "problem", "reason", new[] { option }, outside, null, null));
        }

        [Fact]
        public void AiViolationCommentHandlesRequiredDependencyAndUnknownEvidence()
        {
            var source = new BrickElement(BrickElementId.From("type:Sales.Application.OrderService"), BrickElementKind.Type, "OrderService");
            var violation = new BrickViolation(
                BrickViolationKind.RequiredDependency,
                source,
                "Missing dependency.",
                BrickSeverity.Warning,
                BrickViolationState.Active,
                ruleName: null,
                evidenceLevel: BrickEvidenceLevel.Unknown);

            var comment = new BrickAiViolationComment(
                violation,
                "Missing required dependency.",
                "The expected collaboration is absent.",
                null,
                null,
                new string[] { null, "Keep the violation visible." },
                null);

            Assert.Null(comment.RuleId);
            Assert.Equal(string.Empty, comment.RuleName);
            Assert.Equal(BrickDecision.Require, comment.Decision);
            Assert.False(comment.IsTraceableToDeterministicEvidence);
            Assert.Equal("Keep the violation visible.", comment.AiRepairHints.Single());
        }

        [Fact]
        public void AiCommentDocumentCarriesSchemaAndSerializerWritesAiReadyShape()
        {
            var comment = new BrickAiViolationComment(
                Violation(),
                "Application layer depends directly on infrastructure implementation.",
                "This weakens testability.",
                new[]
                {
                    Option("IntroduceContract", BrickRemediationKind.IntroduceContract, BrickRemediationRisk.Low, true),
                    Option("SuppressRule", BrickRemediationKind.AddSuppression, BrickRemediationRisk.High, false)
                },
                Option("IntroduceContract", BrickRemediationKind.IntroduceContract, BrickRemediationRisk.Low, true),
                new[] { "Do not suppress automatically.", "Reuse existing abstractions." },
                "Only suppress when intentional and time-bounded.");

            var document = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, new[] { comment });

            using var json = JsonDocument.Parse(BrickAiCommentJsonSerializer.Serialize(document));
            var root = json.RootElement;
            var firstComment = root.GetProperty("comments")[0];
            var options = firstComment.GetProperty("repairOptions").EnumerateArray().ToArray();

            Assert.Equal(BrickAiCommentDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Equal(BrickAiCommentDocument.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal("XMoleculesBricks0001", firstComment.GetProperty("ruleId").GetString());
            Assert.Equal("Deny", firstComment.GetProperty("decision").GetString());
            Assert.Equal("CompilerConfirmed", firstComment.GetProperty("evidenceLevel").GetString());
            Assert.Equal("Sales.Application.OrderService", firstComment.GetProperty("source").GetProperty("name").GetString());
            Assert.Equal("Architecture.Layer.Application", firstComment.GetProperty("source").GetProperty("roles")[0].GetString());
            Assert.Equal("Sales.Infrastructure.SqlOrderRepository", firstComment.GetProperty("target").GetProperty("name").GetString());
            Assert.Equal("ObjectCreation", firstComment.GetProperty("dependency").GetProperty("kind").GetString());
            Assert.Equal("Static", firstComment.GetProperty("dependency").GetProperty("layer").GetString());
            Assert.Equal("IntroduceContract", firstComment.GetProperty("recommendedOption").GetString());
            Assert.Equal("IntroduceContract", options[0].GetProperty("id").GetString());
            Assert.True(options[0].GetProperty("preferred").GetBoolean());
            Assert.Equal("SuppressRule", options[1].GetProperty("id").GetString());
            Assert.Equal("Do not suppress automatically.", firstComment.GetProperty("aiRepairHints")[0].GetString());
            Assert.Throws<ArgumentNullException>(() => BrickAiCommentJsonSerializer.Serialize(null));
        }

        [Fact]
        public void AiCommentDocumentNormalizesSchemaCommentsAndSortsBySource()
        {
            var empty = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, null, null);
            var later = new BrickAiViolationComment(Violation("z"), "problem", "reason", null, null, null, null);
            var earlier = new BrickAiViolationComment(Violation("a"), "problem", "reason", null, null, null, null);

            var sorted = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, new[] { later, earlier });

            Assert.Equal(string.Empty, empty.Schema);
            Assert.False(empty.IsCurrentSchema);
            Assert.Empty(empty.Comments);
            Assert.Equal("type:a", sorted.Comments[0].Violation.Source.Id.Value);
            Assert.Equal("type:z", sorted.Comments[1].Violation.Source.Id.Value);
        }

        [Fact]
        public void AiCommentJsonSerializerOmitsMissingTargetDependencyAndRecommendation()
        {
            var source = new BrickElement(
                BrickElementId.From("type:Sales.Application.OrderService"),
                BrickElementKind.Type,
                "OrderService",
                fullName: "Sales.Application.OrderService");
            var violation = new BrickViolation(
                BrickViolationKind.RequiredDependency,
                source,
                "Missing contract.",
                BrickSeverity.Warning,
                BrickViolationState.Active,
                RuleId.From("XMoleculesBricks0002"),
                "Missing contract",
                resolvedSourceRoles: new[] { RoleId.From("Architecture.Layer.Application") });
            var comment = new BrickAiViolationComment(violation, "problem", "reason", null, null, null, null);
            var document = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, new[] { comment });

            using var json = JsonDocument.Parse(BrickAiCommentJsonSerializer.Serialize(document));
            var firstComment = json.RootElement.GetProperty("comments")[0];

            Assert.Equal("Sales.Application.OrderService", firstComment.GetProperty("source").GetProperty("name").GetString());
            Assert.False(firstComment.TryGetProperty("target", out _));
            Assert.False(firstComment.TryGetProperty("dependency", out _));
            Assert.False(firstComment.TryGetProperty("recommendedOption", out _));
        }

        [Fact]
        public void AiCommentJsonSerializerWritesNullRuleIdAndPartialDependency()
        {
            var source = new BrickElement(BrickElementId.From("type:source"), BrickElementKind.Type, "Source");
            var target = new BrickElement(BrickElementId.From("type:target"), BrickElementKind.Type, "Target");
            var violation = new BrickViolation(
                BrickViolationKind.DependencyRule,
                source,
                "message",
                BrickSeverity.Info,
                BrickViolationState.Active,
                target: target,
                dependencyKindId: BrickDependencyKindId.From("uses"));
            var comment = new BrickAiViolationComment(violation, "problem", "reason", null, null, null, null);
            var document = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, new[] { comment });

            using var json = JsonDocument.Parse(BrickAiCommentJsonSerializer.Serialize(document));
            var firstComment = json.RootElement.GetProperty("comments")[0];

            Assert.False(firstComment.TryGetProperty("ruleId", out _));
            Assert.Equal("uses", firstComment.GetProperty("dependency").GetProperty("kind").GetString());
            Assert.False(firstComment.GetProperty("dependency").TryGetProperty("layer", out _));
        }

        [Fact]
        public void AiCommentJsonSerializerWritesDependencyLayerWithoutKind()
        {
            var source = new BrickElement(BrickElementId.From("type:source"), BrickElementKind.Type, "Source");
            var target = new BrickElement(BrickElementId.From("type:target"), BrickElementKind.Type, "Target");
            var violation = new BrickViolation(
                BrickViolationKind.DependencyRule,
                source,
                "message",
                BrickSeverity.Info,
                BrickViolationState.Active,
                target: target,
                dependencyLayer: BrickDependencyLayer.Runtime);
            var comment = new BrickAiViolationComment(violation, "problem", "reason", null, null, null, null);
            var document = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, new[] { comment });

            using var json = JsonDocument.Parse(BrickAiCommentJsonSerializer.Serialize(document));
            var dependency = json.RootElement.GetProperty("comments")[0].GetProperty("dependency");

            Assert.False(dependency.TryGetProperty("kind", out _));
            Assert.Equal("Runtime", dependency.GetProperty("layer").GetString());
        }

        [Fact]
        public void AiCommentDocumentSortsCommentsWithoutRuleIds()
        {
            var z = new BrickAiViolationComment(WithoutRuleIdViolation("z"), "problem", "reason", null, null, null, null);
            var a = new BrickAiViolationComment(WithoutRuleIdViolation("a"), "problem", "reason", null, null, null, null);

            var document = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, new[] { z, a });

            Assert.Equal("type:a", document.Comments[0].Violation.Source.Id.Value);
            Assert.Equal("type:z", document.Comments[1].Violation.Source.Id.Value);
        }

        [Fact]
        public void RuleProposalRecordsAdvisoryRuleCandidateWithEvidence()
        {
            var evidence = new BrickRuleProposalEvidence(
                "Application repeatedly creates infrastructure repositories.",
                new[] { "OrderService -> SqlOrderRepository" },
                new[] { "CompositionRoot -> SqlOrderRepository" },
                new[] { "Generated DI wiring may look similar." },
                new[] { "Sales.Application", "Sales.Infrastructure" },
                "Move wiring through a contract.");
            var proposal = new BrickRuleProposal(
                "proposal-001",
                "No application to infrastructure implementation dependency",
                "Keep application independent from infrastructure.",
                BrickRoleSelector.From("Architecture.Layer.Application"),
                BrickRoleSelector.From("Architecture.Layer.Infrastructure"),
                BrickDependencyKindId.From("ObjectCreation"),
                BrickDecision.Deny,
                BrickSeverity.Warning,
                evidence,
                BrickRuleLifecycleState.Candidate);

            Assert.Equal("proposal-001", proposal.ProposalId);
            Assert.Equal("No application to infrastructure implementation dependency", proposal.Title);
            Assert.Equal("Keep application independent from infrastructure.", proposal.Rationale);
            Assert.Equal(BrickRoleSelector.From("Architecture.Layer.Application"), proposal.SourceRoles);
            Assert.Equal(BrickRoleSelector.From("Architecture.Layer.Infrastructure"), proposal.TargetRoles);
            Assert.Equal(BrickDependencyKindId.From("ObjectCreation"), proposal.DependencyKindId);
            Assert.Equal(BrickDecision.Deny, proposal.SuggestedDecision);
            Assert.Equal(BrickSeverity.Warning, proposal.SuggestedSeverity);
            Assert.Equal(BrickRuleLifecycleState.Candidate, proposal.LifecycleState);
            Assert.True(proposal.IsAdvisory);
            Assert.False(proposal.CanBreakBuild);
            Assert.Same(evidence, proposal.Evidence);
            Assert.Equal("OrderService -> SqlOrderRepository", proposal.Evidence.PositiveExamples.Single());
            Assert.True(proposal.HasRequiredEvidence);
        }

        [Fact]
        public void RuleProposalRejectsMissingEvidenceAndNormalizesText()
        {
            var evidence = new BrickRuleProposalEvidence(null, null, null, null, null, null);
            var proposal = new BrickRuleProposal(
                null,
                null,
                null,
                default,
                default,
                default,
                BrickDecision.Allow,
                BrickSeverity.Info,
                evidence,
                BrickRuleLifecycleState.Draft);

            Assert.Equal(string.Empty, proposal.ProposalId);
            Assert.Equal(string.Empty, proposal.Title);
            Assert.Equal(string.Empty, proposal.Rationale);
            Assert.False(proposal.HasRequiredEvidence);
            Assert.Empty(proposal.Evidence.PositiveExamples);
            Assert.Empty(proposal.Evidence.NegativeExamples);
            Assert.Empty(proposal.Evidence.FalsePositiveRisks);
            Assert.Empty(proposal.Evidence.AffectedScopes);
            Assert.Equal(string.Empty, proposal.Evidence.ObservedStructure);
            Assert.Equal(string.Empty, proposal.Evidence.MigrationImpact);
        }

        [Fact]
        public void RuleProposalCreatesEmptyEvidenceWhenNullEvidenceIsSupplied()
        {
            var proposal = new BrickRuleProposal(
                "proposal",
                "title",
                "rationale",
                BrickRoleSelector.From("A"),
                BrickRoleSelector.From("B"),
                BrickDependencyKindId.From("uses"),
                BrickDecision.Deny,
                BrickSeverity.Warning,
                null,
                BrickRuleLifecycleState.Observing);

            Assert.False(proposal.HasRequiredEvidence);
            Assert.NotNull(proposal.Evidence);
        }

        [Fact]
        public void RuleProposalLifecyclePreventsAiFromStartingEnforced()
        {
            var evidence = new BrickRuleProposalEvidence("observed", new[] { "positive" }, new[] { "negative" }, new[] { "risk" }, new[] { "scope" }, "impact");

            Assert.Throws<ArgumentOutOfRangeException>(() => new BrickRuleProposal(
                "proposal",
                "title",
                "rationale",
                BrickRoleSelector.From("A"),
                BrickRoleSelector.From("B"),
                BrickDependencyKindId.From("uses"),
                BrickDecision.Deny,
                BrickSeverity.Error,
                evidence,
                BrickRuleLifecycleState.Enforced));
        }

        [Fact]
        public void AiTrustBoundaryDefaultsToAdvisoryAndBlocksSilentPolicyChanges()
        {
            var boundary = BrickAiTrustBoundary.Default;

            Assert.Equal(BrickAiMode.Off, boundary.Mode);
            Assert.Equal(BrickAiCommentFormat.Markdown, boundary.CommentFormat);
            Assert.False(boundary.AllowRuleProposal);
            Assert.False(boundary.AllowAutoEnforcement);
            Assert.False(boundary.AllowsSilentPolicyMutation);
            Assert.False(boundary.CanActivateRuleWithoutReview);
            Assert.False(boundary.CanCreateSuppressionWithoutReview);
            Assert.False(boundary.CanCreateBaselineWithoutReview);
        }

        [Fact]
        public void AiTrustBoundaryNormalizesUnsafeAutoEnforcement()
        {
            var boundary = new BrickAiTrustBoundary(
                BrickAiMode.SuggestRules,
                BrickAiCommentFormat.Both,
                true,
                true,
                false);

            Assert.Equal(BrickAiMode.SuggestRules, boundary.Mode);
            Assert.Equal(BrickAiCommentFormat.Both, boundary.CommentFormat);
            Assert.True(boundary.AllowRuleProposal);
            Assert.False(boundary.AllowAutoEnforcement);
            Assert.False(boundary.AllowsSilentPolicyMutation);
        }

        [Fact]
        public void AiTrustBoundaryCanRepresentExplicitReviewedAutomation()
        {
            var boundary = new BrickAiTrustBoundary(
                BrickAiMode.Explain,
                BrickAiCommentFormat.Json,
                false,
                true,
                true);

            Assert.True(boundary.AllowAutoEnforcement);
            Assert.True(boundary.AllowsSilentPolicyMutation);
            Assert.True(boundary.CanActivateRuleWithoutReview);
            Assert.True(boundary.CanCreateSuppressionWithoutReview);
            Assert.True(boundary.CanCreateBaselineWithoutReview);
        }

        private static BrickRemediationOption Option(
            string id,
            BrickRemediationKind kind,
            BrickRemediationRisk risk,
            bool preferred) =>
            new BrickRemediationOption(id, kind, id, id, risk, preferred);

        private static BrickViolation Violation() => Violation("Sales.Application.OrderService");

        private static BrickViolation Violation(string sourceName)
        {
            var source = new BrickElement(
                BrickElementId.From("type:" + sourceName),
                BrickElementKind.Type,
                sourceName);
            var target = new BrickElement(
                BrickElementId.From("type:Sales.Infrastructure.SqlOrderRepository"),
                BrickElementKind.Type,
                "Sales.Infrastructure.SqlOrderRepository");

            return new BrickViolation(
                BrickViolationKind.DependencyRule,
                source,
                "No infrastructure dependency.",
                BrickSeverity.Error,
                BrickViolationState.Active,
                RuleId.From("XMoleculesBricks0001"),
                "No infrastructure dependency",
                target,
                new[] { RoleId.From("Architecture.Layer.Application") },
                new[] { RoleId.From("Architecture.Layer.Infrastructure") },
                BrickDependencyKindId.From("ObjectCreation"),
                BrickScope.Type,
                BrickDependencyLayer.Static,
                BrickEvidenceLevel.CompilerConfirmed);
        }

        private static BrickViolation WithoutRuleIdViolation(string sourceName)
        {
            var source = new BrickElement(BrickElementId.From("type:" + sourceName), BrickElementKind.Type, sourceName);

            return new BrickViolation(
                BrickViolationKind.RequiredDependency,
                source,
                "message",
                BrickSeverity.Warning,
                BrickViolationState.Active);
        }
    }
}
