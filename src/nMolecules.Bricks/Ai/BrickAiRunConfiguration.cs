using System;
using System.Collections.Generic;

namespace NMolecules.Bricks
{
    /// <summary>
    /// CI/MSBuild-facing configuration for the Bricks v3 advisory AI layer.
    /// </summary>
    public sealed class BrickAiRunConfiguration
    {
        public const string ModeProperty = "NMoleculesBricksAiMode";
        public const string CommentFormatProperty = "NMoleculesBricksAiCommentFormat";
        public const string AllowRuleProposalsProperty = "NMoleculesBricksAiAllowRuleProposals";
        public const string AllowAutoEnforcementProperty = "NMoleculesBricksAiAllowAutoEnforcement";
        public const string AllowSilentPolicyMutationProperty = "NMoleculesBricksAiAllowSilentPolicyMutation";
        public const string OutputDirectoryProperty = "NMoleculesBricksAiOutputDirectory";
        public const string ProposalQueuePathProperty = "NMoleculesBricksAiProposalQueuePath";

        public BrickAiRunConfiguration(
            BrickAiTrustBoundary trustBoundary,
            string outputDirectory,
            string proposalQueuePath)
        {
            TrustBoundary = trustBoundary ?? BrickAiTrustBoundary.Default;
            OutputDirectory = outputDirectory ?? string.Empty;
            ProposalQueuePath = proposalQueuePath ?? string.Empty;
        }

        public BrickAiTrustBoundary TrustBoundary { get; }
        public string OutputDirectory { get; }
        public string ProposalQueuePath { get; }
        public bool ShouldEmitComments => TrustBoundary.Mode != BrickAiMode.Off;
        public bool ShouldEmitMarkdown => ShouldEmitComments &&
            (TrustBoundary.CommentFormat == BrickAiCommentFormat.Markdown || TrustBoundary.CommentFormat == BrickAiCommentFormat.Both);
        public bool ShouldEmitJson => ShouldEmitComments &&
            (TrustBoundary.CommentFormat == BrickAiCommentFormat.Json || TrustBoundary.CommentFormat == BrickAiCommentFormat.Both);
        public bool CanCreateRuleProposals => TrustBoundary.Mode == BrickAiMode.SuggestRules && TrustBoundary.AllowRuleProposal;

        public static BrickAiRunConfiguration Default { get; } =
            new BrickAiRunConfiguration(BrickAiTrustBoundary.Default, string.Empty, string.Empty);

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
