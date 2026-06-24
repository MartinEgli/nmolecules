using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public static class BrickAdoptionDocumentValidator
    {
        public static readonly RuleId MissingDocumentRuleId = RuleId.From("XMoleculesBricks0400");
        public static readonly RuleId UnsupportedSchemaRuleId = RuleId.From("XMoleculesBricks0401");

        public static IReadOnlyList<BrickAdoptionDocumentIssue> Validate(BrickAdoptionDocument document)
        {
            if (document == null)
            {
                return new[]
                {
                    new BrickAdoptionDocumentIssue(
                        MissingDocumentRuleId,
                        BrickSeverity.Error,
                        "Adoption document is required.")
                };
            }

            if (!document.IsCurrentSchema)
            {
                return new[]
                {
                    new BrickAdoptionDocumentIssue(
                        UnsupportedSchemaRuleId,
                        BrickSeverity.Error,
                        $"Adoption schema '{document.Schema}' is not supported. Expected '{BrickAdoptionDocument.CurrentSchema}'.")
                };
            }

            return Enumerable.Empty<BrickAdoptionDocumentIssue>().ToArray();
        }
    }
}
