using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksAdoptionJsonSerializerTest
    {
        [Fact]
        public void SerializeWritesBaselinesAndSuppressions()
        {
            var generatedAt = new DateTimeOffset(2026, 6, 23, 19, 0, 0, TimeSpan.Zero);
            var expiresAt = generatedAt.AddDays(30);
            var document = new BrickAdoptionDocument(
                generatedAt,
                new[]
                {
                    new BrickBaselineEntry(RuleId.From("XMoleculesBricks0001"), "Billing.Domain.*", "Billing.Infrastructure.*", "Migration debt.", "Architecture", expiresAt)
                },
                new[]
                {
                    new BrickSuppression(RuleId.From("XMoleculesBricks0002"), new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.LegacyOrder", "Billing"), "Intentional exception.", "Billing", expiresAt)
                });

            using var json = JsonDocument.Parse(BrickAdoptionJsonSerializer.Serialize(document));
            var root = json.RootElement;
            var baseline = root.GetProperty("baselines").EnumerateArray().Single();
            var suppression = root.GetProperty("suppressions").EnumerateArray().Single();

            Assert.Equal(BrickAdoptionDocument.CurrentSchema, root.GetProperty("schema").GetString());
            Assert.Equal(generatedAt, root.GetProperty("generatedAt").GetDateTimeOffset());
            Assert.Equal("XMoleculesBricks0001", baseline.GetProperty("ruleId").GetString());
            Assert.Equal("Billing.Domain.*", baseline.GetProperty("sourcePattern").GetString());
            Assert.Equal("Billing.Infrastructure.*", baseline.GetProperty("targetPattern").GetString());
            Assert.Equal("Migration debt.", baseline.GetProperty("justification").GetString());
            Assert.Equal("Architecture", baseline.GetProperty("owner").GetString());
            Assert.Equal(expiresAt, baseline.GetProperty("expiresAt").GetDateTimeOffset());
            Assert.Equal("XMoleculesBricks0002", suppression.GetProperty("ruleId").GetString());
            Assert.Equal("Type", suppression.GetProperty("selector").GetProperty("kind").GetString());
            Assert.Equal("Billing.Domain.LegacyOrder", suppression.GetProperty("selector").GetProperty("pattern").GetString());
            Assert.Equal("Billing", suppression.GetProperty("selector").GetProperty("assemblyName").GetString());
            Assert.Equal("Intentional exception.", suppression.GetProperty("justification").GetString());
        }

        [Fact]
        public void DeserializeReadsBaselinesAndSuppressions()
        {
            var document = BrickAdoptionJsonSerializer.Deserialize(@"{
                ""schema"": ""NMolecules.Bricks.Adoption/1.0"",
                ""generatedAt"": ""2026-06-23T19:00:00+00:00"",
                ""baselines"": [
                    {
                        ""ruleId"": ""XMoleculesBricks0001"",
                        ""sourcePattern"": ""Billing.Domain.*"",
                        ""targetPattern"": ""Billing.Infrastructure.*"",
                        ""justification"": ""Migration debt."",
                        ""owner"": ""Architecture"",
                        ""expiresAt"": ""2026-07-23T19:00:00+00:00""
                    }
                ],
                ""suppressions"": [
                    {
                        ""ruleId"": ""XMoleculesBricks0002"",
                        ""selector"": {
                            ""kind"": ""Type"",
                            ""pattern"": ""Billing.Domain.LegacyOrder"",
                            ""assemblyName"": ""Billing""
                        },
                        ""justification"": ""Intentional exception."",
                        ""owner"": ""Billing"",
                        ""expiresAt"": ""2026-07-23T19:00:00+00:00""
                    }
                ]
            }");

            Assert.Equal(BrickAdoptionDocument.CurrentSchema, document.Schema);
            Assert.True(document.IsCurrentSchema);
            Assert.Empty(BrickAdoptionDocumentValidator.Validate(document));
            Assert.Equal(new DateTimeOffset(2026, 6, 23, 19, 0, 0, TimeSpan.Zero), document.GeneratedAt);

            var baseline = document.Baselines.Single();
            Assert.Equal(RuleId.From("XMoleculesBricks0001"), baseline.RuleId);
            Assert.Equal("Billing.Domain.*", baseline.SourcePattern);
            Assert.Equal("Billing.Infrastructure.*", baseline.TargetPattern);
            Assert.Equal("Migration debt.", baseline.Justification);
            Assert.Equal("Architecture", baseline.Owner);
            Assert.Equal(new DateTimeOffset(2026, 7, 23, 19, 0, 0, TimeSpan.Zero), baseline.ExpiresAt);

            var suppression = document.Suppressions.Single();
            Assert.Equal(RuleId.From("XMoleculesBricks0002"), suppression.RuleId);
            Assert.Equal(new BrickElementSelector(BrickElementKind.Type, "Billing.Domain.LegacyOrder", "Billing"), suppression.Selector);
            Assert.Equal("Intentional exception.", suppression.Justification);
            Assert.Equal("Billing", suppression.Owner);
            Assert.Equal(new DateTimeOffset(2026, 7, 23, 19, 0, 0, TimeSpan.Zero), suppression.ExpiresAt);
        }

        [Fact]
        public void DeserializeNormalizesMissingCollectionsAndSelector()
        {
            var document = BrickAdoptionJsonSerializer.Deserialize(@"{
                ""schema"": ""NMolecules.Bricks.Adoption/1.0"",
                ""generatedAt"": ""2026-06-23T19:00:00+00:00"",
                ""suppressions"": [
                    {
                        ""ruleId"": ""XMoleculesBricks0002"",
                        ""justification"": ""Intentional exception.""
                    }
                ]
            }");

            Assert.Empty(document.Baselines);
            Assert.Equal(new BrickElementSelector(BrickElementKind.Unknown, string.Empty), document.Suppressions.Single().Selector);
        }

        [Fact]
        public void DeserializeNormalizesMissingSuppressions()
        {
            var document = BrickAdoptionJsonSerializer.Deserialize(@"{
                ""schema"": ""NMolecules.Bricks.Adoption/1.0"",
                ""generatedAt"": ""2026-06-23T19:00:00+00:00"",
                ""baselines"": [
                    {
                        ""ruleId"": ""XMoleculesBricks0001"",
                        ""sourcePattern"": ""Source"",
                        ""targetPattern"": ""Target""
                    }
                ]
            }");

            Assert.Single(document.Baselines);
            Assert.Empty(document.Suppressions);
        }

        [Fact]
        public void SerializeOmitsNullOptionalProperties()
        {
            var document = new BrickAdoptionDocument(
                DateTimeOffset.UnixEpoch,
                new[] { new BrickBaselineEntry(RuleId.From("XMoleculesBricks0001"), "Source", "Target") },
                new[] { new BrickSuppression(RuleId.From("XMoleculesBricks0002"), new BrickElementSelector(BrickElementKind.Type, "Type"), "Reason") });

            using var json = JsonDocument.Parse(BrickAdoptionJsonSerializer.Serialize(document));
            var baseline = json.RootElement.GetProperty("baselines").EnumerateArray().Single();
            var suppressionSelector = json.RootElement.GetProperty("suppressions").EnumerateArray().Single().GetProperty("selector");

            Assert.False(baseline.TryGetProperty("owner", out _));
            Assert.False(baseline.TryGetProperty("expiresAt", out _));
            Assert.False(suppressionSelector.TryGetProperty("assemblyName", out _));
        }

        [Fact]
        public void SerializerRequiresInputObjects()
        {
            Assert.Throws<ArgumentNullException>(() => BrickAdoptionJsonSerializer.Serialize(null));
            Assert.Throws<ArgumentNullException>(() => BrickAdoptionJsonSerializer.Deserialize(null));
            Assert.Throws<ArgumentException>(() => BrickAdoptionJsonSerializer.Deserialize("null"));
        }
    }
}
