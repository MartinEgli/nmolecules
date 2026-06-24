using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public static class BrickPolicyDocumentValidator
    {
        public static readonly RuleId MissingDocumentRuleId = RuleId.From("XMoleculesBricks0200");
        public static readonly RuleId UnsupportedSchemaRuleId = RuleId.From("XMoleculesBricks0201");

        public static IReadOnlyList<BrickPolicyDocumentIssue> Validate(BrickPolicyDocument document)
        {
            if (document == null)
            {
                return new[]
                {
                    new BrickPolicyDocumentIssue(
                        MissingDocumentRuleId,
                        BrickSeverity.Error,
                        "Policy document is required.")
                };
            }

            if (!document.IsCurrentSchema)
            {
                return new[]
                {
                    new BrickPolicyDocumentIssue(
                        UnsupportedSchemaRuleId,
                        BrickSeverity.Error,
                        $"Policy schema '{document.Schema}' is not supported. Expected '{BrickPolicyDocument.CurrentSchema}'.")
                };
            }

            return Enumerable.Empty<BrickPolicyDocumentIssue>().ToArray();
        }
    }
}
