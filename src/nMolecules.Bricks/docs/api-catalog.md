# nMolecules Bricks API Catalog

This catalog documents the public types in the `NMolecules.Bricks` namespace. It is generated from XML summaries in the source files so developers can navigate the Bricks model without opening every class first.
For value-level enum meaning, code snippets, and analyzer/runtime behavior, see `enum-behavior-guide.md`.
For package ownership, Roadmap placement, and future split candidates, see `package-boundaries.md`.

## Namespace

| Namespace | Purpose |
| --- | --- |
| `NMolecules.Bricks` | Composable architecture metadata, role resolution, dependency policy, rule evaluation, adoption, governance, benchmarking, export and reporting primitives for Bricks analyzers and runtime tooling. |

## Adoption

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickAdoptionDocument` | class | Defines the document shape used to exchange adoption document data between Bricks tools. | `Adoption/BrickAdoptionDocument.cs` |
| `BrickAdoptionDocumentIssue` | class | Describes a adoption document issue issue with enough context for diagnostics, governance, or remediation. | `Adoption/BrickAdoptionDocumentIssue.cs` |
| `BrickAdoptionDocumentValidator` | class | Validates adoption document validator input and returns structured issues that consumers can report or fix. | `Adoption/BrickAdoptionDocumentValidator.cs` |
| `BrickAdoptionJsonSerializer` | class | Serializes and deserializes adoption serializer documents using the stable Bricks JSON format. | `Adoption/BrickAdoptionJsonSerializer.cs` |
| `BrickBaselineEntry` | class | Represents baseline entry data used by adoption workflows, baselines, suppressions, and violation lifecycle state. | `Adoption/BrickBaselineEntry.cs` |
| `BrickElementSelector` | struct | Value object that represents element selector data for adoption workflows, baselines, suppressions, and violation lifecycle state. | `Adoption/BrickElementSelector.cs` |
| `BrickSuppression` | class | Represents suppression data used by adoption workflows, baselines, suppressions, and violation lifecycle state. | `Adoption/BrickSuppression.cs` |
| `BrickViolationStateProjector` | class | Represents violation state projector data used by adoption workflows, baselines, suppressions, and violation lifecycle state. | `Adoption/BrickViolationStateProjector.cs` |

## Ai

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickAiCommentFactory` | class | Creates AI-ready advisory comments from deterministic Bricks violations. | `Ai/BrickAiCommentFactory.cs` |
| `BrickAiCommentDocument` | class | Versioned document containing AI-ready comments for deterministic Bricks violations. | `Ai/BrickAiCommentDocument.cs` |
| `BrickAiCommentFormat` | enum | Selects the output shape for AI-assisted violation comments. | `Ai/BrickAiCommentFormat.cs` |
| `BrickAiCommentJsonSerializer` | class | Serializes AI-ready Bricks violation comments to the versioned JSON schema. | `Ai/BrickAiCommentJsonSerializer.cs` |
| `BrickAiCommentMarkdownRenderer` | class | Renders AI-ready violation comments as deterministic Markdown for IDE, PR or CI adapters. | `Ai/BrickAiCommentMarkdownRenderer.cs` |
| `BrickAiMode` | enum | Configures which AI-assisted Bricks capability is enabled for a run. | `Ai/BrickAiMode.cs` |
| `BrickAiRunConfiguration` | class | CI/MSBuild-facing configuration for the Bricks v3 advisory AI layer. | `Ai/BrickAiRunConfiguration.cs` |
| `BrickAiTrustBoundary` | class | Captures the safety boundary between deterministic Bricks enforcement and AI assistance. | `Ai/BrickAiTrustBoundary.cs` |
| `BrickAiViolationComment` | class | AI-ready explanation for one deterministic <see cref="BrickViolation"/>. | `Ai/BrickAiViolationComment.cs` |
| `BrickRemediationKind` | enum | Describes the structural remediation category an AI explanation may suggest for a deterministic Bricks violation. | `Ai/BrickRemediationKind.cs` |
| `BrickRemediationOption` | class | Describes one possible remediation for a deterministic Bricks violation. | `Ai/BrickRemediationOption.cs` |
| `BrickRemediationRisk` | enum | Classifies the expected architectural risk of applying a remediation option. | `Ai/BrickRemediationRisk.cs` |
| `BrickRuleProposal` | class | Advisory structural rule suggested by AI and awaiting deterministic review or promotion. | `Ai/BrickRuleProposal.cs` |
| `BrickRuleProposalEvidence` | class | Evidence package that must accompany an AI-generated structural rule proposal. | `Ai/BrickRuleProposalEvidence.cs` |
| `BrickRuleProposalQueue` | class | Versioned review queue for AI-generated rule proposals. | `Ai/BrickRuleProposalQueue.cs` |
| `BrickRuleProposalQueueJsonSerializer` | class | Serializes and deserializes AI rule proposal review queues. | `Ai/BrickRuleProposalQueueJsonSerializer.cs` |
| `BrickRuleProposalReview` | class | Human review decision for one AI-generated rule proposal. | `Ai/BrickRuleProposalReview.cs` |
| `BrickRuleProposalReviewResult` | class | Result of reviewing an AI-generated rule proposal. | `Ai/BrickRuleProposalReviewResult.cs` |
| `BrickRuleProposalReviewWorkflow` | class | Applies human review decisions to advisory AI rule proposals. | `Ai/BrickRuleProposalReviewWorkflow.cs` |

