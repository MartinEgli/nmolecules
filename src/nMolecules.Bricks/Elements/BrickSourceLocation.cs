using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Value object that identifies a source file location for dependencies, violations and reports.
    /// </summary>
    /// <remarks>
    /// Use source locations when tooling can map a Bricks element or dependency back to source code.
    /// Paths are stored as supplied by the caller; null paths are normalized to an empty string.
    /// </remarks>
    public readonly struct BrickSourceLocation : IEquatable<BrickSourceLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickSourceLocation"/> struct.
        /// </summary>
        /// <param name="path">The source file path. Null values are normalized to an empty string.</param>
        /// <param name="line">The one-based source line when available.</param>
        /// <param name="column">The one-based source column when available.</param>
        public BrickSourceLocation(string path, int line, int column)
        {
            Path = path ?? string.Empty;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Gets the source file path.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the source line.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the source column.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Determines whether this location equals another location by path, line and column.
        /// </summary>
        /// <param name="other">The other source location.</param>
        /// <returns><c>true</c> when path, line and column are equal; otherwise <c>false</c>.</returns>
        public bool Equals(BrickSourceLocation other) => string.Equals(Path, other.Path, StringComparison.Ordinal) && Line == other.Line && Column == other.Column;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is BrickSourceLocation other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Path ?? string.Empty) ^ Line.GetHashCode() ^ Column.GetHashCode();

        /// <summary>
        /// Compares two source locations for equality.
        /// </summary>
        public static bool operator ==(BrickSourceLocation left, BrickSourceLocation right) => left.Equals(right);

        /// <summary>
        /// Compares two source locations for inequality.
        /// </summary>
        public static bool operator !=(BrickSourceLocation left, BrickSourceLocation right) => !left.Equals(right);
    }
}
