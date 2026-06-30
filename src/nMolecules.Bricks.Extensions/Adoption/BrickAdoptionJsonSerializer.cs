using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
/// Serializes and deserializes adoption serializer documents using the stable Bricks JSON format.
/// </summary>
public static class BrickAdoptionJsonSerializer
    {
        private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        private static readonly JsonSerializerOptions ReadOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static string Serialize(BrickAdoptionDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(ToDto(document), WriteOptions);
        }

        public static BrickAdoptionDocument Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var dto = JsonSerializer.Deserialize<AdoptionDocumentDto>(json, ReadOptions);
            if (dto == null)
            {
                throw new ArgumentException("Adoption JSON must contain an object.", nameof(json));
            }

            return new BrickAdoptionDocument(
                dto.GeneratedAt,
                (dto.Baselines ?? new BaselineDto[0]).Select(ToBaseline),
                (dto.Suppressions ?? new SuppressionDto[0]).Select(ToSuppression),
                dto.Schema);
        }

        private static AdoptionDocumentDto ToDto(BrickAdoptionDocument document) =>
            new AdoptionDocumentDto
            {
                Schema = document.Schema,
                GeneratedAt = document.GeneratedAt,
                Baselines = document.Baselines
                    .Select(baseline => new BaselineDto
                    {
                        RuleId = baseline.RuleId.Value,
                        SourcePattern = baseline.SourcePattern,
                        TargetPattern = baseline.TargetPattern,
                        Justification = baseline.Justification,
                        Owner = baseline.Owner,
                        ExpiresAt = baseline.ExpiresAt
                    })
                    .ToArray(),
                Suppressions = document.Suppressions
                    .Select(suppression => new SuppressionDto
                    {
                        RuleId = suppression.RuleId.Value,
                        Selector = ToDto(suppression.Selector),
                        Justification = suppression.Justification,
                        Owner = suppression.Owner,
                        ExpiresAt = suppression.ExpiresAt
                    })
                    .ToArray()
            };

        private static BrickBaselineEntry ToBaseline(BaselineDto dto) =>
            new BrickBaselineEntry(
                RuleId.From(dto.RuleId),
                dto.SourcePattern,
                dto.TargetPattern,
                dto.Justification,
                dto.Owner,
                dto.ExpiresAt);

        private static BrickSuppression ToSuppression(SuppressionDto dto) =>
            new BrickSuppression(
                RuleId.From(dto.RuleId),
                ToSelector(dto.Selector),
                dto.Justification,
                dto.Owner,
                dto.ExpiresAt);

        private static SelectorDto ToDto(BrickElementSelector selector) =>
            new SelectorDto
            {
                Kind = selector.Kind.ToString(),
                Pattern = selector.Pattern,
                AssemblyName = selector.AssemblyName
            };

        private static BrickElementSelector ToSelector(SelectorDto dto) =>
            dto == null
                ? new BrickElementSelector(BrickElementKind.Unknown, string.Empty)
                : new BrickElementSelector(Parse<BrickElementKind>(dto.Kind), dto.Pattern, dto.AssemblyName);

        private static TEnum Parse<TEnum>(string value)
            where TEnum : struct =>
            (TEnum)Enum.Parse(typeof(TEnum), value, false);

        private sealed class AdoptionDocumentDto
        {
            public string Schema { get; set; }
            public DateTimeOffset GeneratedAt { get; set; }
            public BaselineDto[] Baselines { get; set; }
            public SuppressionDto[] Suppressions { get; set; }
        }

        private sealed class BaselineDto
        {
            public string RuleId { get; set; }
            public string SourcePattern { get; set; }
            public string TargetPattern { get; set; }
            public string Justification { get; set; }
            public string Owner { get; set; }
            public DateTimeOffset? ExpiresAt { get; set; }
        }

        private sealed class SuppressionDto
        {
            public string RuleId { get; set; }
            public SelectorDto Selector { get; set; }
            public string Justification { get; set; }
            public string Owner { get; set; }
            public DateTimeOffset? ExpiresAt { get; set; }
        }

        private sealed class SelectorDto
        {
            public string Kind { get; set; }
            public string Pattern { get; set; }
            public string AssemblyName { get; set; }
        }
    }
}
