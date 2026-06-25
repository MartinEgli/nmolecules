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
        public BrickConfigurationEntry(
            string key,
            string value,
            BrickConfigurationSource source)
        {
            Key = key ?? string.Empty;
            Value = value ?? string.Empty;
            Source = source ?? BrickConfigurationSource.Generated;
        }

        public string Key { get; }
        public string Value { get; }
        public BrickConfigurationSource Source { get; }
    }
}
