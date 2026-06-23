using System;

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
}
