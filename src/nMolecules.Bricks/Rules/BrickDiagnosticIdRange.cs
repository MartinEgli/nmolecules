using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickDiagnosticIdRange
    {
        public BrickDiagnosticIdRange(string firstId, string lastId, string description)
        {
            FirstId = RuleId.From(firstId);
            LastId = RuleId.From(lastId);
            Description = description ?? string.Empty;
        }

        public RuleId FirstId { get; }
        public RuleId LastId { get; }
        public string Description { get; }

        public bool Contains(RuleId id) =>
            !id.IsEmpty &&
            string.CompareOrdinal(id.Value, FirstId.Value) >= 0 &&
            string.CompareOrdinal(id.Value, LastId.Value) <= 0;
    }
}