## Attributes

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `DependencyAttribute` | class | Attribute used to declare dependency metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/DependencyAttribute.cs` |
| `ExcludedMemberNameContainsAttribute` | class | Excludes dependency observations whose member names contain any configured token. | `Attributes/ExcludedMemberNameContainsAttribute.cs` |
| `ExcludedSourceNameContainsAttribute` | class | Excludes source types whose names contain any configured token. | `Attributes/ExcludedSourceNameContainsAttribute.cs` |
| `ExcludedTargetNameContainsAttribute` | class | Excludes target types whose names contain any configured token. | `Attributes/ExcludedTargetNameContainsAttribute.cs` |
| `ForbidMemberAttribute` | class | Declares that a type annotated with a custom marker attribute must not expose members carrying the configured marker attribute type. | `Attributes/ForbidMemberAttribute.cs` |
| `NamespaceRoleAttribute` | class | Assigns a Bricks role to all source types whose namespace matches a configured pattern. | `Attributes/NamespaceRoleAttribute.cs` |
| `PolicyAttribute` | class | Attribute used to declare policy metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/PolicyAttribute.cs` |
| `PolicyImportAttribute` | class | Attribute used to declare policy import metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/PolicyImportAttribute.cs` |
| `RequireAllMembersAttribute` | class | Declares that a type annotated with a custom marker attribute must expose at least one member for each configured marker attribute type. | `Attributes/RequireAllMembersAttribute.cs` |
| `RequiredSourceNameContainsAttribute` | class | Requires source types to contain at least one configured token in their names. | `Attributes/RequiredSourceNameContainsAttribute.cs` |
| `RequiredTargetNameContainsAttribute` | class | Requires target types to contain at least one configured token in their names. | `Attributes/RequiredTargetNameContainsAttribute.cs` |
| `RequireExactlyOneMemberAttribute` | class | Declares that a type annotated with a custom marker attribute must expose exactly one member carrying the configured marker attribute. | `Attributes/RequireExactlyOneMemberAttribute.cs` |
| `RequireExclusiveChoiceAttribute` | class | Declares that a type annotated with a custom marker attribute must expose members for exactly one of two configured marker attribute types. | `Attributes/RequireExclusiveChoiceAttribute.cs` |
| `RequireMemberCountAttribute` | class | Declares that a type annotated with a custom marker attribute must expose exactly the configured number of members carrying the configured marker attribute type. | `Attributes/RequireMemberCountAttribute.cs` |
| `RequireMemberRangeAttribute` | class | Declares that a type annotated with a custom marker attribute must expose a number of marked members within an inclusive range. | `Attributes/RequireMemberRangeAttribute.cs` |
| `RequireNamedMembersAttribute` | class | Declares that a type annotated with a custom marker attribute must expose members for each configured marker name. | `Attributes/RequireNamedMembersAttribute.cs` |
| `RequireUniqueNamedMemberAttribute` | class | Declares that a type annotated with a custom marker attribute must not expose more than one member for the same marker name. | `Attributes/RequireUniqueNamedMemberAttribute.cs` |
| `RoleAliasAttribute` | class | Attribute used to declare role alias metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/RoleAliasAttribute.cs` |
| `RoleAttribute` | class | Attribute used to declare role metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/RoleAttribute.cs` |
| `RoleCombinationAttribute` | class | Attribute used to declare role combination metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/RoleCombinationAttribute.cs` |
| `RuleAttribute` | class | Attribute used to declare rule metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/RuleAttribute.cs` |
| `RuleFilterAttribute` | class | Attribute used to declare rule filter metadata for attribute-based role, rule, dependency, and contract configuration. | `Attributes/RuleFilterAttribute.cs` |
| `RuleMode` | enum | Defines how a <see cref="RuleAttribute"/> should be interpreted by analyzers. | `Attributes/RuleMode.cs` |
| `TypeRoleAttribute` | class | Assigns a Bricks role to one exact type by runtime type reference or full type name. | `Attributes/TypeRoleAttribute.cs` |

## Benchmarking

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickBenchmarkBudget` | class | Defines the expected maximum elapsed time per operation for a benchmark case. | `Benchmarking/BrickBenchmarkBudget.cs` |
| `BrickBenchmarkCase` | class | Describes a deterministic benchmark case for a central Bricks capability. | `Benchmarking/BrickBenchmarkCase.cs` |
| `BrickBenchmarkComparison` | class | Compares one current benchmark result with an optional baseline result. | `Benchmarking/BrickBenchmarkComparison.cs` |
| `BrickBenchmarkComparisonReport` | class | Versioned report containing benchmark comparisons for one benchmark run. | `Benchmarking/BrickBenchmarkComparisonReport.cs` |
| `BrickBenchmarkComparisonReportJsonSerializer` | class | Serializes benchmark comparison reports to the versioned JSON schema. | `Benchmarking/BrickBenchmarkComparisonReportJsonSerializer.cs` |
| `BrickBenchmarkComparisonStatus` | enum | Describes how a current benchmark result compares with a previous baseline. | `Benchmarking/BrickBenchmarkComparisonStatus.cs` |
| `BrickBenchmarkComparisonSummary` | class | Summarizes benchmark comparison statuses for a report. | `Benchmarking/BrickBenchmarkComparisonSummary.cs` |
| `BrickBenchmarkComparisonThreshold` | class | Defines slowdown and improvement ratios used when comparing benchmark runs. | `Benchmarking/BrickBenchmarkComparisonThreshold.cs` |
| `BrickBenchmarkReport` | class | Versioned benchmark report for central Bricks benchmark results. | `Benchmarking/BrickBenchmarkReport.cs` |
| `BrickBenchmarkReportJsonSerializer` | class | Serializes benchmark reports to the versioned JSON schema. | `Benchmarking/BrickBenchmarkReportJsonSerializer.cs` |
| `BrickBenchmarkResult` | class | Captures the measured result of one benchmark case. | `Benchmarking/BrickBenchmarkResult.cs` |
| `BrickBenchmarkRunner` | class | Executes benchmark cases against measured operations. | `Benchmarking/BrickBenchmarkRunner.cs` |
| `BrickBenchmarkStatus` | enum | Describes whether a benchmark result satisfies its configured budget. | `Benchmarking/BrickBenchmarkStatus.cs` |
| `BrickBenchmarkSubject` | enum | Identifies the central Bricks capability measured by a benchmark case. | `Benchmarking/BrickBenchmarkSubject.cs` |
| `BrickBenchmarkSummary` | class | Summarizes budget status counts for a benchmark report. | `Benchmarking/BrickBenchmarkSummary.cs` |
| `BrickBuiltInBenchmarkCases` | class | Provides stable benchmark cases for central Bricks capabilities. | `Benchmarking/BrickBuiltInBenchmarkCases.cs` |
| `BrickStopwatchBenchmarkClock` | class | Benchmark clock backed by <see cref="Stopwatch"/>. | `Benchmarking/BrickStopwatchBenchmarkClock.cs` |
| `IBrickBenchmarkClock` | interface | Measures the elapsed time of a benchmark operation. | `Benchmarking/IBrickBenchmarkClock.cs` |

