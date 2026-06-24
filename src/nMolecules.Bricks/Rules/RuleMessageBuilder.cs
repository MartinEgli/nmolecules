using System.Text;

namespace NMolecules.Bricks
{
/// <summary>
    /// Fluent builder for assembling <see cref="RuleMessage"/> templates.
    /// </summary>
    public sealed class RuleMessageBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// Appends literal text to the message template.
        /// </summary>
        public RuleMessageBuilder Text(string value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the <c>{rule}</c> placeholder.
        /// </summary>
        public RuleMessageBuilder Rule()
        {
            _builder.Append(RuleMessage.RulePlaceholder);
            return this;
        }

        /// <summary>
        /// Appends the <c>{source}</c> placeholder.
        /// </summary>
        public RuleMessageBuilder Source()
        {
            _builder.Append(RuleMessage.SourcePlaceholder);
            return this;
        }

        /// <summary>
        /// Appends the <c>{target}</c> placeholder.
        /// </summary>
        public RuleMessageBuilder Target()
        {
            _builder.Append(RuleMessage.TargetPlaceholder);
            return this;
        }

        /// <summary>
        /// Appends the <c>{member}</c> placeholder.
        /// </summary>
        public RuleMessageBuilder Member()
        {
            _builder.Append(RuleMessage.MemberPlaceholder);
            return this;
        }

        /// <summary>
        /// Builds the final typed rule message template.
        /// </summary>
        public RuleMessage Build()
        {
            return new RuleMessage(_builder.ToString());
        }
    }
}
