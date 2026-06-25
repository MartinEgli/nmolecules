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
        public const string DependencyKind = "DependencyRegistration";

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

        public BrickElement RegistrationSite { get; }
        public BrickElement ServiceType { get; }
        public BrickElement ImplementationType { get; }
        public string Lifetime { get; }
        public string Justification { get; }
        public BrickEvidenceLevel EvidenceLevel { get; }

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
