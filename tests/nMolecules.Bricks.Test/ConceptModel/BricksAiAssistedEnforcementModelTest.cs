using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [Fact]
        public void AiCommentFactoryCreatesCommentsOnlyWhenBoundaryAllowsExplanations()
        {
            var disabled = BrickAiCommentFactory.CreateDocument(
                new[] { Violation() },
                DateTimeOffset.UnixEpoch,
                BrickAiTrustBoundary.Default);
            var enabled = BrickAiCommentFactory.CreateDocument(
                new[] { Violation() },
                DateTimeOffset.UnixEpoch,
                new BrickAiTrustBoundary(
                    BrickAiMode.Explain,
                    BrickAiCommentFormat.Both,
                    allowRuleProposal: false,
                    allowAutoEnforcement: false,
                    allowSilentPolicyMutation: false));

            Assert.Empty(disabled.Comments);
            Assert.Single(enabled.Comments);
            Assert.Equal("adjust-architecture-boundary", enabled.Comments.Single().RecommendedOption.Id);
            Assert.Contains("No infrastructure dependency.", enabled.Comments.Single().ProblemSummary);
        }

        [Fact]
        public void AiCommentFactoryHandlesNullBoundaryNullViolationsAndSparseItems()
        {
            var boundary = new BrickAiTrustBoundary(
                BrickAiMode.Explain,
                BrickAiCommentFormat.Markdown,
                allowRuleProposal: false,
                allowAutoEnforcement: false,
                allowSilentPolicyMutation: false);

            var nullBoundary = BrickAiCommentFactory.CreateDocument(
                new[] { Violation() },
                DateTimeOffset.UnixEpoch,
                null);
            var nullViolations = BrickAiCommentFactory.CreateDocument(
                null,
                DateTimeOffset.UnixEpoch,
                boundary);
            var sparse = BrickAiCommentFactory.CreateDocument(
                new[] { null, WithoutRuleIdViolation("Sales.Application.NoTargetService") },
                DateTimeOffset.UnixEpoch,
                boundary);

            Assert.Empty(nullBoundary.Comments);
            Assert.Empty(nullViolations.Comments);
            Assert.Single(sparse.Comments);
            Assert.Contains("the expected target boundary", sparse.Comments.Single().AiRepairHints.Last());
        }

        [Fact]
        public void AiCommentFactoryChoosesRemediationByViolationKindAndFallbackProblemText()
        {
            var boundary = new BrickAiTrustBoundary(
                BrickAiMode.Explain,
                BrickAiCommentFormat.Markdown,
                allowRuleProposal: false,
                allowAutoEnforcement: false,
                allowSilentPolicyMutation: false);
            var source = new BrickElement(BrickElementId.From("type:Source"), BrickElementKind.Type, "Source");
            var violations = new[]
            {
                new BrickViolation(BrickViolationKind.RequiredDependency, source, null, BrickSeverity.Warning, BrickViolationState.Active),
                new BrickViolation(BrickViolationKind.RoleCombination, source, null, BrickSeverity.Warning, BrickViolationState.Active),
                new BrickViolation(BrickViolationKind.RoleResolution, source, null, BrickSeverity.Warning, BrickViolationState.Active),
                new BrickViolation(BrickViolationKind.MemberCardinality, source, null, BrickSeverity.Warning, BrickViolationState.Active)
            };

            var document = BrickAiCommentFactory.CreateDocument(violations, DateTimeOffset.UnixEpoch, boundary);

            Assert.Equal(
                new[]
                {
                    "introduce-required-contract",
                    "split-or-reclassify-role",
                    "split-or-reclassify-role",
                    "rename-or-add-required-member"
                },
                document.Comments.Select(comment => comment.RecommendedOption.Id).ToArray());
            Assert.All(document.Comments, comment => Assert.Contains("Bricks reported", comment.ProblemSummary));
            Assert.Contains("the deterministic Bricks policy", document.Comments[0].ArchitecturalReason);
        }

        [Fact]
        public void AiCommentMarkdownRendererRendersReviewReadyMarkdown()
        {
            var document = new BrickAiCommentDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickAiViolationComment(
                        Violation(),
                        "Application depends on infrastructure.",
                        "This keeps deterministic policy visible.",
                        new[] { Option("IntroduceContract", BrickRemediationKind.IntroduceContract, BrickRemediationRisk.Low, true) },
                        Option("IntroduceContract", BrickRemediationKind.IntroduceContract, BrickRemediationRisk.Low, true),
                        new[] { "Add a contract." },
                        "Do not suppress automatically.")
                });
            var empty = new BrickAiCommentDocument(DateTimeOffset.UnixEpoch, null);

            var markdown = BrickAiCommentMarkdownRenderer.Render(document);
            var emptyMarkdown = BrickAiCommentMarkdownRenderer.Render(empty);

            Assert.Contains("# Bricks AI Review Comments", markdown);
            Assert.Contains("XMoleculesBricks0001", markdown);
            Assert.Contains("Application depends on infrastructure.", markdown);
            Assert.Contains("IntroduceContract", markdown);
            Assert.Contains("No deterministic Bricks violations", emptyMarkdown);
            Assert.Throws<ArgumentNullException>(() => BrickAiCommentMarkdownRenderer.Render(null));
        }

        [Fact]
        public void AiCommentMarkdownRendererHandlesSparseCommentsAndPartialDependencies()
        {
            var sourceWithFullName = new BrickElement(
                BrickElementId.From("type:Sales.Application.SparseService"),
                BrickElementKind.Type,
                "SparseService",
                fullName: "Sales.Application.SparseService");
            var displayOnlySource = new BrickElement(
                BrickElementId.From("type:Sales.Application.RuntimeService"),
                BrickElementKind.Type,
                "RuntimeService");
            var kindOnly = new BrickViolation(
                BrickViolationKind.DependencyRule,
                sourceWithFullName,
                "message",
                BrickSeverity.Info,
                BrickViolationState.Active,
                dependencyKindId: BrickDependencyKindId.From("uses"));
            var layerOnly = new BrickViolation(
                BrickViolationKind.DependencyRule,
                displayOnlySource,
                "message",
                BrickSeverity.Info,
                BrickViolationState.Active,
                dependencyLayer: BrickDependencyLayer.Runtime);
            var option = Option("MoveDependency", BrickRemediationKind.MoveElement, BrickRemediationRisk.Medium, false);
            var document = new BrickAiCommentDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickAiViolationComment(
                        kindOnly,
                        "Problem with `ticks`.",
                        "Reason with `ticks`.",
                        new[] { option },
                        null,
                        null,
                        null),
                    new BrickAiViolationComment(
                        layerOnly,
                        "Runtime dependency.",
                        "Runtime reason.",
                        null,
                        null,
                        null,
                        null)
                });

            var markdown = BrickAiCommentMarkdownRenderer.Render(document);
            var escape = typeof(BrickAiCommentMarkdownRenderer)
                .GetMethod("Escape", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.Contains("unassigned-rule", markdown);
            Assert.Contains("Sales.Application.SparseService", markdown);
            Assert.Contains("RuntimeService", markdown);
            Assert.Contains("Dependency: `uses`", markdown);
            Assert.Contains("Dependency: `Runtime`", markdown);
            Assert.Contains("Problem with 'ticks'.", markdown);
            Assert.Contains("`MoveDependency`:", markdown);
            Assert.DoesNotContain("MoveDependency` recommended", markdown);
            Assert.DoesNotContain("Target:", markdown);
            Assert.DoesNotContain("AI Repair Hints", markdown);
            Assert.DoesNotContain("Suppression Guidance", markdown);
            Assert.NotNull(escape);
            Assert.Equal(string.Empty, escape.Invoke(null, new object[] { null }));
        }

        [Fact]
        public void RuleProposalQueueJsonSerializerRoundTripsReviewQueue()
        {
            var proposal = Proposal("proposal-b");
            var queue = new BrickRuleProposalQueue(
                DateTimeOffset.UnixEpoch,
                new[] { proposal, Proposal("proposal-a") });

            var json = BrickRuleProposalQueueJsonSerializer.Serialize(queue);
            var roundTripped = BrickRuleProposalQueueJsonSerializer.Deserialize(json);

            Assert.Equal(BrickRuleProposalQueue.CurrentSchema, roundTripped.Schema);
            Assert.True(roundTripped.IsCurrentSchema);
            Assert.Equal(new[] { "proposal-a", "proposal-b" }, roundTripped.Proposals.Select(item => item.ProposalId).ToArray());
            Assert.Equal(proposal.Title, roundTripped.Proposals[1].Title);
            Assert.Equal(proposal.SourceRoles, roundTripped.Proposals[1].SourceRoles);
            Assert.Equal(proposal.TargetRoles, roundTripped.Proposals[1].TargetRoles);
            Assert.Equal(proposal.DependencyKindId, roundTripped.Proposals[1].DependencyKindId);
            Assert.True(roundTripped.Proposals[1].HasRequiredEvidence);
            Assert.Throws<ArgumentNullException>(() => BrickRuleProposalQueueJsonSerializer.Serialize(null));
            Assert.Throws<ArgumentNullException>(() => BrickRuleProposalQueueJsonSerializer.Deserialize(null));
            Assert.Throws<ArgumentException>(() => BrickRuleProposalQueueJsonSerializer.Deserialize("null"));
        }

        [Fact]
        public void RuleProposalQueueJsonSerializerHandlesSparseAndUnsafeAiQueueInput()
        {
            var queue = new BrickRuleProposalQueue(DateTimeOffset.UnixEpoch, null, null);
            var noProposals = BrickRuleProposalQueueJsonSerializer.Deserialize(
                "{\"schema\":\"custom\",\"generatedAt\":\"1970-01-01T00:00:00+00:00\"}");
            var unsafeJson =
                "{\"schema\":\"NMolecules.Bricks.RuleProposalQueue/1.0\",\"generatedAt\":\"1970-01-01T00:00:00+00:00\",\"proposals\":[{\"proposalId\":\"proposal-unsafe\",\"title\":\"title\",\"rationale\":\"rationale\",\"sourceRoles\":\"A\",\"targetRoles\":\"B\",\"dependencyKind\":\"uses\",\"suggestedDecision\":\"Maybe\",\"suggestedSeverity\":\"Critical\",\"lifecycleState\":\"Enforced\",\"evidence\":null}]}";

            var unsafeQueue = BrickRuleProposalQueueJsonSerializer.Deserialize(unsafeJson);
            var proposal = unsafeQueue.Proposals.Single();

            Assert.Equal(string.Empty, queue.Schema);
            Assert.False(queue.IsCurrentSchema);
            Assert.Empty(queue.Proposals);
            Assert.Equal("custom", noProposals.Schema);
            Assert.Empty(noProposals.Proposals);
            Assert.Equal(BrickDecision.Deny, proposal.SuggestedDecision);
            Assert.Equal(BrickSeverity.Warning, proposal.SuggestedSeverity);
            Assert.Equal(BrickRuleLifecycleState.Candidate, proposal.LifecycleState);
            Assert.False(proposal.HasRequiredEvidence);
        }

        [Fact]
        public void RuleProposalReviewWorkflowPromotesOnlyExplicitHumanReviewedEvidence()
        {
            var proposal = Proposal("proposal-001");
            var approved = new BrickRuleProposalReview(
                "proposal-001",
                "architecture-owner",
                approved: true,
                BrickRuleLifecycleState.Enforced,
                "Evidence and migration risk were reviewed.",
                DateTimeOffset.UnixEpoch);
            var rejected = new BrickRuleProposalReview(
                "proposal-001",
                "architecture-owner",
                approved: false,
                BrickRuleLifecycleState.Rejected,
                "False positives are too broad.",
                DateTimeOffset.UnixEpoch);

            var promoted = BrickRuleProposalReviewWorkflow.Review(
                proposal,
                approved,
                RuleId.From("XMoleculesBricks0999"),
                "Promoted no application to infrastructure rule",
                priority: 10);
            var blocked = BrickRuleProposalReviewWorkflow.Review(
                proposal,
                rejected,
                RuleId.From("XMoleculesBricks0999"),
                "Promoted no application to infrastructure rule");

            Assert.True(promoted.CanPromote);
            Assert.Same(proposal, promoted.Proposal);
            Assert.Same(approved, promoted.Review);
            Assert.Contains("explicitly reviewed", promoted.Reason);
            Assert.True(promoted.PromotedRule.HasValue);
            Assert.Equal(RuleId.From("XMoleculesBricks0999"), promoted.PromotedRule.Value.RuleId);
            Assert.Equal(RoleId.From("Architecture.Layer.Application"), promoted.PromotedRule.Value.SourceRole);
            Assert.Equal(RoleId.From("Architecture.Layer.Infrastructure"), promoted.PromotedRule.Value.TargetRole);
            Assert.Equal(BrickDecision.Deny, promoted.PromotedRule.Value.Decision);
            Assert.Equal(BrickSeverity.Warning, promoted.PromotedRule.Value.Severity);
            Assert.Equal(10, promoted.PromotedRule.Value.Priority);
            Assert.False(blocked.CanPromote);
            Assert.False(blocked.PromotedRule.HasValue);
            Assert.Contains("did not approve", blocked.Reason);
            Assert.Equal(DateTimeOffset.UnixEpoch, approved.ReviewedAt);
            Assert.Throws<ArgumentNullException>(() => BrickRuleProposalReviewWorkflow.Review(null, approved, RuleId.From("X"), "name"));
            Assert.Throws<ArgumentNullException>(() => BrickRuleProposalReviewWorkflow.Review(proposal, null, RuleId.From("X"), "name"));
            Assert.Throws<ArgumentNullException>(() => new BrickRuleProposalReviewResult(null, approved, false, null, null));
            Assert.Throws<ArgumentNullException>(() => new BrickRuleProposalReviewResult(proposal, null, false, null, null));
            Assert.Equal(string.Empty, new BrickRuleProposalReviewResult(proposal, approved, false, null, null).Reason);
        }

        [Fact]
        public void RuleProposalReviewNormalizesOptionalText()
        {
            var review = new BrickRuleProposalReview(
                null,
                null,
                approved: false,
                BrickRuleLifecycleState.Rejected,
                null,
                DateTimeOffset.UnixEpoch);

            Assert.Equal(string.Empty, review.ProposalId);
            Assert.Equal(string.Empty, review.Reviewer);
            Assert.Equal(string.Empty, review.Rationale);
            Assert.False(review.HasReviewer);
            Assert.False(review.HasRationale);
            Assert.False(review.Approved);
            Assert.Equal(BrickRuleLifecycleState.Rejected, review.TargetLifecycleState);
        }

        [Fact]
        public void RuleProposalReviewWorkflowExplainsEveryBlockedPromotionReason()
        {
            var proposal = Proposal("proposal-001");
            var incomplete = new BrickRuleProposal(
                "proposal-001",
                "title",
                "rationale",
                BrickRoleSelector.From("A"),
                BrickRoleSelector.From("B"),
                BrickDependencyKindId.From("uses"),
                BrickDecision.Deny,
                BrickSeverity.Warning,
                new BrickRuleProposalEvidence(null, null, null, null, null, null),
                BrickRuleLifecycleState.Candidate);

            AssertBlocked(proposal, Review("other", "reviewer", true, BrickRuleLifecycleState.Enforced, "reason"), RuleId.From("X"), "does not match");
            AssertBlocked(proposal, Review("proposal-001", null, true, BrickRuleLifecycleState.Enforced, "reason"), RuleId.From("X"), "requires a reviewer");
            AssertBlocked(proposal, Review("proposal-001", "reviewer", true, BrickRuleLifecycleState.Enforced, null), RuleId.From("X"), "requires a rationale");
            AssertBlocked(incomplete, Review("proposal-001", "reviewer", true, BrickRuleLifecycleState.Enforced, "reason"), RuleId.From("X"), "required evidence");
            AssertBlocked(proposal, Review("proposal-001", "reviewer", true, BrickRuleLifecycleState.Warning, "reason"), RuleId.From("X"), "Enforced review target");
            AssertBlocked(proposal, Review("proposal-001", "reviewer", true, BrickRuleLifecycleState.Enforced, "reason"), default, "rule id is required");
        }

        [Fact]
        public void AiRunConfigurationParsesMsBuildPropertiesAndKeepsUnsafeAutomationClosed()
        {
            var configuration = BrickAiRunConfiguration.FromProperties(new Dictionary<string, string>
            {
                [BrickAiRunConfiguration.ModeProperty] = "SuggestRules",
                [BrickAiRunConfiguration.CommentFormatProperty] = "Both",
                [BrickAiRunConfiguration.AllowRuleProposalsProperty] = "true",
                [BrickAiRunConfiguration.AllowAutoEnforcementProperty] = "true",
                [BrickAiRunConfiguration.AllowSilentPolicyMutationProperty] = "false",
                [BrickAiRunConfiguration.OutputDirectoryProperty] = "artifacts/bricks-ai",
                [BrickAiRunConfiguration.ProposalQueuePathProperty] = "artifacts/bricks-ai/proposals.json"
            });

            Assert.Equal(BrickAiMode.SuggestRules, configuration.TrustBoundary.Mode);
            Assert.Equal(BrickAiCommentFormat.Both, configuration.TrustBoundary.CommentFormat);
            Assert.True(configuration.ShouldEmitComments);
            Assert.True(configuration.ShouldEmitJson);
            Assert.True(configuration.ShouldEmitMarkdown);
            Assert.True(configuration.CanCreateRuleProposals);
            Assert.False(configuration.TrustBoundary.AllowAutoEnforcement);
            Assert.False(configuration.TrustBoundary.AllowsSilentPolicyMutation);
            Assert.Equal("artifacts/bricks-ai", configuration.OutputDirectory);
            Assert.Equal("artifacts/bricks-ai/proposals.json", configuration.ProposalQueuePath);
            Assert.False(BrickAiRunConfiguration.Default.ShouldEmitComments);
        }

        [Fact]
        public void AiRunConfigurationCoversDefaultsAndCommentFormatCombinations()
        {
            var constructedDefault = new BrickAiRunConfiguration(null, null, null);
            var parsedDefault = BrickAiRunConfiguration.FromProperties(null);
            var markdown = BrickAiRunConfiguration.FromProperties(new Dictionary<string, string>
            {
                [BrickAiRunConfiguration.ModeProperty] = "Explain",
                [BrickAiRunConfiguration.CommentFormatProperty] = "Markdown"
            });
            var json = BrickAiRunConfiguration.FromProperties(new Dictionary<string, string>
            {
                [BrickAiRunConfiguration.ModeProperty] = "Explain",
                [BrickAiRunConfiguration.CommentFormatProperty] = "Json",
                [BrickAiRunConfiguration.AllowRuleProposalsProperty] = "false"
            });
            var explainWithProposalFlag = new BrickAiRunConfiguration(
                new BrickAiTrustBoundary(
                    BrickAiMode.Explain,
                    BrickAiCommentFormat.Both,
                    allowRuleProposal: true,
                    allowAutoEnforcement: false,
                    allowSilentPolicyMutation: false),
                "out",
                "queue.json");

            Assert.Equal(BrickAiMode.Off, constructedDefault.TrustBoundary.Mode);
            Assert.Equal(string.Empty, constructedDefault.OutputDirectory);
            Assert.Equal(string.Empty, constructedDefault.ProposalQueuePath);
            Assert.False(parsedDefault.ShouldEmitComments);
            Assert.False(parsedDefault.ShouldEmitMarkdown);
            Assert.False(parsedDefault.ShouldEmitJson);
            Assert.False(parsedDefault.CanCreateRuleProposals);
            Assert.True(markdown.ShouldEmitMarkdown);
            Assert.False(markdown.ShouldEmitJson);
            Assert.False(markdown.CanCreateRuleProposals);
            Assert.False(json.ShouldEmitMarkdown);
            Assert.True(json.ShouldEmitJson);
            Assert.False(json.CanCreateRuleProposals);
            Assert.True(explainWithProposalFlag.ShouldEmitMarkdown);
            Assert.True(explainWithProposalFlag.ShouldEmitJson);
            Assert.False(explainWithProposalFlag.CanCreateRuleProposals);
        }

        private static BrickRemediationOption Option(
            string id,
            BrickRemediationKind kind,
            BrickRemediationRisk risk,
            bool preferred) =>
            new BrickRemediationOption(id, kind, id, id, risk, preferred);

        private static BrickRuleProposal Proposal(string id)
        {
            var evidence = new BrickRuleProposalEvidence(
                "Application repeatedly creates infrastructure repositories.",
                new[] { "OrderService -> SqlOrderRepository" },
                new[] { "CompositionRoot -> SqlOrderRepository" },
                new[] { "Generated DI wiring may look similar." },
                new[] { "Sales.Application", "Sales.Infrastructure" },
                "Move wiring through a contract.");

            return new BrickRuleProposal(
                id,
                "No application to infrastructure implementation dependency",
                "Keep application independent from infrastructure.",
                BrickRoleSelector.From("Architecture.Layer.Application"),
                BrickRoleSelector.From("Architecture.Layer.Infrastructure"),
                BrickDependencyKindId.From("ObjectCreation"),
                BrickDecision.Deny,
                BrickSeverity.Warning,
                evidence,
                BrickRuleLifecycleState.Candidate);
        }

        private static BrickRuleProposalReview Review(
            string proposalId,
            string reviewer,
            bool approved,
            BrickRuleLifecycleState targetState,
            string rationale) =>
            new BrickRuleProposalReview(proposalId, reviewer, approved, targetState, rationale, DateTimeOffset.UnixEpoch);

        private static void AssertBlocked(
            BrickRuleProposal proposal,
            BrickRuleProposalReview review,
            RuleId ruleId,
            string expectedReason)
        {
            var result = BrickRuleProposalReviewWorkflow.Review(proposal, review, ruleId, "name");
            Assert.False(result.CanPromote);
            Assert.False(result.PromotedRule.HasValue);
            Assert.Contains(expectedReason, result.Reason);
        }

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
