using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Represents a reserved diagnostic id range for Bricks diagnostics and rule identifiers.
    /// </summary>
    /// <remarks>
    /// Use ranges to keep analyzer diagnostics, runtime validation issues and generated reports
    /// within documented id blocks.
    /// </remarks>
    public sealed class BrickDiagnosticIdRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickDiagnosticIdRange"/> class.
        /// </summary>
        /// <param name="firstId">The first id included in the range.</param>
        /// <param name="lastId">The last id included in the range.</param>
        /// <param name="description">A human-readable description of the reserved range.</param>
        public BrickDiagnosticIdRange(string firstId, string lastId, string description)
        {
            FirstId = RuleId.From(firstId);
            LastId = RuleId.From(lastId);
            Description = description ?? string.Empty;
        }

        /// <summary>
        /// Gets the first id included in the range.
        /// </summary>
        public RuleId FirstId { get; }

        /// <summary>
        /// Gets the last id included in the range.
        /// </summary>
        public RuleId LastId { get; }

        /// <summary>
        /// Gets the human-readable range description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Determines whether the supplied id is included in this range.
        /// </summary>
        /// <param name="id">The id to test.</param>
        /// <returns><c>true</c> when the id is within the range; otherwise <c>false</c>.</returns>
        public bool Contains(RuleId id) =>
            !id.IsEmpty &&
            string.CompareOrdinal(id.Value, FirstId.Value) >= 0 &&
            string.CompareOrdinal(id.Value, LastId.Value) <= 0;
    }
}
