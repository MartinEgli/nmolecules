using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents diagnostic id governance data used by rule evaluation, diagnostics, filtering, and diagnostic
/// id governance.
/// </summary>
public static class BrickDiagnosticIdGovernance
    {
        private static readonly BrickDiagnosticIdRange[] KnownRanges =
        {
            new BrickDiagnosticIdRange("XMoleculesBricks0001", "XMoleculesBricks0099", "Role and dependency rules"),
            new BrickDiagnosticIdRange("XMoleculesBricks0100", "XMoleculesBricks0199", "Role resolution conflicts"),
            new BrickDiagnosticIdRange("XMoleculesBricks0200", "XMoleculesBricks0299", "Policy configuration errors"),
            new BrickDiagnosticIdRange("XMoleculesBricks0300", "XMoleculesBricks0399", "Member cardinality contracts"),
            new BrickDiagnosticIdRange("XMoleculesBricks0400", "XMoleculesBricks0499", "Suppression and baseline issues"),
            new BrickDiagnosticIdRange("XMoleculesBricks0500", "XMoleculesBricks0599", "Export and reporting issues"),
            new BrickDiagnosticIdRange("XMoleculesBricks0600", "XMoleculesBricks0699", "Pack and profile bridge issues"),
            new BrickDiagnosticIdRange("XMoleculesBricks0700", "XMoleculesBricks0799", "Runtime and wiring analysis issues")
        };

        public static IReadOnlyList<BrickDiagnosticIdRange> Ranges => KnownRanges.ToArray();

        public static BrickDiagnosticIdRange FindRange(RuleId id) =>
            KnownRanges.FirstOrDefault(range => range.Contains(id));

        public static bool IsKnown(RuleId id) => FindRange(id) != null;
    }
}
