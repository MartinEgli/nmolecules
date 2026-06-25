using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Value object that represents source location data for core elements, dependencies, violations, and source
/// locations.
/// </summary>
public readonly struct BrickSourceLocation : IEquatable<BrickSourceLocation>
    {
        public BrickSourceLocation(string path, int line, int column)
        {
            Path = path ?? string.Empty;
            Line = line;
            Column = column;
        }

        public string Path { get; }
        public int Line { get; }
        public int Column { get; }
        public bool Equals(BrickSourceLocation other) => string.Equals(Path, other.Path, StringComparison.Ordinal) && Line == other.Line && Column == other.Column;
        public override bool Equals(object obj) => obj is BrickSourceLocation other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Path ?? string.Empty) ^ Line.GetHashCode() ^ Column.GetHashCode();
        public static bool operator ==(BrickSourceLocation left, BrickSourceLocation right) => left.Equals(right);
        public static bool operator !=(BrickSourceLocation left, BrickSourceLocation right) => !left.Equals(right);
    }
}
