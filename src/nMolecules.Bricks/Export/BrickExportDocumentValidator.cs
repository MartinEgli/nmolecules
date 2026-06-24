using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public static class BrickExportDocumentValidator
    {
        public static readonly RuleId MissingDocumentRuleId = RuleId.From("XMoleculesBricks0500");
        public static readonly RuleId UnsupportedSchemaRuleId = RuleId.From("XMoleculesBricks0501");

        public static IReadOnlyList<BrickExportDocumentIssue> Validate(BrickRoleMapDocument document)
        {
            if (document == null)
            {
                return Missing("Role map");
            }

            return ValidateSchema("Role map", document.Schema, document.IsCurrentSchema, BrickRoleMapDocument.CurrentSchema);
        }

        public static IReadOnlyList<BrickExportDocumentIssue> Validate(BrickDependencyGraphDocument document)
        {
            if (document == null)
            {
                return Missing("Dependency graph");
            }

            return ValidateSchema("Dependency graph", document.Schema, document.IsCurrentSchema, BrickDependencyGraphDocument.CurrentSchema);
        }

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
