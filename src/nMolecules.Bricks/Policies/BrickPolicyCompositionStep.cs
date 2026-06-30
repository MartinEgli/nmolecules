using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes one import step performed during policy composition.
    /// </summary>
    public sealed class BrickPolicyCompositionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickPolicyCompositionStep"/> class.
        /// </summary>
        /// <param name="policyId">The policy id involved in the step.</param>
        /// <param name="mode">The import mode used for the step.</param>
        /// <param name="applied">Whether the step was applied.</param>
        public BrickPolicyCompositionStep(BrickPolicyId policyId, BrickPolicyImportMode mode, bool applied)
        {
            PolicyId = policyId;
            Mode = mode;
            Applied = applied;
        }

        /// <summary>
        /// Gets the policy id involved in the step.
        /// </summary>
        public BrickPolicyId PolicyId { get; }

        /// <summary>
        /// Gets the import mode used for the step.
        /// </summary>
        public BrickPolicyImportMode Mode { get; }

        /// <summary>
        /// Gets a value indicating whether the step was applied.
        /// </summary>
        public bool Applied { get; }
    }
}
