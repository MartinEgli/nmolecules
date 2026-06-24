using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickPolicyCompositionStep
    {
        public BrickPolicyCompositionStep(BrickPolicyId policyId, BrickPolicyImportMode mode, bool applied)
        {
            PolicyId = policyId;
            Mode = mode;
            Applied = applied;
        }

        public BrickPolicyId PolicyId { get; }
        public BrickPolicyImportMode Mode { get; }
        public bool Applied { get; }
    }
}
