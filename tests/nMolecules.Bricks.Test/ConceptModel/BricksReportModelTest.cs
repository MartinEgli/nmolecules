using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksReportModelTest
    {
        [Fact]
        public void BrickReportDocumentUsesCurrentSchemaByDefaultAndCopiesViolations()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
            var violation = Violation(BrickViolationState.Active);
            var violations = new[] { violation };

            var report = new BrickReportDocument(generatedAt, violations);
            violations[0] = Violation(BrickViolationState.Suppressed);

            Assert.Equal(BrickReportDocument.CurrentSchema, report.Schema);
            Assert.True(report.IsCurrentSchema);
            Assert.Equal(generatedAt, report.GeneratedAt);
            Assert.Equal(violation, report.Violations.Single());
            Assert.Equal(1, report.Summary.Total);
            Assert.Equal(1, report.Summary.Active);
            Assert.Equal(0, report.Summary.Suppressed);
        }

        [Fact]
        public void BrickReportDocumentNormalizesNullSchemaAndViolationList()
        {
            var report = new BrickReportDocument(DateTimeOffset.UnixEpoch, null, null);

            Assert.Equal(string.Empty, report.Schema);
            Assert.False(report.IsCurrentSchema);
            Assert.Empty(report.Violations);
            Assert.Equal(0, report.Summary.Total);
        }

        [Fact]
        public void BrickReportSummaryCountsViolationStates()
        {
            var report = new BrickReportDocument(
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    Violation(BrickViolationState.Active),
                    Violation(BrickViolationState.Suppressed),
                    Violation(BrickViolationState.Baselined),
                    Violation(BrickViolationState.ExpiredSuppression),
                    Violation(BrickViolationState.ExpiredBaseline)
                });

            Assert.Equal(5, report.Summary.Total);
            Assert.Equal(1, report.Summary.Active);
            Assert.Equal(1, report.Summary.Suppressed);
            Assert.Equal(1, report.Summary.Baselined);
            Assert.Equal(1, report.Summary.ExpiredSuppressions);
            Assert.Equal(1, report.Summary.ExpiredBaselineEntries);
        }

        [Fact]
        public void BrickReportSummaryCanBeConstructedDirectly()
        {
            var summary = new BrickReportSummary(5, 1, 2, 3, 4, 5);

            Assert.Equal(5, summary.Total);
            Assert.Equal(1, summary.Active);
            Assert.Equal(2, summary.Suppressed);
            Assert.Equal(3, summary.Baselined);
            Assert.Equal(4, summary.ExpiredSuppressions);
            Assert.Equal(5, summary.ExpiredBaselineEntries);
        }

        private static BrickViolation Violation(BrickViolationState state) =>
            new BrickViolation(
                BrickViolationKind.DependencyRule,
                new BrickElement(BrickElementId.From("type:Order"), BrickElementKind.Type, "Order"),
                "Violation",
                BrickSeverity.Error,
                state);
    }
}
