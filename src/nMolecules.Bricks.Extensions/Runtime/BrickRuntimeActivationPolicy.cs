using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Defines the policy data used to govern runtime dependency registration, activation, and wiring checks.
/// </summary>
public sealed class BrickRuntimeActivationPolicy
    {
        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickRuntimeActivationPolicy(
            IEnumerable<RoleId> allowedActivationSiteRoles = null,
            bool requireJustification = true,
            bool enabled = true)
        {
            AllowedActivationSiteRoles = (allowedActivationSiteRoles ?? Defaults()).Distinct().ToArray();
            RequireJustification = requireJustification;
            Enabled = enabled;
        }

        /// <summary>
        /// Gets a value indicating whether Allowed Activation Site Roles applies.
        /// </summary>
        public IReadOnlyList<RoleId> AllowedActivationSiteRoles { get; }
        /// <summary>
        /// Gets a value indicating whether Require Justification applies.
        /// </summary>
        public bool RequireJustification { get; }
        /// <summary>
        /// Gets a value indicating whether Enabled applies.
        /// </summary>
        public bool Enabled { get; }

        private static IEnumerable<RoleId> Defaults()
        {
            yield return RoleId.From("Infrastructure");
            yield return RoleId.From("Platform");
        }
    }
}
