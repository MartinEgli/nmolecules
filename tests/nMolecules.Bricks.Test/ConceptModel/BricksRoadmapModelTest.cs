using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test.ConceptModel
{
    public class BricksRoadmapModelTest
    {
        [Fact]
        public void RoadmapItemCapturesRequirementAndRationale()
        {
            var item = new BrickRoadmapItem(
                "direct-role-assignment",
                "Direct role assignment",
                true,
                "V1 baseline capability.");

            Assert.Equal("direct-role-assignment", item.Id);
            Assert.Equal("Direct role assignment", item.DisplayName);
            Assert.True(item.Required);
            Assert.Equal("V1 baseline capability.", item.Rationale);
        }

        [Fact]
        public void RoadmapItemNormalizesNullText()
        {
            var item = new BrickRoadmapItem(null, null, false, null);

            Assert.Equal(string.Empty, item.Id);
            Assert.Equal(string.Empty, item.DisplayName);
            Assert.False(item.Required);
            Assert.Equal(string.Empty, item.Rationale);
        }

        [Fact]
        public void RoadmapStageDefinitionSortsItemsAndNormalizesText()
        {
            var later = Item("z", true);
            var earlier = Item("a", true);
            var excluded = Item("runtime-wiring-analysis", false);

            var definition = new BrickRoadmapStageDefinition(
                BrickRoadmapStage.V1,
                null,
                null,
                new[] { later, null, earlier },
                new[] { excluded, null });

            Assert.Equal(BrickRoadmapStage.V1, definition.Stage);
            Assert.Equal(string.Empty, definition.DisplayName);
            Assert.Equal(string.Empty, definition.Description);
            Assert.Equal(new[] { "a", "z" }, definition.IncludedItems.Select(item => item.Id).ToArray());
            Assert.Equal(new[] { "runtime-wiring-analysis" }, definition.ExcludedItems.Select(item => item.Id).ToArray());
        }

        [Fact]
        public void RoadmapStageDefinitionNormalizesNullCollections()
        {
            var definition = new BrickRoadmapStageDefinition(BrickRoadmapStage.V2, "V2", "Runtime-aware", null, null);

            Assert.Empty(definition.IncludedItems);
            Assert.Empty(definition.ExcludedItems);
        }

        [Fact]
        public void RoadmapItemResultCapturesStatusAndEvidence()
        {
            var item = Item("role-map-export", true);

            var result = new BrickRoadmapItemResult(item, BrickRoadmapItemStatus.Completed, "Exporter is implemented.");

            Assert.Equal(item, result.Item);
            Assert.Equal(BrickRoadmapItemStatus.Completed, result.Status);
            Assert.Equal("Exporter is implemented.", result.Evidence);
            Assert.True(result.SatisfiesRequirement);
        }

        [Fact]
        public void RoadmapItemResultNormalizesEvidenceAndRequiresItem()
        {
            var optional = Item("ide-visualisation", false);
            var result = new BrickRoadmapItemResult(optional, BrickRoadmapItemStatus.NotRequired, null);

            Assert.Equal(string.Empty, result.Evidence);
            Assert.True(result.SatisfiesRequirement);
            Assert.Throws<ArgumentNullException>(() => new BrickRoadmapItemResult(null, BrickRoadmapItemStatus.Completed));
        }

        [Fact]
        public void BuiltInRoadmapStagesExposeV22BoundariesInStableOrder()
        {
            var stages = BrickBuiltInRoadmapStages.All.ToArray();

            Assert.Equal(
                new[] { BrickRoadmapStage.V1, BrickRoadmapStage.V1_1, BrickRoadmapStage.V1_2, BrickRoadmapStage.V2 },
                stages.Select(stage => stage.Stage).ToArray());
            Assert.Contains(stages[0].IncludedItems, item => item.Id == "direct-role-assignment");
            Assert.Contains(stages[0].IncludedItems, item => item.Id == "minimal-suppression-support");
            Assert.Contains(stages[0].ExcludedItems, item => item.Id == "runtime-wiring-analysis");
            Assert.Contains(stages[1].IncludedItems, item => item.Id == "schema-versioning-policy-files");
            Assert.Contains(stages[2].IncludedItems, item => item.Id == "dependency-graph-export");
            Assert.Contains(stages[3].IncludedItems, item => item.Id == "reflection-modelling-confidence");
            Assert.All(stages, stage => Assert.False(string.IsNullOrWhiteSpace(stage.DisplayName)));
        }

        [Fact]
        public void RoadmapStageAssessmentReportsCompleteStage()
        {
            var definition = new BrickRoadmapStageDefinition(
                BrickRoadmapStage.V1,
                "V1",
                "Baseline",
                new[] { Item("direct-role-assignment", true), Item("broad-ide-visualisation", false) },
                null);
            var required = definition.IncludedItems.Single(item => item.Id == "direct-role-assignment");
            var optional = definition.IncludedItems.Single(item => item.Id == "broad-ide-visualisation");

            var assessment = new BrickRoadmapStageAssessment(
                definition,
                new[]
                {
                    Result(required, BrickRoadmapItemStatus.Completed),
                    Result(optional, BrickRoadmapItemStatus.NotRequired)
                });

            Assert.Equal(BrickRoadmapStageStatus.Complete, assessment.Status);
            Assert.Equal(1, assessment.RequiredItemCount);
            Assert.Equal(1, assessment.CompletedRequiredItemCount);
            Assert.Equal(0, assessment.MissingRequiredItemCount);
            Assert.Equal(1.0, assessment.CompletionRatio);
        }

        [Fact]
        public void RoadmapStageAssessmentReportsPartialStage()
        {
            var definition = new BrickRoadmapStageDefinition(
                BrickRoadmapStage.V1_2,
                "V1.2",
                "Exports",
                new[] { Item("role-map-export", true), Item("dependency-graph-export", true) },
                null);

            var assessment = new BrickRoadmapStageAssessment(
                definition,
                new[] { Result(definition.IncludedItems[1], BrickRoadmapItemStatus.Completed) });

            Assert.Equal(BrickRoadmapStageStatus.Partial, assessment.Status);
            Assert.Equal(2, assessment.RequiredItemCount);
            Assert.Equal(1, assessment.CompletedRequiredItemCount);
            Assert.Equal(1, assessment.MissingRequiredItemCount);
            Assert.Equal(0.5, assessment.CompletionRatio);
        }

        [Fact]
        public void RoadmapStageAssessmentReportsNotStartedStage()
        {
            var definition = new BrickRoadmapStageDefinition(
                BrickRoadmapStage.V2,
                "V2",
                "Runtime-aware",
                new[] { Item("reflection-modelling-confidence", true) },
                null);

            var assessment = new BrickRoadmapStageAssessment(
                definition,
                new[] { Result(definition.IncludedItems[0], BrickRoadmapItemStatus.Missing) });

            Assert.Equal(BrickRoadmapStageStatus.NotStarted, assessment.Status);
            Assert.Equal(0, assessment.CompletedRequiredItemCount);
            Assert.Equal(1, assessment.MissingRequiredItemCount);
            Assert.Equal(0, assessment.CompletionRatio);
        }

        [Fact]
        public void RoadmapStageAssessmentIgnoresNullResultsAndHandlesNoRequiredItems()
        {
            var optional = Item("ide-visualisation", false);
            var definition = new BrickRoadmapStageDefinition(
                BrickRoadmapStage.V2,
                "V2",
                "Optional helper work.",
                new[] { optional },
                null);

            var assessment = new BrickRoadmapStageAssessment(
                definition,
                new BrickRoadmapItemResult[] { null, Result(optional, BrickRoadmapItemStatus.NotRequired) });

            Assert.Single(assessment.Results);
            Assert.Equal(0, assessment.RequiredItemCount);
            Assert.Equal(0, assessment.MissingRequiredItemCount);
            Assert.Equal(1.0, assessment.CompletionRatio);
            Assert.Equal(BrickRoadmapStageStatus.Complete, assessment.Status);
        }

        [Fact]
        public void RoadmapStageAssessmentNormalizesNullResultsAndRequiresDefinition()
        {
            var definition = new BrickRoadmapStageDefinition(
                BrickRoadmapStage.V1_1,
                "V1.1",
                "Policy files",
                new[] { Item("policy-file-prototype", true) },
                null);

            var assessment = new BrickRoadmapStageAssessment(definition, null);

            Assert.Empty(assessment.Results);
            Assert.Equal(BrickRoadmapStageStatus.NotStarted, assessment.Status);
            Assert.Throws<ArgumentNullException>(() => new BrickRoadmapStageAssessment(null, null));
        }

        [Fact]
        public void RoadmapSummaryCountsStagesAndHighestContiguousCompleteStage()
        {
            var complete = Assessment(BrickRoadmapStage.V1, BrickRoadmapStageStatus.Complete);
            var partial = Assessment(BrickRoadmapStage.V1_1, BrickRoadmapStageStatus.Partial);
            var notStarted = Assessment(BrickRoadmapStage.V1_2, BrickRoadmapStageStatus.NotStarted);

            var summary = BrickRoadmapSummary.FromAssessments(new[] { partial, notStarted, complete });

            Assert.Equal(3, summary.TotalStages);
            Assert.Equal(1, summary.CompleteStages);
            Assert.Equal(1, summary.PartialStages);
            Assert.Equal(1, summary.NotStartedStages);
            Assert.Equal(2, summary.MissingRequiredItems);
            Assert.Equal(BrickRoadmapStage.V1, summary.HighestContiguousCompleteStage);
        }

        [Fact]
        public void RoadmapSummaryHandlesNullOrNoCompleteAssessments()
        {
            var empty = BrickRoadmapSummary.FromAssessments(null);
            var none = BrickRoadmapSummary.FromAssessments(new[] { Assessment(BrickRoadmapStage.V1, BrickRoadmapStageStatus.NotStarted) });

            Assert.Equal(0, empty.TotalStages);
            Assert.Null(empty.HighestContiguousCompleteStage);
            Assert.Null(none.HighestContiguousCompleteStage);
        }

        [Fact]
        public void RoadmapReportSortsAssessmentsAndUsesCurrentSchema()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 13, 0, 0, TimeSpan.Zero);
            var report = new BrickRoadmapReport(
                generatedAt,
                new[]
                {
                    Assessment(BrickRoadmapStage.V2, BrickRoadmapStageStatus.NotStarted),
                    Assessment(BrickRoadmapStage.V1, BrickRoadmapStageStatus.Complete)
                });

            Assert.Equal(BrickRoadmapReport.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Equal(new[] { BrickRoadmapStage.V1, BrickRoadmapStage.V2 }, report.Assessments.Select(assessment => assessment.Definition.Stage).ToArray());
            Assert.Equal(2, report.Summary.TotalStages);
        }

        [Fact]
        public void RoadmapReportNormalizesNullAssessmentsAndSchema()
        {
            var report = new BrickRoadmapReport(DateTimeOffset.UnixEpoch, null, null);

            Assert.Empty(report.Assessments);
            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Equal(0, report.Summary.TotalStages);
        }

        [Fact]
        public void RoadmapJsonSerializerWritesVersionedMachineReadableReport()
        {
            var report = new BrickRoadmapReport(
                new DateTimeOffset(2026, 6, 23, 13, 0, 0, TimeSpan.Zero),
                new[]
                {
                    Assessment(BrickRoadmapStage.V1_1, BrickRoadmapStageStatus.Partial),
                    Assessment(BrickRoadmapStage.V1, BrickRoadmapStageStatus.Complete)
                });

            var json = BrickRoadmapReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var assessments = root.GetProperty("assessments");

            Assert.Equal(BrickRoadmapReport.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(2, root.GetProperty("summary").GetProperty("totalStages").GetInt32());
            Assert.Equal("V1", root.GetProperty("summary").GetProperty("highestContiguousCompleteStage").GetString());
            Assert.Equal("V1", assessments[0].GetProperty("stage").GetString());
            Assert.Equal("Complete", assessments[0].GetProperty("status").GetString());
            Assert.Equal("V1_1", assessments[1].GetProperty("stage").GetString());
            Assert.Equal("Partial", assessments[1].GetProperty("status").GetString());
            Assert.Equal("item", assessments[1].GetProperty("results")[0].GetProperty("itemId").GetString());
        }

        [Fact]
        public void RoadmapJsonSerializerOmitsMissingHighestCompleteStage()
        {
            var item = Item("item", true);
            var definition = new BrickRoadmapStageDefinition(BrickRoadmapStage.V1, "V1", "V1", new[] { item }, null);
            var report = new BrickRoadmapReport(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new BrickRoadmapStageAssessment(
                        definition,
                        new[] { new BrickRoadmapItemResult(item, BrickRoadmapItemStatus.Missing, null) })
                });

            var json = BrickRoadmapReportJsonSerializer.Serialize(report);
            using var document = JsonDocument.Parse(json);
            var result = document.RootElement.GetProperty("assessments")[0].GetProperty("results")[0];

            Assert.False(document.RootElement
                .GetProperty("summary")
                .TryGetProperty("highestContiguousCompleteStage", out _));
            Assert.False(result.TryGetProperty("evidence", out _));
        }

        [Fact]
        public void RoadmapJsonSerializerRequiresReport()
        {
            Assert.Throws<ArgumentNullException>(() => BrickRoadmapReportJsonSerializer.Serialize(null));
        }

        private static BrickRoadmapItem Item(string id, bool required) =>
            new BrickRoadmapItem(id, id, required, id);

        private static BrickRoadmapItemResult Result(BrickRoadmapItem item, BrickRoadmapItemStatus status) =>
            new BrickRoadmapItemResult(item, status, status.ToString());

        private static BrickRoadmapStageAssessment Assessment(BrickRoadmapStage stage, BrickRoadmapStageStatus status)
        {
            var item = Item("item", true);
            var secondItem = Item("second-item", true);

            if (status == BrickRoadmapStageStatus.Partial)
            {
                var partialDefinition = new BrickRoadmapStageDefinition(stage, stage.ToString(), stage.ToString(), new[] { item, secondItem }, null);
                return new BrickRoadmapStageAssessment(
                    partialDefinition,
                    new[]
                    {
                        Result(item, BrickRoadmapItemStatus.Completed),
                        Result(secondItem, BrickRoadmapItemStatus.Missing)
                    });
            }

            var definition = new BrickRoadmapStageDefinition(stage, stage.ToString(), stage.ToString(), new[] { item }, null);
            var resultStatus = status == BrickRoadmapStageStatus.Complete
                ? BrickRoadmapItemStatus.Completed
                : BrickRoadmapItemStatus.Missing;

            return new BrickRoadmapStageAssessment(definition, new[] { Result(item, resultStatus) });
        }
    }
}
