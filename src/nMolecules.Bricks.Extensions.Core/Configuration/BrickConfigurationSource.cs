using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents configuration source data used by configuration source precedence and resolved Bricks
/// settings.
/// </summary>
public sealed class BrickConfigurationSource
    {
        /// <summary>
        /// Gets the Generated value used by Bricks developer tooling.
        /// </summary>
        public static BrickConfigurationSource Generated { get; } = new BrickConfigurationSource(
            "generated",
            BrickConfigurationSourceKind.Generated,
            "Generated configuration.");

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickConfigurationSource(
            string id,
            BrickConfigurationSourceKind kind,
            string description = null)
        {
            Id = id ?? string.Empty;
            Kind = kind;
            Description = description ?? string.Empty;
        }

        /// <summary>
        /// Gets the Id value used by Bricks developer tooling.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the Kind value used by Bricks developer tooling.
        /// </summary>
        public BrickConfigurationSourceKind Kind { get; }
        /// <summary>
        /// Gets the Description value used by Bricks developer tooling.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the Precedence value used by Bricks developer tooling.
        /// </summary>
        public int Precedence => BrickConfigurationPrecedence.Rank(Kind);
    }
}
