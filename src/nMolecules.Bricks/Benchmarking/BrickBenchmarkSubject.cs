using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Identifies the central Bricks capability measured by a benchmark case.
    /// </summary>
    public enum BrickBenchmarkSubject
    {
        /// <summary>Measures deterministic rule evaluation.</summary>
        RuleEvaluation = 0,
        /// <summary>Measures role resolution and role assignment behavior.</summary>
        RoleResolution = 1,
        /// <summary>Measures policy composition and import handling.</summary>
        PolicyComposition = 2,
        /// <summary>Measures projection of violations into reportable state.</summary>
        ViolationProjection = 3,
        /// <summary>Measures runtime dependency evaluation surfaces.</summary>
        RuntimeDependencyEvaluation = 4,
        /// <summary>Measures report serialization cost.</summary>
        ReportSerialization = 5
    }
}
