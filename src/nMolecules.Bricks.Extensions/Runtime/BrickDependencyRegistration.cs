using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents dependency registration data used by runtime dependency registration, activation, and wiring
/// checks.
/// </summary>
public sealed class BrickDependencyRegistration
    {
        /// <summary>
        /// Gets the Dependency Kind value used by Bricks developer tooling.
        /// </summary>
        public const string DependencyKind = "DependencyRegistration";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickDependencyRegistration(
            BrickElement registrationSite,
            BrickElement serviceType,
            BrickElement implementationType,
            string lifetime = null,
            string justification = null,
            BrickEvidenceLevel evidenceLevel = BrickEvidenceLevel.AnalyzerInferred)
        {
            RegistrationSite = registrationSite ?? throw new ArgumentNullException(nameof(registrationSite));
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime ?? string.Empty;
            Justification = justification;
            EvidenceLevel = evidenceLevel;
        }

        /// <summary>
        /// Gets the Registration Site value used by Bricks developer tooling.
        /// </summary>
        public BrickElement RegistrationSite { get; }
        /// <summary>
        /// Gets the Service Type value used by Bricks developer tooling.
        /// </summary>
        public BrickElement ServiceType { get; }
        /// <summary>
        /// Gets the Implementation Type value used by Bricks developer tooling.
        /// </summary>
        public BrickElement ImplementationType { get; }
        /// <summary>
        /// Gets the Lifetime value used by Bricks developer tooling.
        /// </summary>
        public string Lifetime { get; }
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
                RegistrationSite,
                ImplementationType,
                BrickDependencyKindId.From(DependencyKind),
                BrickScope.Type,
                BrickDependencyLayer.Runtime,
                BrickDependencyStrength.Direct,
                EvidenceLevel,
                detail: Lifetime);
    }
}
