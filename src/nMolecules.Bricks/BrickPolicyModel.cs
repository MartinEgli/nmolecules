using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    public sealed class BrickAlias
    {
        public BrickAlias(
            string name,
            BrickElementSelector? selector,
            RoleId canonicalRoleId,
            BrickAssignmentPrecedence precedence,
            BrickAssignmentBehavior behavior,
            string reason = null)
        {
            Name = name ?? string.Empty;
            Selector = selector ?? default;
            CanonicalRoleId = canonicalRoleId;
            Precedence = precedence;
            Behavior = behavior;
            Reason = reason;
        }

        public string Name { get; }
        public BrickElementSelector Selector { get; }
        public RoleId CanonicalRoleId { get; }
        public BrickAssignmentPrecedence Precedence { get; }
        public BrickAssignmentBehavior Behavior { get; }
        public string Reason { get; }
    }

    public sealed class BrickPolicyDocument
    {
        public const string CurrentSchema = "NMolecules.Bricks.Policy/1.0";

        public BrickPolicyDocument(BrickPolicy policy)
            : this(policy, CurrentSchema)
        {
        }

        public BrickPolicyDocument(BrickPolicy policy, string schema)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Schema = schema ?? string.Empty;
        }

        public string Schema { get; }
        public BrickPolicy Policy { get; }
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }

    public sealed class BrickPolicyDocumentIssue
    {
        public BrickPolicyDocumentIssue(RuleId ruleId, BrickSeverity severity, string message)
        {
            RuleId = ruleId;
            Severity = severity;
            Message = message ?? string.Empty;
        }

        public RuleId RuleId { get; }
        public BrickSeverity Severity { get; }
        public string Message { get; }
    }

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