## Configuration

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickConfigurationEntry` | class | Represents configuration entry data used by configuration source precedence and resolved Bricks settings. | `Configuration/BrickConfigurationEntry.cs` |
| `BrickConfigurationPrecedence` | class | Represents configuration precedence data used by configuration source precedence and resolved Bricks settings. | `Configuration/BrickConfigurationPrecedence.cs` |
| `BrickConfigurationResolution` | class | Represents configuration resolution data used by configuration source precedence and resolved Bricks settings. | `Configuration/BrickConfigurationResolution.cs` |
| `BrickConfigurationResolvedEntry` | class | Represents configuration resolved entry data used by configuration source precedence and resolved Bricks settings. | `Configuration/BrickConfigurationResolvedEntry.cs` |
| `BrickConfigurationResolver` | class | Represents configuration resolver data used by configuration source precedence and resolved Bricks settings. | `Configuration/BrickConfigurationResolver.cs` |
| `BrickConfigurationSource` | class | Represents configuration source data used by configuration source precedence and resolved Bricks settings. | `Configuration/BrickConfigurationSource.cs` |
| `BrickConfigurationSourceKind` | enum | Describes the configuration channel that supplied Bricks settings. Use it to explain configuration precedence across generated defaults, packages, policy files, MSBuild, analyzer config, and source annotations. | `Configuration/BrickConfigurationSourceKind.cs` |

## Conformance

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickConformanceCapability` | class | Represents conformance capability data used by conformance capability checks and level assessments. | `Conformance/BrickConformanceCapability.cs` |
| `BrickConformanceCapabilityResult` | class | Represents the result of conformance capability result processing in the Bricks pipeline. | `Conformance/BrickConformanceCapabilityResult.cs` |
| `BrickConformanceCapabilityStatus` | enum | Describes the outcome status used by conformance capability checks and level assessments. | `Conformance/BrickConformanceCapabilityStatus.cs` |
| `BrickConformanceLevel` | enum | Represents the conformance level level used to communicate Bricks maturity or confidence. | `Conformance/BrickConformanceLevel.cs` |
| `BrickConformanceLevelAssessment` | class | Represents conformance level assessment data used by conformance capability checks and level assessments. | `Conformance/BrickConformanceLevelAssessment.cs` |
| `BrickConformanceLevelDefinition` | class | Represents conformance level definition data used by conformance capability checks and level assessments. | `Conformance/BrickConformanceLevelDefinition.cs` |
| `BrickConformanceLevelStatus` | enum | Describes the outcome status used by conformance capability checks and level assessments. | `Conformance/BrickConformanceLevelStatus.cs` |
| `BrickConformanceReport` | class | Captures a complete conformance report report for consumers, tools, and CI pipelines. | `Conformance/BrickConformanceReport.cs` |
| `BrickConformanceReportJsonSerializer` | class | Serializes and deserializes conformance report serializer documents using the stable Bricks JSON format. | `Conformance/BrickConformanceReportJsonSerializer.cs` |
| `BrickConformanceSummary` | class | Summarizes conformance summary results so callers can display the important outcome without reading every detail. | `Conformance/BrickConformanceSummary.cs` |

