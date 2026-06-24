using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickConfigurationSource
    {
        public static BrickConfigurationSource Generated { get; } = new BrickConfigurationSource(
            "generated",
            BrickConfigurationSourceKind.Generated,
            "Generated configuration.");

        public BrickConfigurationSource(
            string id,
            BrickConfigurationSourceKind kind,
            string description = null)
        {
            Id = id ?? string.Empty;
            Kind = kind;
            Description = description ?? string.Empty;
        }

        public string Id { get; }
        public BrickConfigurationSourceKind Kind { get; }
        public string Description { get; }
        public int Precedence => BrickConfigurationPrecedence.Rank(Kind);
    }
}
