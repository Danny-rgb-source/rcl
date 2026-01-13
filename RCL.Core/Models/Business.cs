using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RCL.Core.Models
{
    /// <summary>
    /// Business entity. Contains a RewardRule configuration (serializable).
    /// Stored in DB as simple columns; the RewardRuleJson property can be used
    /// to persist reward configuration.
    /// </summary>
    public class Business
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// JSON-serializable reward rule configuration. When persisting to a relational DB,
        /// store this string; when reading, use GetRewardRule() helper.
        /// </summary>
        public string RewardRuleJson { get; set; } = string.Empty;

        /// <summary>
        /// Convenience property not persisted explicitly: created timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        private RewardRule? _cachedRule;

        /// <summary>
        /// Returns a RewardRule instance for this business. If none configured, returns a default fixed-visits rule.
        /// </summary>
        public RewardRule GetRewardRule()
        {
            if (_cachedRule != null) return _cachedRule;

            if (!string.IsNullOrWhiteSpace(RewardRuleJson))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    _cachedRule = JsonSerializer.Deserialize<RewardRule>(RewardRuleJson, options) ?? RewardRule.Default();
                    return _cachedRule;
                }
                catch
                {
                    // Fall through to default
                }
            }

            _cachedRule = RewardRule.Default();
            return _cachedRule;
        }

        public void SetRewardRule(RewardRule rule)
        {
            _cachedRule = rule;
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter() }
            };
            RewardRuleJson = JsonSerializer.Serialize(rule, options);
        }
    }
}

