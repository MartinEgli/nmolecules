using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Describes an observed dependency from one Bricks element to another.
    /// </summary>
    /// <remarks>
    /// Use <see cref="BrickDependency"/> as the input model for deterministic policy evaluation with
    /// <see cref="BrickRuleEvaluator"/>. Dependencies are evidence: they do not decide whether a relation is allowed.
    /// Policies and resolved roles make that decision.
    ///
    /// Example: see
    /// <c>../nmolecules.brick-examples/samples/bricks/implementation-samples/function-coverage/ViolationAndRuntimeExamples.cs</c>.
    /// </remarks>
    public sealed class BrickDependency
    {
        /// <summary>
        /// Creates a dependency observation.
        /// </summary>
        /// <param name="source">Element that owns or creates the dependency.</param>
        /// <param name="target">Element that is referenced by the source.</param>
        /// <param name="kindId">Dependency kind, for example compile-time or runtime dependency.</param>
        /// <param name="scope">Architectural scope in which the dependency was observed.</param>
        /// <param name="layer">Layer that produced the evidence.</param>
        /// <param name="strength">Strength of the observed dependency.</param>
        /// <param name="evidenceLevel">Confidence level of the dependency evidence.</param>
        /// <param name="location">Optional source location for diagnostics.</param>
        /// <param name="detail">Optional human-readable detail for reports.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.</exception>
        public BrickDependency(
            BrickElement source,
            BrickElement target,
            BrickDependencyKindId kindId,
            BrickScope scope,
            BrickDependencyLayer layer,
            BrickDependencyStrength strength,
            BrickEvidenceLevel evidenceLevel,
            BrickSourceLocation? location = null,
            string detail = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            KindId = kindId;
            Scope = scope;
            Layer = layer;
            Strength = strength;
            EvidenceLevel = evidenceLevel;
            Location = location;
            Detail = detail;
        }

        /// <summary>
        /// Element that owns or creates the dependency.
        /// </summary>
        public BrickElement Source { get; }

        /// <summary>
        /// Element referenced by the source.
        /// </summary>
        public BrickElement Target { get; }

        /// <summary>
        /// Dependency kind used by policies, diagnostics and reports.
        /// </summary>
        public BrickDependencyKindId KindId { get; }

        /// <summary>
        /// Architectural scope in which the dependency was observed.
        /// </summary>
        public BrickScope Scope { get; }

        /// <summary>
        /// Evidence layer that produced the dependency.
        /// </summary>
        public BrickDependencyLayer Layer { get; }

        /// <summary>
        /// Strength of the observed dependency.
        /// </summary>
        public BrickDependencyStrength Strength { get; }

        /// <summary>
        /// Confidence level of the evidence.
        /// </summary>
        public BrickEvidenceLevel EvidenceLevel { get; }

        /// <summary>
        /// Optional source location that can be shown by analyzers or reports.
        /// </summary>
        public BrickSourceLocation? Location { get; }

        /// <summary>
        /// Optional human-readable detail for reports.
        /// </summary>
        public string Detail { get; }
    }
}
