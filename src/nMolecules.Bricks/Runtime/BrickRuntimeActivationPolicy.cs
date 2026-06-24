using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickRuntimeActivationPolicy
    {
        public BrickRuntimeActivationPolicy(
            IEnumerable<RoleId> allowedActivationSiteRoles = null,
            bool requireJustification = true,
            bool enabled = true)
        {
            AllowedActivationSiteRoles = (allowedActivationSiteRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        public IReadOnlyList<RoleId> AllowedActivationSiteRoles { get; }
        public bool RequireJustification { get; }
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("Infrastructure");
            yield return RoleId.From("Platform");
        }
    }
}
