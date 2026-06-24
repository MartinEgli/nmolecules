using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Versioned document containing AI-ready comments for deterministic Bricks violations.
    /// </summary>
    public sealed class BrickAiCommentDocument
    {
        /// <summary>Current JSON schema identifier for AI comment documents.</summary>
        public const string CurrentSchema = "NMolecules.Bricks.AIComment/1.0";

        /// <summary>
        /// Creates a comment document using the current schema.
        /// </summary>
        public BrickAiCommentDocument(DateTimeOffset generatedAt, IEnumerable<BrickAiViolationComment> comments)
            : this(generatedAt, comments, CurrentSchema)
        {
        }

        /// <summary>
        /// Creates a comment document with an explicit schema identifier.
        /// </summary>
        public BrickAiCommentDocument(DateTimeOffset generatedAt, IEnumerable<BrickAiViolationComment> comments, string schema)
        {
            GeneratedAt = generatedAt;
            Schema = schema ?? string.Empty;
            Comments = (comments ?? Enumerable.Empty<BrickAiViolationComment>())
                .OrderBy(comment => comment.RuleId?.Value ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(comment => comment.Violation.Source.Id.Value, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>Schema identifier used to serialize the document.</summary>
        public string Schema { get; }
        /// <summary>Time the document was generated.</summary>
        public DateTimeOffset GeneratedAt { get; }
        /// <summary>AI-ready comments sorted by rule id and source element id.</summary>
        public IReadOnlyList<BrickAiViolationComment> Comments { get; }
        /// <summary>Indicates whether the document uses <see cref="CurrentSchema"/>.</summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
