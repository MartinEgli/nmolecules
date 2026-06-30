using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Represents the result of composing a root policy with its imports.
    /// </summary>
    public sealed class BrickPolicyCompositionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyCompositionResult"/> class.
        /// </summary>
        /// <param name="policy">The composed policy.</param>
        /// <param name="steps">The composition steps that were attempted.</param>
        /// <param name="issues">Issues discovered during composition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
        public BrickPolicyCompositionResult(
            BrickPolicy policy,
            IEnumerable<BrickPolicyCompositionStep> steps,
            IEnumerable<BrickPolicyDocumentIssue> issues)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Steps = (steps ?? Enumerable.Empty<BrickPolicyCompositionStep>()).ToArray();
            Issues = (issues ?? Enumerable.Empty<BrickPolicyDocumentIssue>()).ToArray();
        }

        /// <summary>
        /// Gets the composed policy.
        /// </summary>
        public BrickPolicy Policy { get; }

        /// <summary>
        /// Gets the composition steps that were attempted.
        /// </summary>
        public IReadOnlyList<BrickPolicyCompositionStep> Steps { get; }

        /// <summary>
        /// Gets issues discovered during composition.
        /// </summary>
        public IReadOnlyList<BrickPolicyDocumentIssue> Issues { get; }
    }
}
