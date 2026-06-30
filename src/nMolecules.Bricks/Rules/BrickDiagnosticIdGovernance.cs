using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Provides the documented Bricks diagnostic id ranges.
    /// </summary>
    /// <remarks>
    /// Use this helper when custom tooling needs to validate whether a diagnostic or rule id belongs
    /// to a known Bricks range before emitting reports or policy documents.
    /// </remarks>
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

        /// <summary>
        /// Gets the known Bricks diagnostic id ranges.
        /// </summary>
        public static IReadOnlyList<BrickDiagnosticIdRange> Ranges => KnownRanges.ToArray();

        /// <summary>
        /// Finds the range that contains the supplied id.
        /// </summary>
        /// <param name="id">The id to classify.</param>
        /// <returns>The matching range, or <c>null</c> when the id is unknown.</returns>
        public static BrickDiagnosticIdRange FindRange(RuleId id) =>
            KnownRanges.FirstOrDefault(range => range.Contains(id));

        /// <summary>
        /// Determines whether the supplied id belongs to a known Bricks diagnostic range.
        /// </summary>
        /// <param name="id">The id to classify.</param>
        /// <returns><c>true</c> when the id belongs to a known range; otherwise <c>false</c>.</returns>
        public static bool IsKnown(RuleId id) => FindRange(id) != null;
    }
}
