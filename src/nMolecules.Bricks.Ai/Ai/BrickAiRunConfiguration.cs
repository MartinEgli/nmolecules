using System;
using System.Collections.Generic;

namespace NMolecules.Bricks
{
    /// <summary>
    /// CI/MSBuild-facing configuration for the Bricks v3 advisory AI layer.
    /// </summary>
    public sealed class BrickAiRunConfiguration
    {
        /// <summary>
        /// Gets the MSBuild or configuration property name for Mode Property.
        /// </summary>
        public const string ModeProperty = "NMoleculesBricksAiMode";
        /// <summary>
        /// Gets the MSBuild or configuration property name for Comment Format Property.
        /// </summary>
        public const string CommentFormatProperty = "NMoleculesBricksAiCommentFormat";
        /// <summary>
        /// Gets the MSBuild or configuration property name for Allow Rule Proposals Property.
        /// </summary>
        public const string AllowRuleProposalsProperty = "NMoleculesBricksAiAllowRuleProposals";
        /// <summary>
        /// Gets the MSBuild or configuration property name for Allow Auto Enforcement Property.
        /// </summary>
        public const string AllowAutoEnforcementProperty = "NMoleculesBricksAiAllowAutoEnforcement";
        /// <summary>
        /// Gets the MSBuild or configuration property name for Allow Silent Policy Mutation Property.
        /// </summary>
        public const string AllowSilentPolicyMutationProperty = "NMoleculesBricksAiAllowSilentPolicyMutation";
        /// <summary>
        /// Gets the MSBuild or configuration property name for Output Directory Property.
        /// </summary>
        public const string OutputDirectoryProperty = "NMoleculesBricksAiOutputDirectory";
        /// <summary>
        /// Gets the MSBuild or configuration property name for Proposal Queue Path Property.
        /// </summary>
        public const string ProposalQueuePathProperty = "NMoleculesBricksAiProposalQueuePath";

        /// <summary>
        /// Initializes a new instance for Bricks developer tooling and automation workflows.
        /// </summary>
        public BrickAiRunConfiguration(
            BrickAiTrustBoundary trustBoundary,
            string outputDirectory,
            string proposalQueuePath)
        {
            TrustBoundary = trustBoundary ?? BrickAiTrustBoundary.Default;
            OutputDirectory = outputDirectory ?? string.Empty;
            ProposalQueuePath = proposalQueuePath ?? string.Empty;
        }

        /// <summary>
        /// Gets the Trust Boundary value used by Bricks developer tooling.
        /// </summary>
        public BrickAiTrustBoundary TrustBoundary { get; }
        /// <summary>
        /// Gets the Output Directory value used by Bricks developer tooling.
        /// </summary>
        public string OutputDirectory { get; }
        /// <summary>
        /// Gets the Proposal Queue Path value used by Bricks developer tooling.
        /// </summary>
        public string ProposalQueuePath { get; }
        /// <summary>
        /// Gets a value indicating whether Should Emit Comments applies.
        /// </summary>
        public bool ShouldEmitComments => TrustBoundary.Mode != BrickAiMode.Off;
        /// <summary>
        /// Gets a value indicating whether Should Emit Markdown applies.
        /// </summary>
        public bool ShouldEmitMarkdown => ShouldEmitComments &&
            (TrustBoundary.CommentFormat == BrickAiCommentFormat.Markdown || TrustBoundary.CommentFormat == BrickAiCommentFormat.Both);
        /// <summary>
        /// Gets a value indicating whether Should Emit Json applies.
        /// </summary>
        public bool ShouldEmitJson => ShouldEmitComments &&
            (TrustBoundary.CommentFormat == BrickAiCommentFormat.Json || TrustBoundary.CommentFormat == BrickAiCommentFormat.Both);
        /// <summary>
        /// Gets a value indicating whether Can Create Rule Proposals applies.
        /// </summary>
        public bool CanCreateRuleProposals => TrustBoundary.Mode == BrickAiMode.SuggestRules && TrustBoundary.AllowRuleProposal;

        /// <summary>
        /// Gets the default Bricks configuration used when no explicit settings are provided.
        /// </summary>
        public static BrickAiRunConfiguration Default { get; } =
            new BrickAiRunConfiguration(BrickAiTrustBoundary.Default, string.Empty, string.Empty);

        /// <summary>
        /// Creates a Bricks configuration object from external key-value properties.
        /// </summary>
        public static BrickAiRunConfiguration FromProperties(IReadOnlyDictionary<string, string> properties)
        {
            var values = properties ?? new Dictionary<string, string>();
            var mode = ParseEnum(Get(values, ModeProperty), BrickAiMode.Off);
            var format = ParseEnum(Get(values, CommentFormatProperty), BrickAiCommentFormat.Markdown);
            var allowRuleProposals = ParseBool(Get(values, AllowRuleProposalsProperty));
            var allowAutoEnforcement = ParseBool(Get(values, AllowAutoEnforcementProperty));
            var allowSilentPolicyMutation = ParseBool(Get(values, AllowSilentPolicyMutationProperty));

            return new BrickAiRunConfiguration(
                new BrickAiTrustBoundary(
                    mode,
                    format,
                    allowRuleProposals,
                    allowAutoEnforcement,
                    allowSilentPolicyMutation),
                Get(values, OutputDirectoryProperty),
                Get(values, ProposalQueuePathProperty));
        }

        private static string Get(IReadOnlyDictionary<string, string> values, string key)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : string.Empty;
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum defaultValue)
            where TEnum : struct
        {
            TEnum parsed;
            return Enum.TryParse(value, ignoreCase: true, result: out parsed) ? parsed : defaultValue;
        }

        private static bool ParseBool(string value)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) && parsed;
        }
    }
}
