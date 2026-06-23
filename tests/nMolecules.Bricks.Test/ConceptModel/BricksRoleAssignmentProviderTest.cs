using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksRoleAssignmentProviderTest
    {
        [Fact]
        public void BrickModelContextCopiesElementsAndRequiresPolicy()
        {
            var policy = Policy();
            var element = Element("type:Order", "Order");
            var elements = new[] { element };

            var context = new BrickModelContext(policy, elements);
            elements[0] = Element("type:Other", "Other");

            Assert.Same(policy, context.Policy);
            Assert.Equal(new[] { element }, context.Elements);
            Assert.Throws<ArgumentNullException>(() => new BrickModelContext(null, null));
        }

        [Fact]
        public void BrickModelContextNormalizesNullElements()
        {
            var context = new BrickModelContext(Policy(), null);

            Assert.Empty(context.Elements);
        }

        [Fact]
        public void PolicyRoleAssignmentProviderReturnsExternalAssignmentsAndAliasMappings()
        {
            var externalAssignment = Assignment("Domain", BrickAssignmentMode.ExternalConfiguration, BrickAssignmentSource.PolicyFile);
            var alias = new BrickAlias(
                "Repository alias",
                new BrickElementSelector(BrickElementKind.Type, "*.Repository"),
                RoleId.From("DDD.Repository"),
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Alias),
                BrickAssignmentBehavior.Apply,
                "Map repository suffix.");
            var policy = new BrickPolicy(
                BrickPolicyId.From("Default"),
                "Default",
                null,
                null,
                null,
                new[] { externalAssignment },
                new[] { alias },
                BrickPermissionDefault.Allow,
                BrickEnforcementMode.Analyze);
            var provider = new BrickPolicyRoleAssignmentProvider();

            var assignments = provider.GetAssignments(new BrickModelContext(policy, null)).ToArray();

            Assert.Equal(2, assignments.Length);
            Assert.Same(externalAssignment, assignments[0]);
            Assert.Equal(alias.Selector, assignments[1].Selector);
            Assert.Equal(alias.CanonicalRoleId, assignments[1].RoleId);
            Assert.Equal(BrickAssignmentMode.AliasMapping, assignments[1].Mode);
            Assert.Equal(BrickAssignmentSource.AliasMapping, assignments[1].Source);
            Assert.Equal(alias.Precedence, assignments[1].Precedence);
            Assert.Equal(alias.Behavior, assignments[1].Behavior);
            Assert.Equal(alias.Reason, assignments[1].Reason);
        }

        [Fact]
        public void PolicyRoleAssignmentProviderRequiresContext()
        {
            var provider = new BrickPolicyRoleAssignmentProvider();

            Assert.Throws<ArgumentNullException>(() => provider.GetAssignments(null).ToArray());
        }

        [Fact]
        public void RoleAssignmentCollectorPreservesProviderOrderAndNormalizesNullResults()
        {
            var first = Assignment("First", BrickAssignmentMode.DirectAttribute, BrickAssignmentSource.SourceAttribute);
            var second = Assignment("Second", BrickAssignmentMode.Convention, BrickAssignmentSource.Convention);
            var context = new BrickModelContext(Policy(), null);
            var providers = new IBrickRoleAssignmentProvider[]
            {
                new StaticProvider(first),
                null,
                new NullProvider(),
                new StaticProvider(second)
            };

            var assignments = BrickRoleAssignmentCollector.Collect(context, providers);

            Assert.Equal(new[] { first, second }, assignments);
        }

        [Fact]
        public void RoleAssignmentCollectorRequiresContextAndNormalizesNullProviders()
        {
            var context = new BrickModelContext(Policy(), null);

            Assert.Empty(BrickRoleAssignmentCollector.Collect(context, null));
            Assert.Throws<ArgumentNullException>(() => BrickRoleAssignmentCollector.Collect(null, null));
        }

        private static BrickPolicy Policy() =>
            new BrickPolicy(BrickPolicyId.From("Default"), "Default", null, null, BrickPermissionDefault.Allow, BrickEnforcementMode.Analyze);

        private static BrickRoleAssignment Assignment(string role, BrickAssignmentMode mode, BrickAssignmentSource source) =>
            new BrickRoleAssignment(
                new BrickElementSelector(BrickElementKind.Type, "*"),
                RoleId.From(role),
                mode,
                source,
                new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Direct),
                BrickAssignmentBehavior.Apply);

        private static BrickElement Element(string id, string displayName) =>
            new BrickElement(BrickElementId.From(id), BrickElementKind.Type, displayName);

        private sealed class StaticProvider : IBrickRoleAssignmentProvider
        {
            private readonly BrickRoleAssignment assignment;

            public StaticProvider(BrickRoleAssignment assignment)
            {
                this.assignment = assignment;
            }

            public IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context)
            {
                yield return assignment;
            }
        }

        private sealed class NullProvider : IBrickRoleAssignmentProvider
        {
            public IEnumerable<BrickRoleAssignment> GetAssignments(BrickModelContext context) => null;
        }
    }
}
