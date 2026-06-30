using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Identifies an architectural element that can receive roles, participate in dependencies and appear in Bricks violations.
    /// </summary>
    /// <remarks>
    /// Use <see cref="BrickElement"/> as the stable node model when connecting analyzer results, runtime dependency scans
    /// and violation reports. The element is intentionally small so tools can create it from assemblies, namespaces,
    /// folders, projects or individual types without depending on Roslyn-specific objects.
    ///
    /// Example: see
    /// <c>../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/PolicyAndResolutionExamples.cs</c>.
    /// </remarks>
    public sealed class BrickElement
    {
        /// <summary>
        /// Creates an architectural element with optional source metadata.
        /// </summary>
        /// <param name="id">Stable element identifier used to correlate roles, dependencies and violations.</param>
        /// <param name="kind">Kind of element represented by the instance.</param>
        /// <param name="displayName">Human-readable name used in diagnostics and reports. <c>null</c> is normalised to an empty string.</param>
        /// <param name="assemblyName">Optional assembly name that contributed the element.</param>
        /// <param name="namespaceName">Optional namespace associated with the element.</param>
        /// <param name="fullName">Optional fully-qualified element name.</param>
        /// <param name="origin">Origin that explains where the element was discovered.</param>
        /// <param name="source">Source system that provided the element.</param>
        public BrickElement(
            BrickElementId id,
            BrickElementKind kind,
            string displayName,
            string assemblyName = null,
            string namespaceName = null,
            string fullName = null,
            BrickElementOrigin origin = BrickElementOrigin.Unknown,
            BrickElementSource source = BrickElementSource.Unknown)
        {
            Id = id;
            Kind = kind;
            DisplayName = displayName ?? string.Empty;
            AssemblyName = assemblyName;
            NamespaceName = namespaceName;
            FullName = fullName;
            Origin = origin;
            Source = source;
        }

        /// <summary>
        /// Stable element identifier used as the correlation key for roles, dependencies and violations.
        /// </summary>
        public BrickElementId Id { get; }

        /// <summary>
        /// Describes the architectural granularity of the element.
        /// </summary>
        public BrickElementKind Kind { get; }

        /// <summary>
        /// Human-readable name for diagnostics, documentation and reports.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Optional assembly name associated with the element.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Optional namespace associated with the element.
        /// </summary>
        public string NamespaceName { get; }

        /// <summary>
        /// Optional fully-qualified element name.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Origin that explains how the element entered the Bricks model.
        /// </summary>
        public BrickElementOrigin Origin { get; }

        /// <summary>
        /// Source system or discovery mechanism that produced the element.
        /// </summary>
        public BrickElementSource Source { get; }
    }
}
