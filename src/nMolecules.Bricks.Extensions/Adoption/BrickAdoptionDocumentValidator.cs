using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Validates adoption document validator input and returns structured issues that consumers can report or
/// fix.
/// </summary>
public static class BrickAdoptionDocumentValidator
    {
        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public static readonly RuleId MissingDocumentRuleId = RuleId.From("XMoleculesBricks0400");
        /// <summary>
        /// Gets the diagnostic rule id used when reporting this Bricks validation condition.
        /// </summary>
        public static readonly RuleId UnsupportedSchemaRuleId = RuleId.From("XMoleculesBricks0401");

        /// <summary>
        /// Validates the Bricks document and returns issues that developers can report or fix.
        /// </summary>
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