## Core

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickCombinationKind` | enum | Describes how two roles may be combined on one element. Use it to express whether roles can coexist, compete, or must never be combined. | `Core/BrickCombinationKind.cs` |
| `BrickDecision` | enum | Describes the policy decision a rule makes for matching dependencies. Use it to model allowed, forbidden, and required architecture relations. | `Core/BrickDecision.cs` |
| `BrickDependencyLayer` | enum | Describes the observation layer where a dependency was found. Use it to separate compile-time references from visibility, runtime, and configuration links. | `Core/BrickDependencyLayer.cs` |
| `BrickDependencyStrength` | enum | Describes how strongly a dependency is connected to the source element. Use it to communicate whether evidence is direct, indirect, or inferred. | `Core/BrickDependencyStrength.cs` |
| `BrickElementKind` | enum | Describes which kind of architecture element a Bricks fact refers to. Use it when selecting, reporting, or evaluating dependencies at assembly, namespace, type, member, or infrastructure-registration level. | `Core/BrickElementKind.cs` |
| `BrickElementOrigin` | enum | Describes where an architecture element originally came from. Use it to distinguish source-code facts from generated, external, or metadata-only facts. | `Core/BrickElementOrigin.cs` |
| `BrickElementSource` | enum | Describes which input channel contributed an element or assignment. Use it when explaining why a role, element, or policy fact exists. | `Core/BrickElementSource.cs` |
| `BrickEnforcementMode` | enum | Defines how strongly a policy participates in tooling. Use it to move from documentation through analysis to build-breaking enforcement. | `Core/BrickEnforcementMode.cs` |
| `BrickEvidenceLevel` | enum | Describes how reliable the evidence behind a dependency or violation is. Use it to rank findings and decide whether human review is required. | `Core/BrickEvidenceLevel.cs` |
| `BrickPermissionDefault` | enum | Defines the default behavior for dependencies not matched by any explicit rule. Use it to choose between permissive and allow-list style policies. | `Core/BrickPermissionDefault.cs` |
| `BrickRuleLifecycleState` | enum | Represents the review lifecycle of an AI-generated rule proposal. | `Core/BrickRuleLifecycleState.cs` |
| `BrickScope` | enum | Defines the architectural level at which a rule, dependency, or violation is evaluated. Use it to make clear whether a check applies to a type, member, namespace, assembly, or the whole model. | `Core/BrickScope.cs` |
| `BrickSeverity` | enum | Describes the severity of a finding. Use it to map Bricks findings to diagnostics, reports, CI gates, and review workflows. | `Core/BrickSeverity.cs` |
| `BrickViolationKind` | enum | Describes why a violation exists. Use it to separate dependency-rule problems from role-resolution, configuration, baseline, suppression, and member-contract issues. | `Core/BrickViolationKind.cs` |
| `BrickViolationState` | enum | Describes the lifecycle state of a violation. Use it to distinguish active violations from reviewed exceptions and expired exceptions. | `Core/BrickViolationState.cs` |

## Dependencies

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickBuiltInDependencyCoverageTargets` | class | Provides built-in Bricks defaults for dependency coverage targets and coverage reporting. | `Dependencies/BrickBuiltInDependencyCoverageTargets.cs` |
| `BrickDependencyCoverageReport` | class | Captures a complete dependency coverage report report for consumers, tools, and CI pipelines. | `Dependencies/BrickDependencyCoverageReport.cs` |
| `BrickDependencyCoverageReportJsonSerializer` | class | Serializes and deserializes dependency coverage report serializer documents using the stable Bricks JSON format. | `Dependencies/BrickDependencyCoverageReportJsonSerializer.cs` |
| `BrickDependencyCoverageResult` | class | Represents the result of dependency coverage result processing in the Bricks pipeline. | `Dependencies/BrickDependencyCoverageResult.cs` |
| `BrickDependencyCoverageStatus` | enum | Describes the outcome status used by dependency coverage targets and coverage reporting. | `Dependencies/BrickDependencyCoverageStatus.cs` |
| `BrickDependencyCoverageSummary` | class | Summarizes dependency coverage summary results so callers can display the important outcome without reading every detail. | `Dependencies/BrickDependencyCoverageSummary.cs` |
| `BrickDependencyCoverageTarget` | class | Represents dependency coverage target data used by dependency coverage targets and coverage reporting. | `Dependencies/BrickDependencyCoverageTarget.cs` |

