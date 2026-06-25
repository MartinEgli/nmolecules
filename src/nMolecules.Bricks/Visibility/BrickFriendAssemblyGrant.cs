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
        public const string DependencyKind = "FriendAssembly";

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

        public BrickElement ExposingAssembly { get; }
        public BrickElement FriendAssembly { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

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
