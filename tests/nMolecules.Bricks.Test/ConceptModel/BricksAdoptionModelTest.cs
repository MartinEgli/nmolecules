using System;
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
    }
}