## Export

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickDependencyGraphDocument` | class | Defines the document shape used to exchange dependency graph document data between Bricks tools. | `Export/BrickDependencyGraphDocument.cs` |
| `BrickDependencyGraphEdge` | class | Represents dependency graph edge data used by exportable role maps, dependency graphs, and resolution traces. | `Export/BrickDependencyGraphEdge.cs` |
| `BrickDependencyGraphNode` | class | Represents dependency graph node data used by exportable role maps, dependency graphs, and resolution traces. | `Export/BrickDependencyGraphNode.cs` |
| `BrickExportDocumentIssue` | class | Describes a export document issue issue with enough context for diagnostics, governance, or remediation. | `Export/BrickExportDocumentIssue.cs` |
| `BrickExportDocumentValidator` | class | Validates export document validator input and returns structured issues that consumers can report or fix. | `Export/BrickExportDocumentValidator.cs` |
| `BrickExportJsonSerializer` | class | Serializes and deserializes export serializer documents using the stable Bricks JSON format. | `Export/BrickExportJsonSerializer.cs` |
| `BrickResolutionTraceDocument` | class | Defines the document shape used to exchange resolution trace document data between Bricks tools. | `Export/BrickResolutionTraceDocument.cs` |
| `BrickResolutionTraceEntry` | class | Represents resolution trace entry data used by exportable role maps, dependency graphs, and resolution traces. | `Export/BrickResolutionTraceEntry.cs` |
| `BrickRoleMapDocument` | class | Defines the document shape used to exchange role map document data between Bricks tools. | `Export/BrickRoleMapDocument.cs` |
| `BrickRoleMapEntry` | class | Represents role map entry data used by exportable role maps, dependency graphs, and resolution traces. | `Export/BrickRoleMapEntry.cs` |

## Governance

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickBuiltInGovernanceAreas` | class | Provides built-in Bricks defaults for governance readiness areas, requirements, and summaries. | `Governance/BrickBuiltInGovernanceAreas.cs` |
| `BrickGovernanceArea` | enum | Enumerates the supported governance area values used by governance readiness areas, requirements, and summaries. | `Governance/BrickGovernanceArea.cs` |
| `BrickGovernanceAreaAssessment` | class | Represents governance area assessment data used by governance readiness areas, requirements, and summaries. | `Governance/BrickGovernanceAreaAssessment.cs` |
| `BrickGovernanceAreaDefinition` | class | Represents governance area definition data used by governance readiness areas, requirements, and summaries. | `Governance/BrickGovernanceAreaDefinition.cs` |
| `BrickGovernanceAreaStatus` | enum | Describes the outcome status used by governance readiness areas, requirements, and summaries. | `Governance/BrickGovernanceAreaStatus.cs` |
| `BrickGovernanceReport` | class | Captures a complete governance report report for consumers, tools, and CI pipelines. | `Governance/BrickGovernanceReport.cs` |
| `BrickGovernanceReportJsonSerializer` | class | Serializes and deserializes governance report serializer documents using the stable Bricks JSON format. | `Governance/BrickGovernanceReportJsonSerializer.cs` |
| `BrickGovernanceRequirement` | class | Represents governance requirement data used by governance readiness areas, requirements, and summaries. | `Governance/BrickGovernanceRequirement.cs` |
| `BrickGovernanceRequirementResult` | class | Represents the result of governance requirement result processing in the Bricks pipeline. | `Governance/BrickGovernanceRequirementResult.cs` |
| `BrickGovernanceRequirementStatus` | enum | Describes the outcome status used by governance readiness areas, requirements, and summaries. | `Governance/BrickGovernanceRequirementStatus.cs` |
| `BrickGovernanceSummary` | class | Summarizes governance summary results so callers can display the important outcome without reading every detail. | `Governance/BrickGovernanceSummary.cs` |

