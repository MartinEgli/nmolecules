using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Validates export document validator input and returns structured issues that consumers can report or fix.
/// </summary>
public static class BrickExportDocumentValidator
    {
        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public static readonly RuleId MissingDocumentRuleId = RuleId.From("XMoleculesBricks0500");
        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public static readonly RuleId UnsupportedSchemaRuleId = RuleId.From("XMoleculesBricks0501");

        /// <summary>
        /// Validates the Bricks document and returns issues that developers can report or fix.
        /// </summary>
        public static IReadOnlyList<BrickExportDocumentIssue> Validate(BrickRoleMapDocument document)
        {
            if (document == null)
            {
                return Missing("Role map");
            }

            return ValidateSchema("Role map", document.Schema, document.IsCurrentSchema, BrickRoleMapDocument.CurrentSchema);
        }

        /// <summary>
        /// Validates the Bricks document and returns issues that developers can report or fix.
        /// </summary>
        public static IReadOnlyList<BrickExportDocumentIssue> Validate(BrickDependencyGraphDocument document)
        {
            if (document == null)
            {
                return Missing("Dependency graph");
            }

            return ValidateSchema("Dependency graph", document.Schema, document.IsCurrentSchema, BrickDependencyGraphDocument.CurrentSchema);
        }

        /// <summary>
        /// Validates the Bricks document and returns issues that developers can report or fix.
        /// </summary>
        public static IReadOnlyList<BrickExportDocumentIssue> Validate(BrickResolutionTraceDocument document)
        {
            if (document == null)
            {
                return Missing("Resolution trace");
            }

            return ValidateSchema("Resolution trace", document.Schema, document.IsCurrentSchema, BrickResolutionTraceDocument.CurrentSchema);
        }

        private static IReadOnlyList<BrickExportDocumentIssue> Missing(string documentName) =>
            new[]
            {
                new BrickExportDocumentIssue(
                    MissingDocumentRuleId,
                    BrickSeverity.Error,
                    $"{documentName} export document is required.")
            };

        private static IReadOnlyList<BrickExportDocumentIssue> ValidateSchema(string documentName, string schema, bool isCurrentSchema, string expectedSchema)
        {
            if (!isCurrentSchema)
            {
                return new[]
                {
                    new BrickExportDocumentIssue(
                        UnsupportedSchemaRuleId,
                        BrickSeverity.Error,
                        $"{documentName} export schema '{schema}' is not supported. Expected '{expectedSchema}'.")
                };
            }

            return Enumerable.Empty<BrickExportDocumentIssue>().ToArray();
        }
    }
}
