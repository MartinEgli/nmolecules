using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksPolicyJsonSerializerTest
    {
        [Fact]
        public void DeserializeReadsFullPolicyDocument()
        {
            var document = BrickPolicyJsonSerializer.Deserialize(FullPolicyJson);

            Assert.Equal(BrickPolicyDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Empty(BrickPolicyDocumentValidator.Validate(document));
            Assert.Equal(BrickPolicyId.From("Strict"), document.Policy.Id);
            Assert.Equal("Strict Policy", document.Policy.Name);
            Assert.Equal(BrickPermissionDefault.Deny, document.Policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Analyze, document.Policy.Enforcement);

            var import = document.Policy.Imports.Single();
            Assert.Equal(BrickPolicyId.From("Base"), import.ImportedPolicyId);
            Assert.Equal(BrickPolicyImportMode.Extend, import.Mode);

            var rule = document.Policy.Rules.Single();
            Assert.Equal(RuleId.From("XMoleculesBricks0001"), rule.RuleId);
            Assert.Equal("No infrastructure", rule.Name);
            Assert.Equal(RoleId.From("DDD.Entity"), rule.SourceRole);
            Assert.Equal(RoleId.From("Infrastructure"), rule.TargetRole);
            Assert.Equal(BrickDecision.Deny, rule.Decision);
            Assert.Equal(BrickScope.Type, rule.Scope);
            Assert.Equal(BrickSeverity.Error, rule.Severity);
            Assert.Equal(10, rule.Priority);
            Assert.Equal("Keep domain clean.", rule.Reason);

            var combinationRule = document.Policy.CombinationRules.Single();
            Assert.Equal("DDD+Business", combinationRule.Name);
            Assert.Equal(BrickCombinationKind.Additive, combinationRule.Kind);
            Assert.Equal("DDD roles can carry business partition roles.", combinationRule.Reason);

            var assignment = document.Policy.ExternalAssignments.Single();
            Assert.Equal(new BrickElementSelector(BrickElementKind.Namespace, "Billing.Domain.*", "Billing"), assignment.Selector);
            Assert.Equal(RoleId.From("Business.Billing"), assignment.RoleId);
            Assert.Equal(BrickAssignmentMode.ExternalConfiguration, assignment.Mode);
            Assert.Equal(BrickAssignmentSource.PolicyFile, assignment.Source);
            Assert.Equal(new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Namespace, BrickAssignmentAuthority.External), assignment.Precedence);
            Assert.Equal(BrickAssignmentBehavior.Apply, assignment.Behavior);
            Assert.Equal("Project policy.", assignment.Reason);

            var alias = document.Policy.Aliases.Single();
            Assert.Equal("Repository suffix", alias.Name);
            Assert.Equal(new BrickElementSelector(BrickElementKind.Type, "*Repository", null), alias.Selector);
            Assert.Equal(RoleId.From("DDD.Repository"), alias.CanonicalRoleId);
            Assert.Equal(new BrickAssignmentPrecedence(BrickAssignmentSpecificity.Element, BrickAssignmentAuthority.Alias), alias.Precedence);
            Assert.Equal(BrickAssignmentBehavior.Apply, alias.Behavior);
            Assert.Equal("Convention bridge.", alias.Reason);
        }

        [Fact]
        public void DeserializeNormalizesMissingCollectionsThroughPolicyModel()
        {
            var document = BrickPolicyJsonSerializer.Deserialize(
                @"{
                    ""schema"": ""NMolecules.Bricks.Policy/1.0"",
                    ""policy"": {
                        ""id"": ""Default"",
                        ""name"": ""Default"",
                        ""defaultDecision"": ""Allow"",
                        ""enforcement"": ""Document""
                    }
                }");

            Assert.Equal(BrickPolicyId.From("Default"), document.Policy.Id);
            Assert.Empty(document.Policy.Imports);
            Assert.Empty(document.Policy.Rules);
            Assert.Empty(document.Policy.CombinationRules);
            Assert.Empty(document.Policy.ExternalAssignments);
            Assert.Empty(document.Policy.Aliases);
            Assert.Equal(BrickPermissionDefault.Allow, document.Policy.DefaultDecision);
            Assert.Equal(BrickEnforcementMode.Document, document.Policy.Enforcement);
        }

        [Fact]
        public void DeserializeRequiresJsonObject()
        {
            Assert.Throws<ArgumentNullException>(() => BrickPolicyJsonSerializer.Deserialize(null));
            Assert.Throws<ArgumentException>(() => BrickPolicyJsonSerializer.Deserialize("null"));
        }

        private const string FullPolicyJson = @"{
            ""schema"": ""NMolecules.Bricks.Policy/1.0"",
            ""policy"": {
                ""id"": ""Strict"",
                ""name"": ""Strict Policy"",
                ""defaultDecision"": ""Deny"",
                ""enforcement"": ""Analyze"",
                ""imports"": [
                    { ""id"": ""Base"", ""mode"": ""Extend"" }
                ],
                ""rules"": [
                    {
                        ""ruleId"": ""XMoleculesBricks0001"",
                        ""name"": ""No infrastructure"",
                        ""sourceRole"": ""DDD.Entity"",
                        ""targetRole"": ""Infrastructure"",
                        ""decision"": ""Deny"",
                        ""scope"": ""Type"",
                        ""severity"": ""Error"",
                        ""priority"": 10,
                        ""reason"": ""Keep domain clean.""
                    }
                ],
                ""combinationRules"": [
                    {
                        ""name"": ""DDD+Business"",
                        ""leftRole"": ""DDD.*"",
                        ""rightRole"": ""Business.*"",
                        ""kind"": ""Additive"",
                        ""reason"": ""DDD roles can carry business partition roles.""
                    }
                ],
                ""externalAssignments"": [
                    {
                        ""selector"": {
                            ""kind"": ""Namespace"",
                            ""pattern"": ""Billing.Domain.*"",
                            ""assemblyName"": ""Billing""
                        },
                        ""roleId"": ""Business.Billing"",
                        ""mode"": ""ExternalConfiguration"",
                        ""source"": ""PolicyFile"",
                        ""precedence"": {
                            ""specificity"": ""Namespace"",
                            ""authority"": ""External""
                        },
                        ""behavior"": ""Apply"",
                        ""reason"": ""Project policy.""
                    }
                ],
                ""aliases"": [
                    {
                        ""name"": ""Repository suffix"",
                        ""selector"": {
                            ""kind"": ""Type"",
                            ""pattern"": ""*Repository""
                        },
                        ""canonicalRoleId"": ""DDD.Repository"",
                        ""precedence"": {
                            ""specificity"": ""Element"",
                            ""authority"": ""Alias""
                        },
                        ""behavior"": ""Apply"",
                        ""reason"": ""Convention bridge.""
                    }
                ]
            }
        }";
    }
}