## Identity

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickDependencyKindId` | struct | Strongly typed identifier for dependency kind id values used as stable keys across Bricks APIs. | `Identity/BrickDependencyKindId.cs` |
| `BrickDependencyKinds` | class | Built-in dependency kind identifiers used by Bricks policies, reports and attribute-based dependency evidence. | `Identity/BrickDependencyKinds.cs` |
| `BrickDimensionId` | struct | Strongly typed identifier for dimension id values used as stable keys across Bricks APIs. | `Identity/BrickDimensionId.cs` |
| `BrickElementId` | struct | Strongly typed identifier for element id values used as stable keys across Bricks APIs. | `Identity/BrickElementId.cs` |
| `BrickPolicyId` | struct | Strongly typed identifier for policy id values used as stable keys across Bricks APIs. | `Identity/BrickPolicyId.cs` |
| `RoleId` | struct | Represents a typed architectural role identifier for bricks-based catalogs, comparisons, and helper APIs outside direct attribute argument lists. Strongly typed identifier for role id values used as stable keys across Bricks APIs. | `Identity/RoleId.cs` |
| `RuleId` | struct | Represents a typed architectural rule identifier for bricks-based catalogs, diagnostics, and helper APIs outside direct attribute argument lists. Strongly typed identifier for rule id values used as stable keys across Bricks APIs. | `Identity/RuleId.cs` |

## IO

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickJsonFile` | class | Represents json file data used by Bricks file IO helpers. | `IO/BrickJsonFile.cs` |

## Members

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickMemberCardinalityEvaluator` | class | Evaluates member cardinality evaluator rules against Bricks model data and produces deterministic assessment results. | `Members/BrickMemberCardinalityEvaluator.cs` |

## Model

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickBuiltInConformanceLevels` | class | Provides built-in Bricks defaults for core elements, dependencies, violations, and source locations. | `Model/BrickBuiltInConformanceLevels.cs` |
| `BrickDependency` | class | Represents dependency data used by core elements, dependencies, violations, and source locations. | `Model/BrickDependency.cs` |
| `BrickElement` | class | Represents element data used by core elements, dependencies, violations, and source locations. | `Model/BrickElement.cs` |
| `BrickSourceLocation` | struct | Value object that represents source location data for core elements, dependencies, violations, and source locations. | `Model/BrickSourceLocation.cs` |
| `BrickViolation` | class | Represents violation data used by core elements, dependencies, violations, and source locations. | `Model/BrickViolation.cs` |

## Policies

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickAlias` | class | Represents alias data used by policy documents, aliases, imports, composition, and policy-driven role assignment. | `Policies/BrickAlias.cs` |
| `BrickPolicy` | class | Defines the policy data used to govern policy documents, aliases, imports, composition, and policy-driven role assignment. | `Policies/BrickPolicy.cs` |
| `BrickPolicyComposer` | class | Represents policy composer data used by policy documents, aliases, imports, composition, and policy-driven role assignment. | `Policies/BrickPolicyComposer.cs` |
| `BrickPolicyCompositionResult` | class | Represents the result of policy composition result processing in the Bricks pipeline. | `Policies/BrickPolicyCompositionResult.cs` |
| `BrickPolicyCompositionStep` | class | Represents policy composition step data used by policy documents, aliases, imports, composition, and policy-driven role assignment. | `Policies/BrickPolicyCompositionStep.cs` |
| `BrickPolicyDocument` | class | Defines the document shape used to exchange policy document data between Bricks tools. | `Policies/BrickPolicyDocument.cs` |
| `BrickPolicyDocumentIssue` | class | Describes a policy document issue issue with enough context for diagnostics, governance, or remediation. | `Policies/BrickPolicyDocumentIssue.cs` |
| `BrickPolicyDocumentValidator` | class | Validates policy document validator input and returns structured issues that consumers can report or fix. | `Policies/BrickPolicyDocumentValidator.cs` |
| `BrickPolicyImport` | struct | Value object that represents policy import data for policy documents, aliases, imports, composition, and policy-driven role assignment. | `Policies/BrickPolicyImport.cs` |
| `BrickPolicyImportMode` | enum | Describes how imported policies are merged into a root policy. Use it when composing platform defaults, package policies, and project-specific policies. | `Policies/BrickPolicyImportMode.cs` |
| `BrickPolicyJsonSerializer` | class | Serializes and deserializes policy serializer documents using the stable Bricks JSON format. | `Policies/BrickPolicyJsonSerializer.cs` |
| `BrickPolicyRoleAssignmentProvider` | class | Represents policy role assignment provider data used by policy documents, aliases, imports, composition, and policy-driven role assignment. | `Policies/BrickPolicyRoleAssignmentProvider.cs` |

## Profiles

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickBuiltInProfiles` | class | Provides built-in Bricks defaults for built-in Bricks profiles. | `Profiles/BrickBuiltInProfiles.cs` |
| `BrickProfile` | class | Represents profile data used by built-in Bricks profiles. | `Profiles/BrickProfile.cs` |

