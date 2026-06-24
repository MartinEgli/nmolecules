using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRuntimeWiringPolicy
    {
        public BrickRuntimeWiringPolicy(
            IEnumerable<RoleId> allowedRegistrationSiteRoles = null,
            bool requireJustification = false,
            bool enabled = true)
        {
            AllowedRegistrationSiteRoles = (allowedRegistrationSiteRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public IReadOnlyList<RoleId> AllowedRegistrationSiteRoles { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("Infrastructure");
            yield return RoleId.From("Platform");
        }
    }
}
