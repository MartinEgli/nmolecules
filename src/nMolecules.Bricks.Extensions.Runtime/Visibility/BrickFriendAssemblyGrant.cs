using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents friend assembly grant data used by assembly visibility and friend-assembly policy checks.
/// </summary>
public sealed class BrickFriendAssemblyGrant
    {
        /// <summary>
        /// Gets the Dependency Kind value used by Bricks developer tooling.
        /// </summary>
        public const string DependencyKind = "FriendAssembly";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickFriendAssemblyGrant(
            BrickElement exposingAssembly,
            BrickElement friendAssembly,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.CompilerConfirmed)
        {
            ExposingAssembly = exposingAssembly ?? throw new ArgumentNullException(nameof(exposingAssembly));
            FriendAssembly = friendAssembly ?? throw new ArgumentNullException(nameof(friendAssembly));
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        /// <summary>
        /// Gets the Exposing Assembly value used by Bricks developer tooling.
        /// </summary>
        public BrickElement ExposingAssembly { get; }
        /// <summary>
        /// Gets the Friend Assembly value used by Bricks developer tooling.
        /// </summary>
        public BrickElement FriendAssembly { get; }
        /// <summary>
        /// Gets the Justification value used by Bricks developer tooling.
        /// </summary>
        public string Justification { get; }
        /// <summary>
        /// Gets the Evidence Level value used by Bricks developer tooling.
        /// </summary>
        public BrickEvidenceLevel EvidenceLevel { get; }

        /// <summary>
        /// Converts this Bricks model object to the corresponding core representation.
        /// </summary>
        public BrickDependency ToDependency() =>
            new BrickDependency(
                FriendAssembly,
                ExposingAssembly,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Assembly,
                BrickDependencyLayer.Visibility,
                BrickDependencyStrength.Direct,
                EvidenceLevel,
                detail: Justification);
    }
}
