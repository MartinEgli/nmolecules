using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRoleResolutionModelTest
    {
        [Fact]
        public void AssignmentPrecedenceOrdersBySpecificityThenAuthority()
        {
            var convention = new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Convention, BrickAssignmentAuthority.Derived);
            var assembly = new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Assembly, BrickAssignmentAuthority.Derived);
            var namespaceAlias = new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Alias);
            var namespaceDirect = new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Direct);

            Assert.True(assembly.CompareTo(convention) > 0);
            Assert.True(namespaceDirect.CompareTo(namespaceAlias) > 0);
            Assert.True(namespaceAlias.CompareTo(namespaceDirect) < 0);
            Assert.True(namespaceDirect.IsStrongerThan(namespaceAlias));
            Assert.False(namespaceAlias.IsStrongerThan(namespaceDirect));
            Assert.Equal(0, namespaceDirect.CompareTo(new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Direct)));
        }

        [Fact]
        public void AssignmentPrecedenceExposesValueSemantics()
        {
            var precedence = new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);

            Assert.True(precedence.Equals((object)new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct)));
            Assert.False(precedence.Equals("Element"));
            Assert.Equal(new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct).GetHashCode(), precedence.GetHashCode());
            Assert.True(precedence == new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct));
            Assert.True(precedence != new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.External));
        }

        [Fact]
        public void RoleAssignmentCapturesSelectorRoleModeSourceAndBehavior()
        {
            var selector = new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.OrderPolicy", "Billing");
            var precedence = new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var assignment = new BrickRoleAssignment(
                selector,
                RoleId.From("Architecture.Layer.Domain"),
                BrickAssignmentMode.DirectAttribute,
                BrickAssignmentSource.SourceAttribute,
                precedence,
                BrickAssignmentBehavior.Apply,
                "Declared through [Role].");

            Assert.Equal(selector, assignment.Selector);
            Assert.Equal(RoleId.From("Architecture.Layer.Domain"), assignment.RoleId);
            Assert.Equal(BrickAssignmentMode.DirectAttribute, assignment.Mode);
            Assert.Equal(BrickAssignmentSource.SourceAttribute, assignment.Source);
            Assert.Equal(precedence, assignment.Precedence);
            Assert.Equal(BrickAssignmentBehavior.Apply, assignment.Behavior);
            Assert.Equal("Declared through [Role].", assignment.Reason);
        }

        [Fact]
        public void RoleAssignmentNormalizesNullValues()
        {
            var assignment = new BrickRoleAssignment(
                null,
                default,
                BrickAssignmentMode.Inference,
                BrickAssignmentSource.Inference,
                default,
                BrickAssignmentBehavior.Suppress,
                null);

            Assert.Equal(default, assignment.Selector);
            Assert.Equal(default, assignment.RoleId);
            Assert.Null(assignment.Reason);
        }

        [Fact]
        public void RoleConflictCapturesEqualPrecedenceConflict()
        {
            var left = Assignment("Domain", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var right = Assignment("Infrastructure", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var conflict = new BrickRoleConflict(left, right, "Exclusive roles share equal precedence.");

            Assert.Equal(left, conflict.FirstAssignment);
            Assert.Equal(right, conflict.SecondAssignment);
            Assert.Equal("Exclusive roles share equal precedence.", conflict.Reason);
        }

        [Fact]
        public void ResolvedRolesCopiesRoleResolutionLists()
        {
            var element = new BrickElement(BrickElementId.From("type:OrderPolicy"), BrickElementKind.Type, "OrderPolicy");
            var candidate = Assignment("Domain", BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.Alias);
            var applied = Assignment("Application", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var suppressed = Assignment("Legacy", BrickAssignmentSpecificity.Convention, BrickAssignmentAuthority.Derived);
            var conflict = new BrickRoleConflict(candidate, applied, "Different exclusive roles.");
            var candidates = new[] { candidate };
            var appliedAssignments = new[] { applied };
            var suppressedAssignments = new[] { suppressed };
            var conflicts = new[] { conflict };
            var resolved = new BrickResolvedRoles(
                element,
                candidates,
                appliedAssignments,
                suppressedAssignments,
                conflicts);

            candidates[0] = suppressed;
            appliedAssignments[0] = candidate;
            suppressedAssignments[0] = applied;
            conflicts[0] = new BrickRoleConflict(applied, suppressed, "Changed after construction.");

            Assert.Equal(element, resolved.Element);
            Assert.Equal(candidate, resolved.CandidateAssignments.Single());
            Assert.Equal(applied, resolved.AppliedAssignments.Single());
            Assert.Equal(suppressed, resolved.SuppressedAssignments.Single());
            Assert.Equal(conflict, resolved.Conflicts.Single());
            Assert.Equal(new[] { RoleId.From("Application") }, resolved.EffectiveRoles);
            Assert.True(resolved.HasConflicts);
        }

        [Fact]
        public void ResolvedRolesNormalizesNullLists()
        {
            var element = new BrickElement(default, BrickElementKind.Unknown, null);
            var resolved = new BrickResolvedRoles(element, null, null, null, null);

            Assert.Equal(element, resolved.Element);
            Assert.Empty(resolved.CandidateAssignments);
            Assert.Empty(resolved.AppliedAssignments);
            Assert.Empty(resolved.SuppressedAssignments);
            Assert.Empty(resolved.Conflicts);
            Assert.Empty(resolved.EffectiveRoles);
            Assert.False(resolved.HasConflicts);
        }

        [Fact]
        public void ResolutionTraceCapturesExplainabilityFacts()
        {
            var element = new BrickElement(BrickElementId.From("type:OrderPolicy"), BrickElementKind.Type, "OrderPolicy");
            var candidate = Assignment("Domain", BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct);
            var trace = new BrickResolutionTrace(
                element,
                new[] { candidate },
                new[] { RoleId.From("Domain") },
                new[] { "Direct element assignment wins." },
                hasConflict: false);

            Assert.Equal(element, trace.Element);
            Assert.Equal(candidate, trace.Candidates.Single());
            Assert.Equal(new[] { RoleId.From("Domain") }, trace.ResolvedRoles);
            Assert.Equal(new[] { "Direct element assignment wins." }, trace.Decisions);
            Assert.False(trace.HasConflict);
        }

        [Fact]
        public void ResolutionTraceNormalizesNullValues()
        {
            var element = new BrickElement(default, BrickElementKind.Unknown, null);
            var trace = new BrickResolutionTrace(element, null, null, null, hasConflict: true);

            Assert.Equal(element, trace.Element);
            Assert.Empty(trace.Candidates);
            Assert.Empty(trace.ResolvedRoles);
            Assert.Empty(trace.Decisions);
            Assert.True(trace.HasConflict);
        }

        private static BrickRoleAssignment Assignment(
            string role,
            BrickAssignmentSpecificity specificity,
            BrickAssignmentAuthority authority) =>
            new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Type, "*"),
                RoleId.From(role),
                BrickAssignmentMode.DirectAttribute,
                BrickAssignmentSource.SourceAttribute,
                new BrickAssignmentPrecedence(specificity, authority),
                BrickAssignmentBehavior.Apply,
                null);
    }
}
