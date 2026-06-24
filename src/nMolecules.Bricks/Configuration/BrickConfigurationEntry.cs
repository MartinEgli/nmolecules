using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
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
