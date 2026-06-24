using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Module |
        AttributeTargets.Class,
        AllowMultiple = true)]
    public class RuleAttribute : Attribute
    {
        private RuleFilter[] _filters = Array.Empty<RuleFilter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// Intended for specialized derived attributes.
        /// </summary>
        protected RuleAttribute()
        {
            Id = string.Empty;
            SourceRole = string.Empty;
            TargetRole = string.Empty;
            Mode = RuleMode.ForbidDependency;
            Message = string.Empty;
            _filters = Array.Empty<RuleFilter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="id">Consumer-defined rule identifier used in diagnostics.</param>
        /// <param name="sourceRole">Source role.</param>
        /// <param name="targetRole">Target role.</param>
        /// <param name="mode">Rule mode: forbid or require dependency.</param>
        /// <param name="message">Custom diagnostic message template.</param>
        public RuleAttribute(
            string id,
            string sourceRole,
            string targetRole,
            RuleMode mode = RuleMode.ForbidDependency,
            string message = "")
            : this(
                RuleId.From(id),
                RoleId.From(sourceRole),
                RoleId.From(targetRole),
                mode,
                RuleMessage.From(message),
                Array.Empty<RuleFilter>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="id">The typed rule identifier used in diagnostics.</param>
        /// <param name="sourceRole">The typed source role identifier.</param>
        /// <param name="targetRole">The typed target role identifier.</param>
        /// <param name="message">The typed rule message template.</param>
        /// <param name="mode">Rule mode: forbid or require dependency.</param>
        protected RuleAttribute(
            RuleId id,
            RoleId sourceRole,
            RoleId targetRole,
            RuleMessage message,
            RuleMode mode = RuleMode.ForbidDependency)
            : this(id, sourceRole, targetRole, mode, message, Array.Empty<RuleFilter>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="id">The typed rule identifier used in diagnostics.</param>
        /// <param name="sourceRole">The typed source role identifier.</param>
        /// <param name="targetRole">The typed target role identifier.</param>
        /// <param name="message">The typed rule message template.</param>
        /// <param name="filters">The optional specialized rule filters.</param>
        protected RuleAttribute(
            RuleId id,
            RoleId sourceRole,
            RoleId targetRole,
            RuleMessage message,
            params RuleFilter[] filters)
            : this(id, sourceRole, targetRole, RuleMode.ForbidDependency, message, filters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="id">The typed rule identifier used in diagnostics.</param>
        /// <param name="sourceRole">The typed source role identifier.</param>
        /// <param name="targetRole">The typed target role identifier.</param>
        /// <param name="mode">Rule mode: forbid or require dependency.</param>
        /// <param name="message">The typed rule message template.</param>
        /// <param name="filters">The optional specialized rule filters.</param>
        protected RuleAttribute(
            RuleId id,
            RoleId sourceRole,
            RoleId targetRole,
            RuleMode mode,
            RuleMessage message,
            params RuleFilter[] filters)
            : this(id, sourceRole, targetRole, mode, message, (IReadOnlyList<RuleFilter>)CloneFilters(filters))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleAttribute"/> class.
        /// </summary>
        /// <param name="id">The typed rule identifier used in diagnostics.</param>
        /// <param name="sourceRole">The typed source role identifier.</param>
        /// <param name="targetRole">The typed target role identifier.</param>
        /// <param name="filters">The optional specialized rule filters.</param>
        protected RuleAttribute(
            RuleId id,
            RoleId sourceRole,
            RoleId targetRole,
            params RuleFilter[] filters)
            : this(id, sourceRole, targetRole, RuleMode.ForbidDependency, RuleMessage.Empty, filters)
        {
        }

        private RuleAttribute(
            RuleId id,
            RoleId sourceRole,
            RoleId targetRole,
            RuleMode mode,
            RuleMessage message,
            IReadOnlyList<RuleFilter> filters)
        {
            Id = id.Value;
            SourceRole = sourceRole.Value;
            TargetRole = targetRole.Value;
            Mode = mode;
            Message = message.Value;
            _filters = CloneFilters(filters);
        }

        /// <summary>
        /// Consumer-defined rule identifier used in diagnostics.
        /// </summary>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the typed rule identifier representation of <see cref="Id"/>.
        /// </summary>
        public RuleId RuleId => RuleId.From(Id);

        /// <summary>
        /// Source role name.
        /// </summary>
        public virtual string SourceRole { get; protected set; }

        /// <summary>
        /// Gets the typed role identifier representation of <see cref="SourceRole"/>.
        /// </summary>
        public RoleId SourceRoleId => RoleId.From(SourceRole);

        /// <summary>
        /// Target role name.
        /// </summary>
        public virtual string TargetRole { get; protected set; }

        /// <summary>
        /// Gets the typed role identifier representation of <see cref="TargetRole"/>.
        /// </summary>
        public RoleId TargetRoleId => RoleId.From(TargetRole);

        /// <summary>
        /// Rule mode.
        /// </summary>
        public virtual RuleMode Mode { get; protected set; }

        /// <summary>
        /// Optional custom message template.
        /// Supported placeholders: {rule}, {source}, {target}, {member}.
        /// </summary>
        public virtual string Message { get; protected set; }

        /// <summary>
        /// Gets the optional rule message template as a typed value object.
        /// </summary>
        public RuleMessage MessageTemplate => RuleMessage.From(Message);

        /// <summary>
        /// Gets the optional rule filters as specialized filter objects.
        /// </summary>
        public RuleFilter[] Filters => CloneFilters(_filters);

        private static RuleFilter[] CloneFilters(IEnumerable<RuleFilter> filters)
        {
            return filters?.ToArray() ?? Array.Empty<RuleFilter>();
        }

    }
}
