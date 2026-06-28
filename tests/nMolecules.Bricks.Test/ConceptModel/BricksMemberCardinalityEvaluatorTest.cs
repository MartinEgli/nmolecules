using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksMemberCardinalityEvaluatorTest
    {
        [Fact]
        public void EvaluateExactlyOneMemberEmitsViolationWhenCountIsNotOne()
        {
            var element = Element();
            var contract = new RequireExactlyOneMemberAttribute(typeof(IdentityMemberAttribute));

            var missing = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts()).Single();
            var duplicate = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts(typeof(IdentityMemberAttribute), 2)).Single();
            var satisfied = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts(typeof(IdentityMemberAttribute), 1));

            Assert.Equal(BrickViolationKind.MemberCardinality, missing.Kind);
            Assert.Equal("RequireExactlyOneMember", missing.RuleName);
            Assert.Equal(element, missing.Source);
            Assert.Contains(nameof(IdentityMemberAttribute), missing.Message);
            Assert.Equal(BrickViolationState.Active, missing.State);
            Assert.Equal(BrickSeverity.Error, missing.Severity);
            Assert.Equal(BrickViolationKind.MemberCardinality, duplicate.Kind);
            Assert.Empty(satisfied);
        }

        [Fact]
        public void EvaluateAllMembersEmitsOneViolationPerMissingMarker()
        {
            var element = Element();
            var contract = new RequireAllMembersAttribute(typeof(IdentityMemberAttribute), typeof(VersionMemberAttribute));

            var violations = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1)).ToArray();

            Assert.Single(violations);
            Assert.Equal("RequireAllMembers", violations[0].RuleName);
            Assert.Contains(nameof(VersionMemberAttribute), violations[0].Message);
        }

        [Fact]
        public void EvaluateMemberCountEmitsViolationWhenCountDiffers()
        {
            var element = Element();
            var contract = new RequireMemberCountAttribute(typeof(VersionMemberAttribute), 0);

            var violations = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 1)).ToArray();

            Assert.Single(violations);
            Assert.Equal("RequireMemberCount", violations[0].RuleName);
            Assert.Contains("expected 0", violations[0].Message);
        }

        [Fact]
        public void EvaluateMemberRangeEmitsViolationWhenCountIsOutsideInclusiveRange()
        {
            var element = Element();
            var contract = new RequireMemberRangeAttribute(typeof(VersionMemberAttribute), 2, 4);

            var tooFew = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 1)).Single();
            var tooMany = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 5)).Single();
            var validLowerBound = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 2));
            var validUpperBound = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 4));

            Assert.Equal("RequireMemberRange", tooFew.RuleName);
            Assert.Equal("RequireMemberRange", tooMany.RuleName);
            Assert.Contains("between 2 and 4", tooFew.Message);
            Assert.Empty(validLowerBound);
            Assert.Empty(validUpperBound);
        }

        [Fact]
        public void EvaluateExclusiveChoiceRequiresExactlyOneSide()
        {
            var element = Element();
            var contract = new RequireExclusiveChoiceAttribute(typeof(IdentityMemberAttribute), typeof(VersionMemberAttribute));

            var none = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts()).Single();
            var both = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1, typeof(VersionMemberAttribute), 1)).Single();
            var leftOnly = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1));

            Assert.Equal("RequireExclusiveChoice", none.RuleName);
            Assert.Equal("RequireExclusiveChoice", both.RuleName);
            Assert.Empty(leftOnly);
        }

        [Fact]
        public void EvaluateForbiddenMemberEmitsViolationWhenMarkerIsPresent()
        {
            var element = Element();
            var contract = new ForbidMemberAttribute(typeof(VersionMemberAttribute));

            var violation = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(VersionMemberAttribute), 1)).Single();
            var valid = BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { contract }, Counts());

            Assert.Equal("ForbidMember", violation.RuleName);
            Assert.Contains(nameof(VersionMemberAttribute), violation.Message);
            Assert.Empty(valid);
        }

        [Fact]
        public void EvaluateUniqueNamedMemberAllowsDifferentNamesAndRejectsDuplicateNames()
        {
            var element = Element();
            var contract = new RequireUniqueNamedMemberAttribute(typeof(IdentityMemberAttribute));

            var duplicate = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 3),
                NamedCounts(typeof(IdentityMemberAttribute), "X", 2, "Y", 1)).Single();
            var valid = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 2),
                NamedCounts(typeof(IdentityMemberAttribute), "X", 1, "Y", 1));

            Assert.Equal("RequireUniqueNamedMember", duplicate.RuleName);
            Assert.Contains("'X'", duplicate.Message);
            Assert.Empty(valid);
        }

        [Fact]
        public void EvaluateUniqueNamedMemberTreatsUnnamedMarkersAsOneSlot()
        {
            var element = Element();
            var contract = new RequireUniqueNamedMemberAttribute(typeof(IdentityMemberAttribute));

            var duplicate = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 2),
                NamedCounts(typeof(IdentityMemberAttribute), string.Empty, 2)).Single();

            Assert.Equal("RequireUniqueNamedMember", duplicate.RuleName);
            Assert.Contains("<unnamed>", duplicate.Message);
        }

        [Fact]
        public void EvaluateUniqueNamedMemberIgnoresMissingNamedCountIndex()
        {
            var element = Element();
            var contract = new RequireUniqueNamedMemberAttribute(typeof(IdentityMemberAttribute));

            var valid = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 2),
                null);

            Assert.Empty(valid);
        }

        [Fact]
        public void EvaluateNamedMembersRequiresEveryConfiguredName()
        {
            var element = Element();
            var contract = new RequireNamedMembersAttribute(typeof(IdentityMemberAttribute), "X", "Y");

            var missing = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1),
                NamedCounts(typeof(IdentityMemberAttribute), "X", 1)).Single();
            var valid = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 2),
                NamedCounts(typeof(IdentityMemberAttribute), "X", 1, "Y", 2));

            Assert.Equal("RequireNamedMembers", missing.RuleName);
            Assert.Contains("'Y'", missing.Message);
            Assert.Empty(valid);
        }

        [Fact]
        public void EvaluateNamedMembersSupportsUnnamedRequiredMarkers()
        {
            var element = Element();
            var contract = new RequireNamedMembersAttribute(typeof(IdentityMemberAttribute), string.Empty);

            var missing = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1),
                NamedCounts(typeof(IdentityMemberAttribute), "X", 1)).Single();
            var valid = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 1),
                NamedCounts(typeof(IdentityMemberAttribute), string.Empty, 1));

            Assert.Equal("RequireNamedMembers", missing.RuleName);
            Assert.Contains("<unnamed>", missing.Message);
            Assert.Empty(valid);
        }

        [Fact]
        public void EvaluateNamedMembersReportsMissingNamesWhenNamedCountIndexIsAbsent()
        {
            var element = Element();
            var contract = new RequireNamedMembersAttribute(typeof(IdentityMemberAttribute), "X", "X", "Y");

            var violations = BrickMemberCardinalityEvaluator.Evaluate(
                element,
                new Attribute[] { contract },
                Counts(typeof(IdentityMemberAttribute), 2),
                null).ToArray();

            Assert.Equal(2, violations.Length);
            Assert.All(violations, violation => Assert.Equal("RequireNamedMembers", violation.RuleName));
            Assert.Contains(violations, violation => violation.Message.Contains("'X'"));
            Assert.Contains(violations, violation => violation.Message.Contains("'Y'"));
        }

        [Fact]
        public void EvaluateNormalizesNullInputsAndIgnoresUnknownContracts()
        {
            var element = Element();

            Assert.Empty(BrickMemberCardinalityEvaluator.Evaluate(element, null, null));
            Assert.Empty(BrickMemberCardinalityEvaluator.Evaluate(element, new Attribute[] { new UnknownContractAttribute() }, null));
        }

        [Fact]
        public void EvaluateRequiresElement()
        {
            Assert.Throws<ArgumentNullException>(() => BrickMemberCardinalityEvaluator.Evaluate(null, null, null));
        }

        private static BrickElement Element() =>
            new BrickElement(BrickElementId.From("type:Order"), BrickElementKind.Type, "Order");

        private static IReadOnlyDictionary<Type, int> Counts(params object[] pairs)
        {
            var counts = new Dictionary<Type, int>();
            for (var index = 0; index < pairs.Length; index += 2)
            {
                counts.Add((Type)pairs[index], (int)pairs[index + 1]);
            }

            return counts;
        }

        private static IReadOnlyDictionary<Type, IReadOnlyDictionary<string, int>> NamedCounts(Type markerType, params object[] pairs)
        {
            var counts = new Dictionary<string, int>();
            for (var index = 0; index < pairs.Length; index += 2)
            {
                counts.Add((string)pairs[index], (int)pairs[index + 1]);
            }

            return new Dictionary<Type, IReadOnlyDictionary<string, int>>
            {
                { markerType, counts }
            };
        }

        private sealed class IdentityMemberAttribute : Attribute
        {
        }

        private sealed class VersionMemberAttribute : Attribute
        {
        }

        private sealed class UnknownContractAttribute : Attribute
        {
        }
    }
}
