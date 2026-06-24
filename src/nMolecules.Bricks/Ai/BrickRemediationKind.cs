using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Describes the structural remediation category an AI explanation may suggest for a deterministic Bricks violation.
    /// </summary>
    public enum BrickRemediationKind
    {
        /// <summary>Introduce or reuse an abstraction so the source does not depend on a concrete target.</summary>
        IntroduceContract = 0,
        /// <summary>Move the source or target element to a role, namespace, or assembly that matches the policy.</summary>
        MoveElement = 1,
        /// <summary>Split an overloaded role into more precise roles before applying policy.</summary>
        SplitRole = 2,
        /// <summary>Add an explicit port and adapter boundary between source and target roles.</summary>
        AddPortAndAdapter = 3,
        /// <summary>Reverse or redirect a dependency so it follows the intended architecture direction.</summary>
        ChangeDependencyDirection = 4,
        /// <summary>Move wiring or construction logic to the composition root.</summary>
        MoveToCompositionRoot = 5,
        /// <summary>Change policy only through explicit review when the policy is too narrow or wrong.</summary>
        AdjustPolicy = 6,
        /// <summary>Add a reviewed suppression for an intentional and bounded exception.</summary>
        AddSuppression = 7,
        /// <summary>Add a reviewed baseline entry for an accepted existing violation.</summary>
        AddBaselineEntry = 8,
        /// <summary>Rename or reclassify an element when the current role assignment is misleading.</summary>
        RenameOrReclassifyElement = 9
    }
}
