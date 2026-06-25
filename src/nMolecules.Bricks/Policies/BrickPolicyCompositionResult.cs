using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents the result of policy composition result processing in the Bricks pipeline.
/// </summary>
public sealed class BrickPolicyCompositionResult
    {
        public BrickPolicyCompositionResult(
            BrickPolicy policy,
            IEnumerable<BrickPolicyCompositionStep> steps,
            IEnumerable<BrickPolicyDocumentIssue> issues)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Steps = (steps ?? Enumerable.Empty<BrickPolicyCompositionStep>()).ToArray();
            Issues = (issues ?? Enumerable.Empty<BrickPolicyDocumentIssue>()).ToArray();
        }

        public BrickPolicy Policy { get; }
        public IReadOnlyList<BrickPolicyCompositionStep> Steps { get; }
        public IReadOnlyList<BrickPolicyDocumentIssue> Issues { get; }
    }
}
