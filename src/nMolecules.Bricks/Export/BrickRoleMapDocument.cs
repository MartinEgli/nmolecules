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
        public const string CurrentSchema = "NMolecules.Bricks.RoleMap/1.0";

        public BrickRoleMapDocument(DateTimeOffset generatedAt, IEnumerable<BrickRoleMapEntry> entries)
            : this(generatedAt, entries, CurrentSchema)
        {
        }

        public BrickRoleMapDocument(DateTimeOffset generatedAt, IEnumerable<BrickRoleMapEntry> entries, string schema)
        {
            GeneratedAt = generatedAt;
            Entries = (entries ?? Enumerable.Empty<BrickRoleMapEntry>())
                .OrderBy(entry => entry.Element.Id.Value, StringComparer.Ordinal)
                .ToArray();
            Schema = schema ?? string.Empty;
        }

        public string Schema { get; }
        public DateTimeOffset GeneratedAt { get; }
        public IReadOnlyList<BrickRoleMapEntry> Entries { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);

        public static BrickRoleMapDocument FromResolvedRoles(DateTimeOffset generatedAt, IEnumerable<BrickResolvedRoles> resolvedRoles, string schema = CurrentSchema) =>
            new BrickRoleMapDocument(
                generatedAt,
                (resolvedRoles ?? Enumerable.Empty<BrickResolvedRoles>()).Select(BrickRoleMapEntry.FromResolvedRoles),
                schema);
    }
}
