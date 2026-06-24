using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
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
