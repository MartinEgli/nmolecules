using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Classifies the expected architectural risk of applying a remediation option.
    /// </summary>
    public enum BrickRemediationRisk
    {
        /// <summary>The remediation is expected to be local, reversible, and policy-aligned.</summary>
        Low = 0,
        /// <summary>The remediation may affect ownership, public API, or multiple project areas.</summary>
        Medium = 1,
        /// <summary>The remediation may hide violations, weaken governance, or require broad migration.</summary>
        High = 2
    }
}