## Reflection

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickReflectionAccess` | class | Represents reflection access data used by reflection-based architecture assessment. | `Reflection/BrickReflectionAccess.cs` |
| `BrickReflectionConfidence` | enum | Describes confidence in reflection-based observations. Use it when a dependency can be seen only through reflection or dynamic access patterns. | `Reflection/BrickReflectionConfidence.cs` |
| `BrickReflectionEvaluator` | class | Evaluates reflection evaluator rules against Bricks model data and produces deterministic assessment results. | `Reflection/BrickReflectionEvaluator.cs` |
| `BrickReflectionPolicy` | class | Defines the policy data used to govern reflection-based architecture assessment. | `Reflection/BrickReflectionPolicy.cs` |

## Reports

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickReportDocument` | class | Defines the document shape used to exchange report document data between Bricks tools. | `Reports/BrickReportDocument.cs` |
| `BrickReportJsonSerializer` | class | Serializes and deserializes report serializer documents using the stable Bricks JSON format. | `Reports/BrickReportJsonSerializer.cs` |
| `BrickReportSarifSerializer` | class | Represents report sarif serializer data used by human- and machine-readable Bricks reports. | `Reports/BrickReportSarifSerializer.cs` |
| `BrickReportSummary` | class | Summarizes report summary results so callers can display the important outcome without reading every detail. | `Reports/BrickReportSummary.cs` |

## Roadmap

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickBuiltInRoadmapStages` | class | Provides built-in Bricks defaults for roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickBuiltInRoadmapStages.cs` |
| `BrickRoadmapItem` | class | Represents roadmap item data used by roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickRoadmapItem.cs` |
| `BrickRoadmapItemResult` | class | Represents the result of roadmap item result processing in the Bricks pipeline. | `Roadmap/BrickRoadmapItemResult.cs` |
| `BrickRoadmapItemStatus` | enum | Describes the outcome status used by roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickRoadmapItemStatus.cs` |
| `BrickRoadmapReport` | class | Captures a complete roadmap report report for consumers, tools, and CI pipelines. | `Roadmap/BrickRoadmapReport.cs` |
| `BrickRoadmapReportJsonSerializer` | class | Serializes and deserializes roadmap report serializer documents using the stable Bricks JSON format. | `Roadmap/BrickRoadmapReportJsonSerializer.cs` |
| `BrickRoadmapStage` | enum | Enumerates the supported roadmap stage values used by roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickRoadmapStage.cs` |
| `BrickRoadmapStageAssessment` | class | Represents roadmap stage assessment data used by roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickRoadmapStageAssessment.cs` |
| `BrickRoadmapStageDefinition` | class | Represents roadmap stage definition data used by roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickRoadmapStageDefinition.cs` |
| `BrickRoadmapStageStatus` | enum | Describes the outcome status used by roadmap stages, readiness checks, and implementation progress. | `Roadmap/BrickRoadmapStageStatus.cs` |
| `BrickRoadmapSummary` | class | Summarizes roadmap summary results so callers can display the important outcome without reading every detail. | `Roadmap/BrickRoadmapSummary.cs` |

## Roles

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickAssignmentAuthority` | enum | Describes the authority of a role assignment. Use it as one part of precedence when resolving aliases, external policy, generated facts, and direct annotations. | `Roles/BrickAssignmentAuthority.cs` |
| `BrickAssignmentBehavior` | enum | Describes whether an assignment contributes a role or intentionally hides one. Use it for role overrides, suppressions, and migration scenarios. | `Roles/BrickAssignmentBehavior.cs` |
| `BrickAssignmentMode` | enum | Describes how a role assignment was created. Use it when auditing role resolution or explaining why a type received a role. | `Roles/BrickAssignmentMode.cs` |
| `BrickAssignmentPrecedence` | struct | Value object that represents assignment precedence data for role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickAssignmentPrecedence.cs` |
| `BrickAssignmentSource` | enum | Describes where a role assignment was sourced from. Use it for provenance in reports, resolution traces, and conflict explanations. | `Roles/BrickAssignmentSource.cs` |
| `BrickAssignmentSpecificity` | enum | Describes how specific a role assignment is. Use it as one part of precedence when multiple role assignments compete. | `Roles/BrickAssignmentSpecificity.cs` |
| `BrickAttributeRoleBridge` | class | Represents attribute role bridge data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickAttributeRoleBridge.cs` |
| `BrickAttributeRoleMapping` | class | Represents attribute role mapping data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickAttributeRoleMapping.cs` |
| `BrickBuiltInAttributeRoleBridges` | class | Provides built-in Bricks defaults for role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickBuiltInAttributeRoleBridges.cs` |
| `BrickBuiltInRolePacks` | class | Provides built-in Bricks defaults for role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickBuiltInRolePacks.cs` |
| `BrickModelContext` | class | Represents model context data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickModelContext.cs` |
| `BrickResolutionTrace` | class | Represents resolution trace data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickResolutionTrace.cs` |
| `BrickResolvedRoles` | class | Provides reusable role identifiers or role definitions for role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickResolvedRoles.cs` |
| `BrickRole` | class | Represents role data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRole.cs` |
| `BrickRoleAssignment` | class | Represents role assignment data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleAssignment.cs` |
| `BrickRoleAssignmentCollector` | class | Represents role assignment collector data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleAssignmentCollector.cs` |
| `BrickRoleCombinationRule` | class | Defines a rule used to evaluate role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleCombinationRule.cs` |
| `BrickRoleConflict` | class | Represents role conflict data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleConflict.cs` |
| `BrickRoleDimension` | class | Represents role dimension data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleDimension.cs` |
| `BrickRolePack` | class | Represents role pack data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRolePack.cs` |
| `BrickRoleResolver` | class | Represents role resolver data used by role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleResolver.cs` |
| `BrickRoleSelector` | struct | Value object that represents role selector data for role dimensions, assignments, resolution, conflicts, and role packs. | `Roles/BrickRoleSelector.cs` |
| `IBrickRoleAssignmentProvider` | interface | Defines the contract for i brick role assignment provider integration points in the Bricks pipeline. | `Roles/IBrickRoleAssignmentProvider.cs` |

