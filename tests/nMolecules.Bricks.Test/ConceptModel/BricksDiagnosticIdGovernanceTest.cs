using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksDiagnosticIdGovernanceTest
    {
        [Fact]
        public void DiagnosticIdRangeContainsInclusiveBounds()
        {
            var range = new BrickDiagnosticIdRange("XMoleculesBricks0001", "XMoleculesBricks0099", "Role and dependency rules");

            Assert.True(range.Contains(RuleId.From("XMoleculesBricks0001")));
            Assert.True(range.Contains(RuleId.From("XMoleculesBricks0099")));
            Assert.False(range.Contains(RuleId.From("XMoleculesBricks0100")));
            Assert.Equal("Role and dependency rules", range.Description);
        }

        [Fact]
        public void DiagnosticIdRangeNormalizesNullValues()
        {
            var range = new BrickDiagnosticIdRange(null, null, null);

            Assert.Equal(string.Empty, range.FirstId.Value);
            Assert.Equal(string.Empty, range.LastId.Value);
            Assert.Equal(string.Empty, range.Description);
            Assert.False(range.Contains(default));
        }

        [Fact]
        public void DiagnosticIdGovernanceExposesV22RangesInStableOrder()
        {
            Assert.Equal(new[]
            {
                "Role and dependency rules",
                "Role resolution conflicts",
                "Policy configuration errors",
                "Member cardinality contracts",
                "Suppression and baseline issues",
                "Export and reporting issues",
                "Pack and profile bridge issues",
                "Runtime and wiring analysis issues"
            }, BrickDiagnosticIdGovernance.Ranges.Select(range => range.Description).ToArray());
        }

        [Theory]
        [InlineData("XMoleculesBricks0001", "Role and dependency rules")]
        [InlineData("XMoleculesBricks0100", "Role resolution conflicts")]
        [InlineData("XMoleculesBricks0200", "Policy configuration errors")]
        [InlineData("XMoleculesBricks0300", "Member cardinality contracts")]
        [InlineData("XMoleculesBricks0400", "Suppression and baseline issues")]
        [InlineData("XMoleculesBricks0500", "Export and reporting issues")]
        [InlineData("XMoleculesBricks0600", "Pack and profile bridge issues")]
        [InlineData("XMoleculesBricks0700", "Runtime and wiring analysis issues")]
        public void DiagnosticIdGovernanceResolvesKnownRange(string id, string description)
        {
            var range = BrickDiagnosticIdGovernance.FindRange(RuleId.From(id));

            Assert.NotNull(range);
            Assert.Equal(description, range.Description);
            Assert.True(BrickDiagnosticIdGovernance.IsKnown(RuleId.From(id)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("XMoleculesBricks0000")]
        [InlineData("XMoleculesBricks0800")]
        [InlineData("Other0001")]
        public void DiagnosticIdGovernanceRejectsUnknownOrMalformedIds(string id)
        {
            Assert.Null(BrickDiagnosticIdGovernance.FindRange(RuleId.From(id)));
            Assert.False(BrickDiagnosticIdGovernance.IsKnown(RuleId.From(id)));
        }
    }
}
