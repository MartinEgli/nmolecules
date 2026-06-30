using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the document shape used to exchange role map document data between Bricks tools.
/// </summary>
public sealed class BrickRoleMapDocument
    {
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.RoleMap/1.0";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRoleMapDocument(DateTimeOffset generatedAt, IEnumerable<BrickRoleMapEntry> entries)
            : this(generatedAt, entries, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRoleMapDocument(DateTimeOffset generatedAt, IEnumerable<BrickRoleMapEntry> entries, string schema)
        {
            GeneratedAt = generatedAt;
            Entries = (entries ?? Enumerable.Empty<BrickRoleMapEntry>())
                .OrderBy(entry => entry.Element.Id.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public string Schema { get; }
        /// <summary>
        /// Gets the timestamp associated with this Bricks model object.
        /// </summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>
        /// Gets the Entries value used by Bricks developer tooling.
        /// </summary>
        public IReadOnlyList<BrickRoleMapEntry> Entries { get; }
        /// <summary>
        /// Gets the schema identifier used for this Bricks document format.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
        public static BrickRoleMapDocument FromResolvedRoles(DateTimeOffset generatedAt, IEnumerable<BrickResolvedRoles> resolvedRoles, string schema = CurrentSchema) =>
            new BrickRoleMapDocument(
                generatedAt,
                (resolvedRoles ?? Enumerable.Empty<BrickResolvedRoles>()).Select(BrickRoleMapEntry.FromResolvedRoles),
                schema);
    }
}