## Rules

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickDiagnosticIdGovernance` | class | Represents diagnostic id governance data used by rule evaluation, diagnostics, filtering, and diagnostic id governance. | `Rules/BrickDiagnosticIdGovernance.cs` |
| `BrickDiagnosticIdRange` | class | Represents diagnostic id range data used by rule evaluation, diagnostics, filtering, and diagnostic id governance. | `Rules/BrickDiagnosticIdRange.cs` |
| `BrickRule` | struct | Defines a rule used to evaluate rule evaluation, diagnostics, filtering, and diagnostic id governance. | `Rules/BrickRule.cs` |
| `BrickRuleEvaluator` | class | Evaluates deterministic Bricks policy rules against observed dependencies and resolved roles. | `Rules/BrickRuleEvaluator.cs` |
| `ExcludedMemberNameContainsRuleFilter` | class | Excludes dependency observations whose member names contain any configured token. | `Rules/ExcludedMemberNameContainsRuleFilter.cs` |
| `ExcludedSourceNameContainsRuleFilter` | class | Excludes source types whose names contain any configured token. | `Rules/ExcludedSourceNameContainsRuleFilter.cs` |
| `ExcludedTargetNameContainsRuleFilter` | class | Excludes target types whose names contain any configured token. | `Rules/ExcludedTargetNameContainsRuleFilter.cs` |
| `RequiredSourceNameContainsRuleFilter` | class | Requires source types to contain at least one configured token in their names. | `Rules/RequiredSourceNameContainsRuleFilter.cs` |
| `RequiredTargetNameContainsRuleFilter` | class | Requires target types to contain at least one configured token in their names. | `Rules/RequiredTargetNameContainsRuleFilter.cs` |
| `RuleFilter` | class | Represents a single optional rule filter for <see cref="RuleAttribute"/>. | `Rules/RuleFilter.cs` |
| `RuleMessage` | struct | Represents a typed rule message template for <see cref="RuleAttribute"/>. | `Rules/RuleMessage.cs` |
| `RuleMessageBuilder` | class | Fluent builder for assembling <see cref="RuleMessage"/> templates. | `Rules/RuleMessageBuilder.cs` |

## Runtime

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickDependencyRegistration` | class | Represents dependency registration data used by runtime dependency registration, activation, and wiring checks. | `Runtime/BrickDependencyRegistration.cs` |
| `BrickRuntimeActivation` | class | Represents runtime activation data used by runtime dependency registration, activation, and wiring checks. | `Runtime/BrickRuntimeActivation.cs` |
| `BrickRuntimeActivationEvaluator` | class | Evaluates runtime activation evaluator rules against Bricks model data and produces deterministic assessment results. | `Runtime/BrickRuntimeActivationEvaluator.cs` |
| `BrickRuntimeActivationPolicy` | class | Defines the policy data used to govern runtime dependency registration, activation, and wiring checks. | `Runtime/BrickRuntimeActivationPolicy.cs` |
| `BrickRuntimeWiringEvaluator` | class | Evaluates runtime wiring evaluator rules against Bricks model data and produces deterministic assessment results. | `Runtime/BrickRuntimeWiringEvaluator.cs` |
| `BrickRuntimeWiringPolicy` | class | Defines the policy data used to govern runtime dependency registration, activation, and wiring checks. | `Runtime/BrickRuntimeWiringPolicy.cs` |

## Visibility

| Type | Kind | Description | Source |
| --- | --- | --- | --- |
| `BrickFriendAssemblyGrant` | class | Represents friend assembly grant data used by assembly visibility and friend-assembly policy checks. | `Visibility/BrickFriendAssemblyGrant.cs` |
| `BrickVisibilityEvaluator` | class | Evaluates visibility evaluator rules against Bricks model data and produces deterministic assessment results. | `Visibility/BrickVisibilityEvaluator.cs` |
| `BrickVisibilityPolicy` | class | Defines the policy data used to govern assembly visibility and friend-assembly policy checks. | `Visibility/BrickVisibilityPolicy.cs` |
