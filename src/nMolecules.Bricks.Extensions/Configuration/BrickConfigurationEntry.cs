using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents configuration entry data used by configuration source precedence and resolved Bricks settings.
/// </summary>
public sealed class BrickConfigurationEntry
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickConfigurationEntry(
            string key,
            string value,
            BrickConfigurationSource source)
        {
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
            Source = source ?? BrickConfigurationSource.Generated;
        }

        /// <summary>
        /// Gets the Key value used by Bricks developer tooling.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Gets the Value value used by Bricks developer tooling.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Gets the Source value used by Bricks developer tooling.
        /// </summary>
        public BrickConfigurationSource Source { get; }
    }
}
