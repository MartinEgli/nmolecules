using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Defines the document shape used to exchange policy data between Bricks tools.
    /// </summary>
    /// <remarks>
    /// Wrap a <see cref="BrickPolicy"/> in this document when serializing, validating or importing
    /// policy files. The schema value allows future document versions to be detected.
    /// </remarks>
    public sealed class BrickPolicyDocument
    {
        /// <summary>
        /// The current policy document schema identifier.
        /// </summary>
        public const string CurrentSchema = "NMolecules.Bricks.Policy/1.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyDocument"/> class using the current schema.
        /// </summary>
        /// <param name="policy">The policy contained in the document.</param>
        public BrickPolicyDocument(BrickPolicy policy)
            : this(policy, CurrentSchema)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyDocument"/> class.
        /// </summary>
        /// <param name="policy">The policy contained in the document.</param>
        /// <param name="schema">The document schema identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
        public BrickPolicyDocument(BrickPolicy policy, string schema)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Schema = schema ?? string.Empty;
        }

        /// <summary>
        /// Gets the document schema identifier.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the policy contained in the document.
        /// </summary>
        public BrickPolicy Policy { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Schema"/> matches <see cref="CurrentSchema"/>.
        /// </summary>
        public bool IsCurrentSchema => string.Equals(Schema, CurrentSchema, StringComparison.Ordinal);
    }
}
