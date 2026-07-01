using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Provides built-in Bricks defaults for dependency coverage targets and coverage reporting.
/// </summary>
public static class BrickBuiltInDependencyCoverageTargets
    {
        /// <summary>
        /// Gets the Dependency Registration value used by Bricks developer tooling.
        /// </summary>
        public static BrickDependencyCoverageTarget DependencyRegistration => Target(
            BrickDependencyRegistration.DependencyKind,
            BrickDependencyLayer.Runtime,
            BrickEvidenceLevel.AnalyzerInferred,
            "Dependency registrations are only visible when analyzers or explicit configuration observe the composition root.");

        /// <summary>
        /// Gets the Friend Assembly value used by Bricks developer tooling.
        /// </summary>
        public static BrickDependencyCoverageTarget FriendAssembly => Target(
            BrickFriendAssemblyGrant.DependencyKind,
            BrickDependencyLayer.Visibility,
            BrickEvidenceLevel.CompilerConfirmed,
            "Friend assembly grants are compiler-visible visibility dependencies.");

        /// <summary>
        /// Gets the Reflection Access value used by Bricks developer tooling.
        /// </summary>
        public static BrickDependencyCoverageTarget ReflectionAccess => Target(
            BrickReflectionAccess.DependencyKind,
            BrickDependencyLayer.Runtime,
            BrickEvidenceLevel.RuntimeInferred,
            "Reflection access can bypass static structure and may be only partially observable.");

        /// <summary>
        /// Gets the Runtime Activation value used by Bricks developer tooling.
        /// </summary>
        public static BrickDependencyCoverageTarget RuntimeActivation => Target(
            BrickRuntimeActivation.DependencyKind,
            BrickDependencyLayer.Runtime,
            BrickEvidenceLevel.RuntimeInferred,
            "Runtime activation edges depend on runtime or configuration evidence.");

        /// <summary>
        /// Gets the Type Reference value used by Bricks developer tooling.
        /// </summary>
        public static BrickDependencyCoverageTarget TypeReference => Target(
            BrickDependencyKinds.TypeReference,
            BrickDependencyLayer.Static,
            BrickEvidenceLevel.CompilerConfirmed,
            "Type references are the baseline compiler-confirmed static dependency kind.");

        /// <summary>
        /// Gets the All value used by Bricks developer tooling.
        /// </summary>
        public static IReadOnlyList<BrickDependencyCoverageTarget> All => new[]
        {
            DependencyRegistration,
            FriendAssembly,
            ReflectionAccess,
            RuntimeActivation,
            TypeReference
        };

        private static BrickDependencyCoverageTarget Target(
            string kindId,
            BrickDependencyLayer layer,
            BrickEvidenceLevel minimumEvidenceLevel,
            string rationale) =>
            new BrickDependencyCoverageTarget(
                BrickDependencyKindId.From(kindId),
                layer,
                minimumEvidenceLevel,
                true,
                rationale);
    }
}
