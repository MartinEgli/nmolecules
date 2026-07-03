using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NMolecules.Bricks.Analyzers
{
    /// <summary>
    /// Analyzes Bricks metadata attributes for empty identifiers, duplicate declarations, and conflicting role or rule configuration.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BrickMetadataAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Gets the diagnostics produced by this metadata analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(
                BrickAnalyzerDiagnostics.BrickConfiguration,
                BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                BrickAnalyzerDiagnostics.BrickDependencyConfiguration,
                BrickAnalyzerDiagnostics.BrickRuleFilterConfiguration,
                BrickAnalyzerDiagnostics.BrickMemberContractConfiguration);

        /// <summary>
        /// Initializes the analyzer and registers compilation and attribute checks.
        /// </summary>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeDuplicateMetadata);
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeDuplicateMetadata(CompilationAnalysisContext context)
        {
            ReportDuplicateRules(context);
            ReportDuplicateRoles(context);
            ReportInvalidRoleCombinations(context);
            ReportConflictingRoleCombinations(context);
            ReportPolicyReferenceConflicts(context);
            ReportConflictingRuleFilters(context);
            ReportConflictingMemberContracts(context);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol as IMethodSymbol;
            var attributeType = symbol?.ContainingType;
            if (attributeType == null)
            {
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.PolicyAttribute))
            {
                ReportIfEmpty(
                    context,
                    attribute,
                    "PolicyAttribute must declare a non-empty policy id",
                    0,
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    "Policy",
                    "id");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.DefaultPolicyAttribute))
            {
                ReportIfEmpty(
                    context,
                    attribute,
                    "DefaultPolicyAttribute must declare a non-empty policy id",
                    0,
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    "DefaultPolicy",
                    "policyId");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.PolicyImportAttribute))
            {
                ReportIfEmpty(
                    context,
                    attribute,
                    "PolicyImportAttribute must declare a non-empty imported policy id",
                    0,
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    "PolicyImport",
                    "id");
                ReportIfEmpty(
                    context,
                    attribute,
                    "PolicyImportAttribute must declare a non-empty owner policy id",
                    1,
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    "PolicyImport",
                    "policy");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.TypeRoleAttribute))
            {
                var typeName = BrickAnalyzerFacts.GetStringArgument(context, attribute, 0, "typeName");
                var type = BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "type");
                if (type == null && typeName != null && typeName.Trim().Length == 0)
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        "TypeRoleAttribute must declare a non-empty type name",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "TypeRole");
                }

                ReportIfEmpty(
                    context,
                    attribute,
                    "TypeRoleAttribute must declare a non-empty role",
                    1,
                    BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                    "TypeRole",
                    "role");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RoleAttribute))
            {
                var value = BrickAnalyzerFacts.GetStringArgument(context, attribute, 0, "name");
                if (string.IsNullOrWhiteSpace(value) && !HasNonEmptyRoleAlias(attributeType))
                {
                    var target = BrickAnalyzerFacts.FindAnnotatedTypeName(attribute) ?? "type";
                    ReportConfiguration(
                        context,
                        attribute,
                        $"RoleAttribute on '{target}' must declare a non-empty role name",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "Role",
                        target: target);
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RuleAttribute))
            {
                ReportIfEmpty(
                    context,
                    attribute,
                    "RuleAttribute must declare a non-empty id",
                    0,
                    BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                    "Rule",
                    "id");
                ReportIfEmpty(
                    context,
                    attribute,
                    "RuleAttribute must declare a non-empty source role",
                    1,
                    BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                    "Rule",
                    "sourceRole");
                ReportIfEmpty(
                    context,
                    attribute,
                    "RuleAttribute must declare a non-empty target role",
                    2,
                    BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                    "Rule",
                    "targetRole");
                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.DependencyAttribute))
            {
                ReportIfEmpty(
                    context,
                    attribute,
                    "DependencyAttribute must declare a non-empty id",
                    0,
                    BrickAnalyzerDiagnostics.BrickDependencyConfiguration,
                    "Dependency",
                    "id");
                ReportIfEmpty(
                    context,
                    attribute,
                    "DependencyAttribute must declare a non-empty source",
                    1,
                    BrickAnalyzerDiagnostics.BrickDependencyConfiguration,
                    "Dependency",
                    "source");
                ReportIfEmpty(
                    context,
                    attribute,
                    "DependencyAttribute must declare a non-empty target",
                    2,
                    BrickAnalyzerDiagnostics.BrickDependencyConfiguration,
                    "Dependency",
                    "target");
                ReportIfEmpty(
                    context,
                    attribute,
                    "DependencyAttribute must declare a non-empty kind",
                    3,
                    BrickAnalyzerDiagnostics.BrickDependencyConfiguration,
                    "Dependency",
                    "kind");
                return;
            }

            AnalyzeMemberContractConfiguration(context, attribute, attributeType);
        }

        private static void ReportDuplicateRules(CompilationAnalysisContext context)
        {
            var seenById = new Dictionary<string, AttributeData>(System.StringComparer.Ordinal);
            var seenBySignature = new Dictionary<string, AttributeData>(System.StringComparer.Ordinal);
            var seenModeByEndpoint = new Dictionary<string, int>(System.StringComparer.Ordinal);
            foreach (var attribute in GetRuleAttributes(context.Compilation, context.CancellationToken))
            {
                var id = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id");
                var sourceRole = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "SourceRole");
                var targetRole = BrickAnalyzerFacts.GetAttributeString(attribute, 2, "TargetRole");
                var mode = BrickAnalyzerFacts.GetAttributeEnum(attribute, 3, "Mode", 0);
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 5, "Policy");
                var hasDuplicateId = false;
                if (string.IsNullOrWhiteSpace(id))
                {
                    hasDuplicateId = true;
                }
                else if (!seenById.ContainsKey(id))
                {
                    seenById.Add(id, attribute);
                }
                else
                {
                    hasDuplicateId = true;
                    ReportConfiguration(
                        context,
                        attribute,
                        $"RuleAttribute id '{id}' is declared more than once",
                        BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                        "Rule",
                        ruleId: id,
                        policyId: policyId,
                        sourceRole: sourceRole,
                        targetRole: targetRole,
                        correlationMode: "PolicyId");
                }

                if (hasDuplicateId ||
                    string.IsNullOrWhiteSpace(sourceRole) ||
                    string.IsNullOrWhiteSpace(targetRole))
                {
                    continue;
                }

                var endpoint = policyId + "\u001f" + sourceRole + "\u001f" + targetRole;
                var signature = policyId + "\u001f" + sourceRole + "\u001f" + targetRole + "\u001f" + mode;
                if (!seenBySignature.ContainsKey(signature))
                {
                    seenBySignature.Add(signature, attribute);
                }
                else
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        $"RuleAttribute for '{sourceRole}' to '{targetRole}' with mode '{FormatRuleMode(mode)}' is declared more than once",
                        BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                        "Rule",
                        ruleId: id,
                        policyId: policyId,
                        sourceRole: sourceRole,
                        targetRole: targetRole,
                        correlationMode: "PolicyId");
                    continue;
                }

                if (!seenModeByEndpoint.TryGetValue(endpoint, out var previousMode))
                {
                    seenModeByEndpoint.Add(endpoint, mode);
                    continue;
                }

                if (previousMode == mode)
                {
                    continue;
                }

                ReportConfiguration(
                    context,
                    attribute,
                    $"RuleAttribute for '{sourceRole}' to '{targetRole}' is declared with conflicting modes '{FormatRuleMode(previousMode)}' and '{FormatRuleMode(mode)}'",
                    BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                    "Rule",
                    ruleId: id,
                    policyId: policyId,
                    sourceRole: sourceRole,
                    targetRole: targetRole,
                    correlationMode: "PolicyId");
            }
        }

        private static void ReportDuplicateRoles(CompilationAnalysisContext context)
        {
            foreach (var type in GetDeclaredTypes(context.Compilation, context.CancellationToken))
            {
                var seenByRole = new Dictionary<string, AttributeData>(System.StringComparer.Ordinal);
                foreach (var attribute in type.GetAttributes())
                {
                    var role = TryGetRoleName(attribute);
                    if (string.IsNullOrWhiteSpace(role))
                    {
                        continue;
                    }

                    if (!seenByRole.ContainsKey(role))
                    {
                        seenByRole.Add(role, attribute);
                        continue;
                    }

                    ReportConfiguration(
                        context,
                        attribute,
                        $"Role '{role}' is assigned more than once to '{type.Name}'",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "Role",
                        target: type.Name);
                }
            }
        }

        private static void ReportInvalidRoleCombinations(CompilationAnalysisContext context)
        {
            ReportInvalidRoleCombinations(context, context.Compilation.Assembly.GetAttributes());
            ReportInvalidRoleCombinations(context, context.Compilation.SourceModule.GetAttributes());

            foreach (var type in GetDeclaredTypes(context.Compilation, context.CancellationToken))
            {
                ReportInvalidRoleCombinations(context, type.GetAttributes());
            }
        }

        private static void ReportInvalidRoleCombinations(
            CompilationAnalysisContext context,
            IEnumerable<AttributeData> attributes)
        {
            var materialized = attributes.ToArray();
            var policyCount = materialized
                .Where(IsPolicyAttribute)
                .Select(attribute => BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id"))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(System.StringComparer.Ordinal)
                .Count();
            var defaultPolicyId = GetDefaultPolicyId(materialized);
            var combinations = materialized
                .Where(IsRoleCombinationAttribute)
                .ToArray();
            var combinationNames = new HashSet<string>(
                combinations
                    .Select(attribute => BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Name"))
                    .Where(name => !string.IsNullOrWhiteSpace(name)),
                System.StringComparer.Ordinal);

            foreach (var attribute in combinations)
            {
                var name = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Name");
                var leftRoles = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "LeftRoles");
                var rightRoles = BrickAnalyzerFacts.GetAttributeString(attribute, 2, "RightRoles");
                var kind = BrickAnalyzerFacts.GetAttributeEnum(attribute, 3, "Kind", 2);
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 5, "Policy");
                var effectivePolicyId = GetEffectivePolicyId(policyId, defaultPolicyId);

                if (string.IsNullOrWhiteSpace(name))
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        "RoleCombinationAttribute must declare a non-empty name",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "RoleCombination",
                        policyId: effectivePolicyId,
                        sourceRole: leftRoles,
                        targetRole: rightRoles);
                }

                if (string.IsNullOrWhiteSpace(leftRoles))
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        "RoleCombinationAttribute must declare non-empty left roles",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "RoleCombination",
                        policyId: effectivePolicyId,
                        targetRole: rightRoles);
                }

                if (string.IsNullOrWhiteSpace(rightRoles))
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        "RoleCombinationAttribute must declare non-empty right roles",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "RoleCombination",
                        policyId: effectivePolicyId,
                        sourceRole: leftRoles);
                }

                if (policyCount > 1 &&
                    string.IsNullOrWhiteSpace(policyId) &&
                    string.IsNullOrWhiteSpace(defaultPolicyId))
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        "RoleCombinationAttribute must declare policyId when multiple PolicyAttribute declarations share the same scope",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "RoleCombination",
                        sourceRole: leftRoles,
                        targetRole: rightRoles,
                        correlationMode: "PolicyId");
                }

                if (!string.IsNullOrWhiteSpace(leftRoles) &&
                    string.Equals(leftRoles, rightRoles, System.StringComparison.Ordinal) &&
                    (kind == 1 || kind == 2))
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        $"RoleCombinationAttribute '{name}' cannot declare {FormatCombinationKind(kind)} for the same left and right role selector '{leftRoles}'",
                        BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                        "RoleCombination",
                        policyId: effectivePolicyId,
                        sourceRole: leftRoles,
                        targetRole: rightRoles);
                }

                ReportIfRoleCombinationReference(
                    context,
                    attribute,
                    name,
                    effectivePolicyId,
                    "LeftRoles",
                    leftRoles,
                    rightRoles,
                    combinationNames);
                ReportIfRoleCombinationReference(
                    context,
                    attribute,
                    name,
                    effectivePolicyId,
                    "RightRoles",
                    rightRoles,
                    leftRoles,
                    combinationNames);
            }
        }

        private static void ReportIfRoleCombinationReference(
            CompilationAnalysisContext context,
            AttributeData attribute,
            string name,
            string policyId,
            string side,
            string referencedSelector,
            string oppositeSelector,
            ISet<string> combinationNames)
        {
            if (string.IsNullOrWhiteSpace(referencedSelector) || !combinationNames.Contains(referencedSelector))
            {
                return;
            }

            ReportConfiguration(
                context,
                attribute,
                $"RoleCombinationAttribute '{name}' must reference roles in {side}, not another RoleCombinationAttribute '{referencedSelector}'",
                BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                "RoleCombination",
                policyId: policyId,
                sourceRole: side == "LeftRoles" ? referencedSelector : oppositeSelector,
                targetRole: side == "LeftRoles" ? oppositeSelector : referencedSelector);
        }

        private static void ReportConflictingRoleCombinations(CompilationAnalysisContext context)
        {
            var seenKindByPair = new Dictionary<string, int>(System.StringComparer.Ordinal);
            foreach (var scopedAttribute in GetRoleCombinationAttributesWithDefaultPolicy(context.Compilation, context.CancellationToken))
            {
                var attribute = scopedAttribute.Attribute;
                var leftRoles = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "LeftRoles");
                var rightRoles = BrickAnalyzerFacts.GetAttributeString(attribute, 2, "RightRoles");
                var kind = BrickAnalyzerFacts.GetAttributeEnum(attribute, 3, "Kind", 2);
                var policyId = GetEffectivePolicyId(
                    BrickAnalyzerFacts.GetAttributeString(attribute, 5, "Policy"),
                    scopedAttribute.DefaultPolicyId);
                if (string.IsNullOrWhiteSpace(leftRoles) || string.IsNullOrWhiteSpace(rightRoles))
                {
                    continue;
                }

                var pair = policyId + "\u001F" + CreateUnorderedPairKey(leftRoles, rightRoles);
                if (!seenKindByPair.TryGetValue(pair, out var previousKind))
                {
                    seenKindByPair.Add(pair, kind);
                    continue;
                }

                if (previousKind == kind)
                {
                    continue;
                }

                ReportConfiguration(
                    context,
                    attribute,
                    $"RoleCombinationAttribute for '{leftRoles}' and '{rightRoles}' is declared with conflicting kinds '{FormatCombinationKind(previousKind)}' and '{FormatCombinationKind(kind)}'",
                    BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                    "RoleCombination",
                    policyId: policyId,
                    sourceRole: leftRoles,
                    targetRole: rightRoles);
            }
        }

        private static void ReportConflictingRuleFilters(CompilationAnalysisContext context)
        {
            var requiredByRuleAndDimension = new Dictionary<string, HashSet<string>>(System.StringComparer.Ordinal);
            var excludedByRuleAndDimension = new Dictionary<string, HashSet<string>>(System.StringComparer.Ordinal);

            foreach (var attribute in GetRuleFilterAttributes(context.Compilation, context.CancellationToken))
            {
                var ruleId = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Rule");
                var dimension = GetRuleFilterDimension(attribute);
                if (string.IsNullOrWhiteSpace(ruleId) || dimension == null)
                {
                    continue;
                }

                var tokens = GetStringArrayArgument(attribute, 1)
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Select(token => token.Trim())
                    .Distinct(System.StringComparer.Ordinal)
                    .ToArray();
                if (tokens.Length == 0)
                {
                    continue;
                }

                var key = ruleId + "\u001f" + dimension;
                var current = IsRequiredRuleFilter(attribute)
                    ? GetOrAddTokenSet(requiredByRuleAndDimension, key)
                    : GetOrAddTokenSet(excludedByRuleAndDimension, key);
                var opposite = IsRequiredRuleFilter(attribute)
                    ? GetOrAddTokenSet(excludedByRuleAndDimension, key)
                    : GetOrAddTokenSet(requiredByRuleAndDimension, key);

                foreach (var token in tokens)
                {
                    if (opposite.Contains(token))
                    {
                        ReportConfiguration(
                            context,
                            attribute,
                            $"RuleFilterAttribute for rule '{ruleId}' both requires and excludes {dimension} token '{token}'",
                            BrickAnalyzerDiagnostics.BrickRuleFilterConfiguration,
                            "RuleFilter",
                            ruleId: ruleId,
                            target: dimension);
                    }

                    current.Add(token);
                }
            }
        }

        private static void ReportConflictingMemberContracts(CompilationAnalysisContext context)
        {
            foreach (var type in GetDeclaredTypes(context.Compilation, context.CancellationToken))
            {
                var contracts = type.GetAttributes()
                    .SelectMany(SafeToMemberContractInfos)
                    .ToArray();

                if (contracts.Length < 2)
                {
                    continue;
                }

                for (var leftIndex = 0; leftIndex < contracts.Length; leftIndex++)
                {
                    for (var rightIndex = leftIndex + 1; rightIndex < contracts.Length; rightIndex++)
                    {
                        var reason = FindMemberContractConflict(contracts[leftIndex], contracts[rightIndex]);
                        if (reason == null)
                        {
                            continue;
                        }

                        ReportConfiguration(
                            context,
                            contracts[rightIndex].Attribute,
                            $"Brick member contract configuration on '{type.Name}' is conflicting: {reason}",
                            BrickAnalyzerDiagnostics.BrickMemberContractConfiguration,
                            "MemberContract",
                            target: type.Name,
                            contractKind: contracts[rightIndex].Kind);
                    }
                }

                ReportExclusiveChoiceAggregateConflicts(context, type, contracts);
            }
        }

        private static void ReportPolicyReferenceConflicts(CompilationAnalysisContext context)
        {
            ReportPolicyReferenceConflicts(context, context.Compilation.Assembly.GetAttributes());
            ReportPolicyReferenceConflicts(context, context.Compilation.SourceModule.GetAttributes());

            foreach (var type in GetDeclaredTypes(context.Compilation, context.CancellationToken))
            {
                ReportPolicyReferenceConflicts(context, type.GetAttributes());
            }
        }

        private static void ReportPolicyReferenceConflicts(
            CompilationAnalysisContext context,
            IEnumerable<AttributeData> attributes)
        {
            var materialized = attributes.ToArray();
            var policyIds = new HashSet<string>(materialized
                .Where(IsPolicyAttribute)
                .Select(attribute => BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id"))
                .Where(id => !string.IsNullOrWhiteSpace(id)),
                System.StringComparer.Ordinal);

            foreach (var attribute in materialized.Where(IsDefaultPolicyAttribute))
            {
                var defaultPolicyId = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Policy");
                ReportMissingPolicyReference(
                    context,
                    attribute,
                    policyIds,
                    defaultPolicyId,
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    "DefaultPolicy",
                    $"DefaultPolicyAttribute policyId '{defaultPolicyId}' does not match any PolicyAttribute in the same scope",
                    correlationMode: "DefaultPolicyId",
                    requireDeclaredPolicy: true);
            }

            foreach (var attribute in materialized.Where(IsPolicyImportAttribute))
            {
                var importedPolicyId = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id");
                var ownerPolicyId = BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Policy");
                if (!string.IsNullOrWhiteSpace(importedPolicyId) &&
                    string.Equals(importedPolicyId, ownerPolicyId, System.StringComparison.Ordinal))
                {
                    ReportConfiguration(
                        context,
                        attribute,
                        $"PolicyImportAttribute for owner policy '{ownerPolicyId}' must not import itself",
                        BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                        "PolicyImport",
                        policyId: ownerPolicyId,
                        source: importedPolicyId,
                        correlationMode: "OwnerPolicyId");
                }

                ReportMissingPolicyReference(
                    context,
                    attribute,
                    policyIds,
                    ownerPolicyId,
                    BrickAnalyzerDiagnostics.BrickPolicyConfiguration,
                    "PolicyImport",
                    $"PolicyImportAttribute owner policy '{ownerPolicyId}' does not match any PolicyAttribute in the same scope",
                    correlationMode: "OwnerPolicyId");
            }

            foreach (var attribute in materialized.Where(IsRuleAttribute))
            {
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 5, "Policy");
                ReportMissingPolicyReference(
                    context,
                    attribute,
                    policyIds,
                    policyId,
                    BrickAnalyzerDiagnostics.BrickRuleConfiguration,
                    "Rule",
                    $"RuleAttribute policyId '{policyId}' does not match any PolicyAttribute in the same scope",
                    ruleId: BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Id"),
                    sourceRole: BrickAnalyzerFacts.GetAttributeString(attribute, 1, "SourceRole"),
                    targetRole: BrickAnalyzerFacts.GetAttributeString(attribute, 2, "TargetRole"),
                    correlationMode: "PolicyId");
            }

            foreach (var attribute in materialized.Where(IsDependencyAttribute))
            {
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 8, "Policy");
                ReportMissingPolicyReference(
                    context,
                    attribute,
                    policyIds,
                    policyId,
                    BrickAnalyzerDiagnostics.BrickDependencyConfiguration,
                    "Dependency",
                    $"DependencyAttribute policyId '{policyId}' does not match any PolicyAttribute in the same scope",
                    source: BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Source"),
                    target: BrickAnalyzerFacts.GetAttributeString(attribute, 2, "Target"),
                    correlationMode: "PolicyId");
            }

            foreach (var attribute in materialized.Where(IsTypeRoleAttribute))
            {
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 2, "Policy");
                ReportMissingPolicyReference(
                    context,
                    attribute,
                    policyIds,
                    policyId,
                    BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                    "TypeRole",
                    $"TypeRoleAttribute policyId '{policyId}' does not match any PolicyAttribute in the same scope",
                    source: GetTypeRoleName(attribute),
                    target: BrickAnalyzerFacts.GetAttributeString(attribute, 1, "Role"),
                    correlationMode: "PolicyId");
            }

            foreach (var attribute in materialized.Where(IsRoleCombinationAttribute))
            {
                var policyId = BrickAnalyzerFacts.GetAttributeString(attribute, 5, "Policy");
                ReportMissingPolicyReference(
                    context,
                    attribute,
                    policyIds,
                    policyId,
                    BrickAnalyzerDiagnostics.BrickRoleConfiguration,
                    "RoleCombination",
                    $"RoleCombinationAttribute policyId '{policyId}' does not match any PolicyAttribute in the same scope",
                    sourceRole: BrickAnalyzerFacts.GetAttributeString(attribute, 1, "LeftRoles"),
                    targetRole: BrickAnalyzerFacts.GetAttributeString(attribute, 2, "RightRoles"),
                    correlationMode: "PolicyId");
            }
        }

        private static void ReportMissingPolicyReference(
            CompilationAnalysisContext context,
            AttributeData attribute,
            IReadOnlyCollection<string> declaredPolicyIds,
            string policyId,
            DiagnosticDescriptor descriptor,
            string configurationKind,
            string message,
            string ruleId = null,
            string sourceRole = null,
            string targetRole = null,
            string source = null,
            string target = null,
            string correlationMode = null,
            bool requireDeclaredPolicy = false)
        {
            if ((!requireDeclaredPolicy && declaredPolicyIds.Count == 0) ||
                string.IsNullOrWhiteSpace(policyId) ||
                declaredPolicyIds.Contains(policyId))
            {
                return;
            }

            ReportConfiguration(
                context,
                attribute,
                message,
                descriptor,
                configurationKind,
                ruleId: ruleId,
                policyId: policyId,
                sourceRole: sourceRole,
                targetRole: targetRole,
                source: source,
                target: target,
                correlationMode: correlationMode);
        }

        private static IEnumerable<ScopedAttributeData> GetRoleCombinationAttributesWithDefaultPolicy(
            Compilation compilation,
            System.Threading.CancellationToken cancellationToken)
        {
            var assemblyAttributes = compilation.Assembly.GetAttributes().ToArray();
            foreach (var attribute in GetRoleCombinationAttributesWithDefaultPolicy(assemblyAttributes))
            {
                yield return attribute;
            }

            var moduleAttributes = compilation.SourceModule.GetAttributes().ToArray();
            foreach (var attribute in GetRoleCombinationAttributesWithDefaultPolicy(moduleAttributes))
            {
                yield return attribute;
            }

            foreach (var type in GetDeclaredTypes(compilation, cancellationToken))
            {
                var typeAttributes = type.GetAttributes().ToArray();
                foreach (var attribute in GetRoleCombinationAttributesWithDefaultPolicy(typeAttributes))
                {
                    yield return attribute;
                }
            }
        }

        private static IEnumerable<ScopedAttributeData> GetRoleCombinationAttributesWithDefaultPolicy(
            AttributeData[] attributes)
        {
            var defaultPolicyId = GetDefaultPolicyId(attributes);
            foreach (var attribute in attributes.Where(IsRoleCombinationAttribute))
            {
                yield return new ScopedAttributeData(attribute, defaultPolicyId);
            }
        }

        private static IEnumerable<AttributeData> GetRuleAttributes(
            Compilation compilation,
            System.Threading.CancellationToken cancellationToken)
        {
            foreach (var attribute in compilation.Assembly.GetAttributes().Where(IsRuleAttribute))
            {
                yield return attribute;
            }

            foreach (var attribute in compilation.SourceModule.GetAttributes().Where(IsRuleAttribute))
            {
                yield return attribute;
            }

            foreach (var type in GetDeclaredTypes(compilation, cancellationToken))
            {
                foreach (var attribute in type.GetAttributes().Where(IsRuleAttribute))
                {
                    yield return attribute;
                }
            }
        }

        private static IEnumerable<AttributeData> GetRoleCombinationAttributes(
            Compilation compilation,
            System.Threading.CancellationToken cancellationToken)
        {
            foreach (var attribute in compilation.Assembly.GetAttributes().Where(IsRoleCombinationAttribute))
            {
                yield return attribute;
            }

            foreach (var attribute in compilation.SourceModule.GetAttributes().Where(IsRoleCombinationAttribute))
            {
                yield return attribute;
            }

            foreach (var type in GetDeclaredTypes(compilation, cancellationToken))
            {
                foreach (var attribute in type.GetAttributes().Where(IsRoleCombinationAttribute))
                {
                    yield return attribute;
                }
            }
        }

        private static IEnumerable<AttributeData> GetRuleFilterAttributes(
            Compilation compilation,
            System.Threading.CancellationToken cancellationToken)
        {
            foreach (var attribute in compilation.Assembly.GetAttributes().Where(IsRuleFilterAttribute))
            {
                yield return attribute;
            }

            foreach (var attribute in compilation.SourceModule.GetAttributes().Where(IsRuleFilterAttribute))
            {
                yield return attribute;
            }

            foreach (var type in GetDeclaredTypes(compilation, cancellationToken))
            {
                foreach (var attribute in type.GetAttributes().Where(IsRuleFilterAttribute))
                {
                    yield return attribute;
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> GetDeclaredTypes(
            Compilation compilation,
            System.Threading.CancellationToken cancellationToken)
        {
            var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot(cancellationToken);
                foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    var symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken) as INamedTypeSymbol;
                    if (symbol != null && seen.Add(symbol))
                    {
                        yield return symbol;
                    }
                }
            }
        }

        private static bool IsRuleAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RuleAttribute);
        }

        private static bool IsPolicyAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.PolicyAttribute);
        }

        private static bool IsDefaultPolicyAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.DefaultPolicyAttribute);
        }

        private static bool IsPolicyImportAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.PolicyImportAttribute);
        }

        private static bool IsDependencyAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.DependencyAttribute);
        }

        private static bool IsTypeRoleAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.TypeRoleAttribute);
        }

        private static bool IsRoleCombinationAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RoleCombinationAttribute);
        }

        private static bool IsRuleFilterAttribute(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return attributeType != null && BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RuleFilterAttribute);
        }

        private static string GetDefaultPolicyId(IEnumerable<AttributeData> attributes) =>
            attributes
                .Where(IsDefaultPolicyAttribute)
                .Select(attribute => BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Policy"))
                .FirstOrDefault(policyId => !string.IsNullOrWhiteSpace(policyId)) ?? string.Empty;

        private static string GetEffectivePolicyId(string policyId, string defaultPolicyId) =>
            string.IsNullOrWhiteSpace(policyId) ? defaultPolicyId ?? string.Empty : policyId;

        private static bool IsRequiredRuleFilter(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            return BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequiredSourceNameContainsAttribute) ||
                BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequiredTargetNameContainsAttribute);
        }

        private static string GetRuleFilterDimension(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequiredSourceNameContainsAttribute) ||
                BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.ExcludedSourceNameContainsAttribute))
            {
                return "source";
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequiredTargetNameContainsAttribute) ||
                BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.ExcludedTargetNameContainsAttribute))
            {
                return "target";
            }

            return null;
        }

        private static string GetTypeRoleName(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 0)
            {
                return string.Empty;
            }

            var typeName = attribute.ConstructorArguments[0].Value as string;
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                return typeName;
            }

            var type = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
            return type == null ? string.Empty : BrickAnalyzerFacts.ToMetadataName(type);
        }

        private static string TryGetRoleName(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            if (attributeType == null)
            {
                return null;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RoleAttribute))
            {
                var directRole = BrickAnalyzerFacts.GetAttributeString(attribute, 0, "Name");
                if (!string.IsNullOrWhiteSpace(directRole))
                {
                    return directRole;
                }
            }

            foreach (var marker in attributeType.GetAttributes())
            {
                var markerType = marker.AttributeClass;
                if (BrickAnalyzerFacts.IsOrDerivesFrom(markerType, BrickAnalyzerFacts.RoleAliasAttribute))
                {
                    return BrickAnalyzerFacts.GetAttributeString(marker, 0, "Role");
                }
            }

            return null;
        }

        private static string FormatRuleMode(int mode) =>
            mode == 1 ? "RequireDependency" : "ForbidDependency";

        private static string FormatCombinationKind(int kind)
        {
            switch (kind)
            {
                case 0:
                    return "Additive";
                case 1:
                    return "Exclusive";
                default:
                    return "Incompatible";
            }
        }

        private static string CreateUnorderedPairKey(string left, string right)
        {
            var normalizedLeft = left ?? string.Empty;
            var normalizedRight = right ?? string.Empty;
            return string.CompareOrdinal(normalizedLeft, normalizedRight) <= 0
                ? normalizedLeft + "\u001f" + normalizedRight
                : normalizedRight + "\u001f" + normalizedLeft;
        }

        private static HashSet<string> GetOrAddTokenSet(IDictionary<string, HashSet<string>> tokensByKey, string key)
        {
            if (!tokensByKey.TryGetValue(key, out var tokens))
            {
                tokens = new HashSet<string>(System.StringComparer.Ordinal);
                tokensByKey.Add(key, tokens);
            }

            return tokens;
        }

        private static IReadOnlyList<string> GetStringArrayArgument(AttributeData attribute, int ordinal)
        {
            if (attribute == null || ordinal >= attribute.ConstructorArguments.Length)
            {
                return new string[0];
            }

            var argument = attribute.ConstructorArguments[ordinal];
            if (argument.Kind == TypedConstantKind.Array)
            {
                return argument.Values
                    .Select(item => item.Value as string)
                    .Where(item => item != null)
                    .ToArray();
            }

            return argument.Value is string singleValue
                ? new[] { singleValue }
                : new string[0];
        }

        private static bool HasNonEmptyRoleAlias(INamedTypeSymbol attributeType)
        {
            foreach (var marker in attributeType.GetAttributes())
            {
                var markerType = marker.AttributeClass;
                if (BrickAnalyzerFacts.IsOrDerivesFrom(markerType, BrickAnalyzerFacts.RoleAliasAttribute) &&
                    !string.IsNullOrWhiteSpace(BrickAnalyzerFacts.GetAttributeString(marker, 0, "Role")))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<MemberContractInfo> ToMemberContractInfos(AttributeData attribute)
        {
            var attributeType = attribute.AttributeClass;
            if (attributeType == null)
            {
                return Enumerable.Empty<MemberContractInfo>();
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExactlyOneMemberAttribute))
            {
                return SingleContract(attribute, "RequireExactlyOneMember", GetTypeArgument(attribute, 0));
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberCountAttribute))
            {
                var contract = CreateContract(attribute, "RequireMemberCount", GetTypeArgument(attribute, 0));
                contract.Count = GetIntArgument(attribute, 1);
                return contract.MemberTypeKey == null ? Enumerable.Empty<MemberContractInfo>() : new[] { contract };
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberRangeAttribute))
            {
                var contract = CreateContract(attribute, "RequireMemberRange", GetTypeArgument(attribute, 0));
                contract.MinimumCount = GetIntArgument(attribute, 1);
                contract.MaximumCount = GetIntArgument(attribute, 2);
                return contract.MemberTypeKey == null ? Enumerable.Empty<MemberContractInfo>() : new[] { contract };
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireAllMembersAttribute))
            {
                return GetTypeArrayArgument(attribute)
                    .Select(type => CreateContract(attribute, "RequireAllMembers", type))
                    .Where(contract => contract.MemberTypeKey != null)
                    .ToArray();
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExclusiveChoiceAttribute))
            {
                var left = GetTypeArgument(attribute, 0);
                var right = GetTypeArgument(attribute, 1);
                if (left == null || right == null)
                {
                    return Enumerable.Empty<MemberContractInfo>();
                }

                return new[]
                {
                    new MemberContractInfo(attribute, "RequireExclusiveChoice")
                    {
                        LeftMemberTypeKey = ToTypeKey(left),
                        LeftMemberTypeName = left.Name,
                        RightMemberTypeKey = ToTypeKey(right),
                        RightMemberTypeName = right.Name
                    }
                };
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.ForbidMemberAttribute))
            {
                return SingleContract(attribute, "ForbidMember", GetTypeArgument(attribute, 0));
            }

            return Enumerable.Empty<MemberContractInfo>();
        }

        private static IEnumerable<MemberContractInfo> SafeToMemberContractInfos(AttributeData attribute)
        {
            try
            {
                return ToMemberContractInfos(attribute);
            }
            catch (System.Exception)
            {
                // Invalid AttributeData is reported by the syntax-level metadata checks.
                return Enumerable.Empty<MemberContractInfo>();
            }
        }

        private static IEnumerable<MemberContractInfo> SingleContract(AttributeData attribute, string kind, ITypeSymbol memberType)
        {
            var contract = CreateContract(attribute, kind, memberType);
            return contract.MemberTypeKey == null ? Enumerable.Empty<MemberContractInfo>() : new[] { contract };
        }

        private static MemberContractInfo CreateContract(AttributeData attribute, string kind, ITypeSymbol memberType)
        {
            var contract = new MemberContractInfo(attribute, kind);
            if (memberType != null)
            {
                contract.MemberTypeKey = ToTypeKey(memberType);
                contract.MemberTypeName = memberType.Name;
            }

            return contract;
        }

        private static string FindMemberContractConflict(MemberContractInfo left, MemberContractInfo right)
        {
            if (left.Kind == "RequireExclusiveChoice" || right.Kind == "RequireExclusiveChoice")
            {
                return FindExclusiveChoiceConflict(left, right);
            }

            if (left.MemberTypeKey == null ||
                right.MemberTypeKey == null ||
                !string.Equals(left.MemberTypeKey, right.MemberTypeKey, System.StringComparison.Ordinal))
            {
                return null;
            }

            var marker = left.MemberTypeName ?? right.MemberTypeName ?? "marker";

            if (HasKind(left, right, "RequireAllMembers", "ForbidMember"))
            {
                return $"'{marker}' is both required by RequireAllMembers and forbidden by ForbidMember";
            }

            if (HasKind(left, right, "RequireExactlyOneMember", "ForbidMember"))
            {
                return $"'{marker}' is both required exactly once and forbidden";
            }

            if (HasKind(left, right, "RequireMemberCount", "ForbidMember"))
            {
                var countContract = left.Kind == "RequireMemberCount" ? left : right;
                if (countContract.Count.HasValue && countContract.Count.Value > 0)
                {
                    return $"'{marker}' requires count {countContract.Count.Value} but is also forbidden";
                }
            }

            if (HasKind(left, right, "RequireMemberRange", "ForbidMember"))
            {
                var rangeContract = left.Kind == "RequireMemberRange" ? left : right;
                if (rangeContract.MinimumCount.HasValue && rangeContract.MinimumCount.Value > 0)
                {
                    return $"'{marker}' requires at least {rangeContract.MinimumCount.Value} member(s) but is also forbidden";
                }
            }

            if (HasKind(left, right, "RequireExactlyOneMember", "RequireMemberCount"))
            {
                var countContract = left.Kind == "RequireMemberCount" ? left : right;
                if (countContract.Count.HasValue && countContract.Count.Value != 1)
                {
                    return $"'{marker}' requires exactly one member and count {countContract.Count.Value}";
                }
            }

            if (HasKind(left, right, "RequireExactlyOneMember", "RequireMemberRange"))
            {
                var rangeContract = left.Kind == "RequireMemberRange" ? left : right;
                if (rangeContract.MinimumCount.HasValue &&
                    rangeContract.MaximumCount.HasValue &&
                    (rangeContract.MinimumCount.Value > 1 || rangeContract.MaximumCount.Value < 1))
                {
                    return $"'{marker}' requires exactly one member but range {rangeContract.MinimumCount.Value}..{rangeContract.MaximumCount.Value}";
                }
            }

            if (left.Kind == "RequireMemberCount" && right.Kind == "RequireMemberCount" &&
                left.Count.HasValue &&
                right.Count.HasValue &&
                left.Count.Value != right.Count.Value)
            {
                return $"'{marker}' has conflicting required counts {left.Count.Value} and {right.Count.Value}";
            }

            if (HasKind(left, right, "RequireAllMembers", "RequireMemberCount"))
            {
                var countContract = left.Kind == "RequireMemberCount" ? left : right;
                if (countContract.Count.HasValue && countContract.Count.Value == 0)
                {
                    return $"'{marker}' is required by RequireAllMembers but count is zero";
                }
            }

            if (HasKind(left, right, "RequireAllMembers", "RequireMemberRange"))
            {
                var rangeContract = left.Kind == "RequireMemberRange" ? left : right;
                if (rangeContract.MaximumCount.HasValue && rangeContract.MaximumCount.Value == 0)
                {
                    return $"'{marker}' is required by RequireAllMembers but range maximum is zero";
                }
            }

            return null;
        }

        private static string FindExclusiveChoiceConflict(MemberContractInfo left, MemberContractInfo right)
        {
            return null;
        }

        private static void ReportExclusiveChoiceAggregateConflicts(
            CompilationAnalysisContext context,
            INamedTypeSymbol type,
            IReadOnlyList<MemberContractInfo> contracts)
        {
            foreach (var exclusive in contracts.Where(contract => contract.Kind == "RequireExclusiveChoice"))
            {
                var requiresLeft = contracts.Any(contract =>
                    RequiresPositiveCount(contract) &&
                    string.Equals(contract.MemberTypeKey, exclusive.LeftMemberTypeKey, System.StringComparison.Ordinal));
                var requiresRight = contracts.Any(contract =>
                    RequiresPositiveCount(contract) &&
                    string.Equals(contract.MemberTypeKey, exclusive.RightMemberTypeKey, System.StringComparison.Ordinal));
                if (!requiresLeft || !requiresRight)
                {
                    continue;
                }

                ReportConfiguration(
                    context,
                    exclusive.Attribute,
                    $"Brick member contract configuration on '{type.Name}' is conflicting: exclusive choice '{exclusive.LeftMemberTypeName}' or '{exclusive.RightMemberTypeName}' cannot require both marker types",
                    BrickAnalyzerDiagnostics.BrickMemberContractConfiguration,
                    "MemberContract",
                    target: type.Name,
                    contractKind: exclusive.Kind);
            }
        }

        private static bool RequiresPositiveCount(MemberContractInfo contract)
        {
            if (contract.MemberTypeKey == null)
            {
                return false;
            }

            switch (contract.Kind)
            {
                case "RequireAllMembers":
                case "RequireExactlyOneMember":
                    return true;
                case "RequireMemberCount":
                    return contract.Count.HasValue && contract.Count.Value > 0;
                case "RequireMemberRange":
                    return contract.MinimumCount.HasValue && contract.MinimumCount.Value > 0;
                default:
                    return false;
            }
        }

        private static bool HasKind(MemberContractInfo left, MemberContractInfo right, string first, string second) =>
            (left.Kind == first && right.Kind == second) ||
            (left.Kind == second && right.Kind == first);

        private static ITypeSymbol GetTypeArgument(AttributeData attribute, int ordinal)
        {
            if (attribute == null || ordinal >= attribute.ConstructorArguments.Length)
            {
                return null;
            }

            return attribute.ConstructorArguments[ordinal].Value as ITypeSymbol;
        }

        private static IReadOnlyList<ITypeSymbol> GetTypeArrayArgument(AttributeData attribute)
        {
            if (attribute == null || attribute.ConstructorArguments.Length == 0)
            {
                return new ITypeSymbol[0];
            }

            var argument = attribute.ConstructorArguments[0];
            if (argument.Kind == TypedConstantKind.Error)
            {
                return new ITypeSymbol[0];
            }

            if (argument.Kind != TypedConstantKind.Array)
            {
                var singleType = argument.Value as ITypeSymbol;
                return singleType == null ? new ITypeSymbol[0] : new[] { singleType };
            }

            var result = new List<ITypeSymbol>();
            foreach (var item in argument.Values)
            {
                if (item.IsNull)
                {
                    continue;
                }

                var itemType = item.Value as ITypeSymbol;
                if (itemType != null)
                {
                    result.Add(itemType);
                }
            }

            return result.ToArray();
        }

        private static int? GetIntArgument(AttributeData attribute, int ordinal)
        {
            if (attribute == null || ordinal >= attribute.ConstructorArguments.Length)
            {
                return null;
            }

            return attribute.ConstructorArguments[ordinal].Value is int value ? (int?)value : null;
        }

        private static string ToTypeKey(ITypeSymbol type) =>
            type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty);

        private static void AnalyzeMemberContractConfiguration(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            INamedTypeSymbol attributeType)
        {
            var target = BrickAnalyzerFacts.FindAnnotatedTypeName(attribute) ?? "type";

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExactlyOneMemberAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireExactlyOneMember", target, "missing marker attribute type");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberCountAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberCount", target, "missing marker attribute type");
                }

                var count = BrickAnalyzerFacts.GetIntArgument(context, attribute, 1, "count");
                if (count.HasValue && count.Value < 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberCount", target, "count must not be negative");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireMemberRangeAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "missing marker attribute type");
                }

                var minimumCount = BrickAnalyzerFacts.GetIntArgument(context, attribute, 1, "minimumCount");
                var maximumCount = BrickAnalyzerFacts.GetIntArgument(context, attribute, 2, "maximumCount");
                if (minimumCount.HasValue && minimumCount.Value < 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "minimum count must not be negative");
                }

                if (maximumCount.HasValue && maximumCount.Value < 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "maximum count must not be negative");
                }

                if (minimumCount.HasValue && maximumCount.HasValue && minimumCount.Value > maximumCount.Value)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireMemberRange", target, "minimum count must not exceed maximum count");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireAllMembersAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArrayArgument(context, attribute).Count == 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireAllMembers", target, "at least one marker attribute type is required");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireExclusiveChoiceAttribute))
            {
                var left = BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "leftMemberAttributeType");
                var right = BrickAnalyzerFacts.GetTypeArgument(context, attribute, 1, "rightMemberAttributeType");
                if (left == null || right == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireExclusiveChoice", target, "both marker attribute types are required");
                    return;
                }

                if (SymbolEqualityComparer.Default.Equals(left, right))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireExclusiveChoice", target, "left and right marker types must be different");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.ForbidMemberAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "ForbidMember", target, "missing marker attribute type");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireNamedMembersAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "missing marker attribute type");
                }

                var requiredNames = GetStringArgumentsFromOrdinal(context, attribute, 1);
                if (requiredNames.Count == 0)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "at least one required marker name is required");
                }

                if (requiredNames.GroupBy(name => name, System.StringComparer.Ordinal).Any(group => group.Count() > 1))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "required marker names must be unique");
                }

                var nameArgument = BrickAnalyzerFacts.GetStringArgument(context, attribute, int.MaxValue, "NameArgument", "nameArgument");
                if (nameArgument != null && string.IsNullOrWhiteSpace(nameArgument))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireNamedMembers", target, "name argument must not be empty");
                }

                return;
            }

            if (BrickAnalyzerFacts.IsOrDerivesFrom(attributeType, BrickAnalyzerFacts.RequireUniqueNamedMemberAttribute))
            {
                if (BrickAnalyzerFacts.GetTypeArgument(context, attribute, 0, "memberAttributeType") == null)
                {
                    ReportInvalidMemberContract(context, attribute, "RequireUniqueNamedMember", target, "missing marker attribute type");
                }

                var nameArgument = BrickAnalyzerFacts.GetStringArgument(context, attribute, 1, "nameArgument");
                if (nameArgument != null && string.IsNullOrWhiteSpace(nameArgument))
                {
                    ReportInvalidMemberContract(context, attribute, "RequireUniqueNamedMember", target, "name argument must not be empty");
                }
            }
        }

        private static IReadOnlyList<string> GetStringArgumentsFromOrdinal(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            int ordinal)
        {
            var result = new List<string>();
            var arguments = attribute.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count == 0)
            {
                return result;
            }

            var positionalIndex = 0;
            foreach (var argument in arguments.Value)
            {
                if (argument.NameEquals != null || argument.NameColon != null)
                {
                    continue;
                }

                if (positionalIndex++ < ordinal)
                {
                    continue;
                }

                if (TryAddStringArrayConstants(context, argument.Expression, result))
                {
                    continue;
                }

                var constant = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
                if (constant.HasValue)
                {
                    result.Add(NormalizeMarkerName(constant.Value as string));
                }
            }

            return result;
        }

        private static bool TryAddStringArrayConstants(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax expression,
            ICollection<string> result)
        {
            var initializer = (expression as ArrayCreationExpressionSyntax)?.Initializer ??
                (expression as ImplicitArrayCreationExpressionSyntax)?.Initializer;
            if (initializer == null)
            {
                return false;
            }

            foreach (var item in initializer.Expressions)
            {
                var constant = context.SemanticModel.GetConstantValue(item, context.CancellationToken);
                if (constant.HasValue)
                {
                    result.Add(NormalizeMarkerName(constant.Value as string));
                }
            }

            return true;
        }

        private static string NormalizeMarkerName(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value;

        private static void ReportIfEmpty(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string message,
            int ordinal,
            params string[] names)
        {
            var value = BrickAnalyzerFacts.GetStringArgument(context, attribute, ordinal, names);
            if (value != null && value.Trim().Length == 0)
            {
                ReportConfiguration(context, attribute, message);
            }
        }

        private static void ReportIfEmpty(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string message,
            int ordinal,
            DiagnosticDescriptor descriptor,
            string configurationKind,
            params string[] names)
        {
            var value = BrickAnalyzerFacts.GetStringArgument(context, attribute, ordinal, names);
            if (value != null && value.Trim().Length == 0)
            {
                ReportConfiguration(context, attribute, message, descriptor, configurationKind);
            }
        }

        private static void ReportInvalidMemberContract(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string contractName,
            string target,
            string reason)
        {
            ReportConfiguration(
                context,
                attribute,
                $"Brick member contract '{contractName}' on '{target}' is invalid: {reason}",
                BrickAnalyzerDiagnostics.BrickMemberContractConfiguration,
                "MemberContract",
                target: target,
                contractKind: contractName);
        }

        private static void ReportConfiguration(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string message,
            DiagnosticDescriptor descriptor = null,
            string configurationKind = null,
            string ruleId = null,
            string policyId = null,
            string sourceRole = null,
            string targetRole = null,
            string source = null,
            string target = null,
            string contractKind = null,
            string correlationMode = null)
        {
            context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                descriptor ?? BrickAnalyzerDiagnostics.BrickConfiguration,
                attribute.GetLocation(),
                message,
                configurationKind: configurationKind,
                ruleId: ruleId,
                policyId: policyId,
                sourceRole: sourceRole,
                targetRole: targetRole,
                source: source,
                target: target,
                contractKind: contractKind,
                correlationMode: correlationMode));
        }

        private static void ReportConfiguration(
            CompilationAnalysisContext context,
            AttributeData attribute,
            string message,
            DiagnosticDescriptor descriptor = null,
            string configurationKind = null,
            string ruleId = null,
            string policyId = null,
            string sourceRole = null,
            string targetRole = null,
            string source = null,
            string target = null,
            string contractKind = null,
            string correlationMode = null)
        {
            var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);
            context.ReportDiagnostic(BrickDiagnosticProperties.Create(
                descriptor ?? BrickAnalyzerDiagnostics.BrickConfiguration,
                syntax?.GetLocation(),
                message,
                configurationKind: configurationKind,
                ruleId: ruleId,
                policyId: policyId,
                sourceRole: sourceRole,
                targetRole: targetRole,
                source: source,
                target: target,
                contractKind: contractKind,
                correlationMode: correlationMode));
        }

        private sealed class ScopedAttributeData
        {
            public ScopedAttributeData(AttributeData attribute, string defaultPolicyId)
            {
                Attribute = attribute;
                DefaultPolicyId = defaultPolicyId ?? string.Empty;
            }

            public AttributeData Attribute { get; }

            public string DefaultPolicyId { get; }
        }

        private sealed class MemberContractInfo
        {
            public MemberContractInfo(AttributeData attribute, string kind)
            {
                Attribute = attribute;
                Kind = kind;
            }

            public AttributeData Attribute { get; }

            public string Kind { get; }

            public string MemberTypeKey { get; set; }

            public string MemberTypeName { get; set; }

            public int? Count { get; set; }

            public int? MinimumCount { get; set; }

            public int? MaximumCount { get; set; }

            public string LeftMemberTypeKey { get; set; }

            public string LeftMemberTypeName { get; set; }

            public string RightMemberTypeKey { get; set; }

            public string RightMemberTypeName { get; set; }
        }
    }
}
