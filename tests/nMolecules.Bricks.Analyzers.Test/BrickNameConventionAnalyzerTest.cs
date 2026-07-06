using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NMolecules.Bricks.Analyzers;
using Xunit;

namespace NMolecules.Bricks.Analyzers.Test
{
    public class BrickNameConventionAnalyzerTest
    {
        [Theory]
        [InlineData("NamePosition.Prefix", "DomainEventOrder", "Order", "begin with")]
        [InlineData("NamePosition.Suffix", "OrderDomainEvent", "Order", "end with")]
        [InlineData("NamePosition.Contains", "OrderDomainEvent", "Order", "contain")]
        [InlineData("NamePosition.Exact", "DomainEvent", "Order", "equal")]
        public async Task ReportsNameConventionViolationsForEveryPosition(
            string position,
            string validTypeName,
            string invalidTypeName,
            string messagePart)
        {
            var validDiagnostics = await AnalyzeAsync($@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", {position}, Reason = ""Name must expose its event role."")]
public interface IDomainEventMarker {{ }}

public sealed class {validTypeName} : IDomainEventMarker {{ }}
");

            var invalidDiagnostics = await AnalyzeAsync($@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", {position}, Reason = ""Name must expose its event role."")]
public interface IDomainEventMarker {{ }}

public sealed class {invalidTypeName} : IDomainEventMarker {{ }}
");

            Assert.Empty(validDiagnostics);
            var diagnostic = Assert.Single(invalidDiagnostics);
            Assert.Equal("XMoleculesBricks0020", diagnostic.Id);
            Assert.Contains(messagePart, diagnostic.GetMessage());
            Assert.Equal("ElementConstraint", diagnostic.Properties["BrickViolationKind"]);
            Assert.Equal("IDomainEventMarker", diagnostic.Properties["Source"]);
            Assert.Equal(invalidTypeName, diagnostic.Properties["Target"]);
        }

        [Fact]
        public async Task ImportsNameConventionAliases()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", NamePosition.Suffix)]
public interface IExternalEventMarker { }

[NameConventionAlias(typeof(IExternalEventMarker))]
public interface ILocalEventMarker { }

public sealed class OrderPlaced : ILocalEventMarker { }
");

            Assert.Equal(new[] { "XMoleculesBricks0020" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task ReportsAliasWithoutConventionSource()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

public interface IExternalEventMarker { }

[NameConventionAlias(typeof(IExternalEventMarker))]
public interface ILocalEventMarker { }
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0022", diagnostic.Id);
            Assert.Equal("NameConventionAlias", diagnostic.Properties["BrickConfigurationKind"]);
        }

        [Fact]
        public async Task AliasCanRestrictImportedPosition()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""Domain"", NamePosition.Prefix)]
[NameConvention(""Event"", NamePosition.Suffix)]
public interface IExternalEventMarker { }

[NameConventionAlias(typeof(IExternalEventMarker), RestrictToPosition = NamePosition.Suffix)]
public interface ILocalEventMarker { }

public sealed class IntegrationEvent : ILocalEventMarker { }
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task ReportsConflictingNameConventions()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", NamePosition.Suffix)]
public interface IDomainEvent { }

[NameConvention(""IntegrationEvent"", NamePosition.Suffix)]
public interface IIntegrationEvent { }

public sealed class OrderPlaced : IDomainEvent, IIntegrationEvent { }
");

            Assert.Contains("XMoleculesBricks0021", DiagnosticIds(diagnostics));
            var conflict = Assert.Single(diagnostics, diagnostic => diagnostic.Id == "XMoleculesBricks0021");
            Assert.Equal("ElementConstraintConflict", conflict.Properties["BrickViolationKind"]);
        }

        [Fact]
        public async Task AlternativeNameConventionsAllowOneMatchingSuffix()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""Command"", NamePosition.Suffix, Requirement = NameConventionRequirement.Alternative)]
public interface ICommandName { }

[NameConvention(""Request"", NamePosition.Suffix, Requirement = NameConventionRequirement.Alternative)]
public interface IRequestName { }

public sealed class SubmitOrderCommand : ICommandName, IRequestName { }

public sealed class SubmitOrderRequest : ICommandName, IRequestName { }
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task AlternativeNameConventionsReportWhenNoAlternativeMatches()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""Command"", NamePosition.Suffix, Requirement = NameConventionRequirement.Alternative)]
public interface ICommandName { }

[NameConvention(""Request"", NamePosition.Suffix, Requirement = NameConventionRequirement.Alternative)]
public interface IRequestName { }

public sealed class SubmitOrder : ICommandName, IRequestName { }
");

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XMoleculesBricks0020", diagnostic.Id);
            Assert.Contains("satisfies none", diagnostic.GetMessage());
            Assert.DoesNotContain("XMoleculesBricks0021", DiagnosticIds(diagnostics));
            Assert.Equal("ElementConstraint", diagnostic.Properties["BrickViolationKind"]);
        }

        [Fact]
        public async Task DirectOverrideSuppressesActiveConvention()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", NamePosition.Suffix)]
public interface IDomainEvent { }

[NameConvention(""IntegrationEvent"", NamePosition.Suffix)]
public interface IIntegrationEvent { }

[NameConventionOverride(typeof(IIntegrationEvent), NameConventionOverrideBehavior.Suppress, Reason = ""Domain event name is canonical."")]
public sealed class OrderPlacedDomainEvent : IDomainEvent, IIntegrationEvent { }
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task PreferOverrideKeepsPreferredConventionAndSuppressesConflicts()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", NamePosition.Suffix)]
public interface IDomainEvent { }

[NameConvention(""IntegrationEvent"", NamePosition.Suffix)]
public interface IIntegrationEvent { }

[NameConventionOverride(typeof(IIntegrationEvent), NameConventionOverrideBehavior.Prefer, Reason = ""Integration event name is exposed externally."")]
public sealed class OrderPlacedIntegrationEvent : IDomainEvent, IIntegrationEvent { }
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task OverrideAliasSuppressesConventionForGeneratedType()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[assembly: NameConventionOverrideAlias(
    typeof(OrderPlacedDomainEvent),
    typeof(IIntegrationEvent),
    NameConventionOverrideBehavior.Suppress,
    Reason = ""Generated type cannot be annotated directly."")]

[NameConvention(""DomainEvent"", NamePosition.Suffix)]
public interface IDomainEvent { }

[NameConvention(""IntegrationEvent"", NamePosition.Suffix)]
public interface IIntegrationEvent { }

public sealed class OrderPlacedDomainEvent : IDomainEvent, IIntegrationEvent { }
");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task ReportsOverrideSourceThatIsNotActive()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""DomainEvent"", NamePosition.Suffix)]
public interface IDomainEvent { }

public interface IExternalEvent { }

[NameConventionOverride(typeof(IExternalEvent), Reason = ""This source is not active."")]
public sealed class OrderPlacedDomainEvent : IDomainEvent { }
");

            Assert.Equal(new[] { "XMoleculesBricks0023" }, DiagnosticIds(diagnostics));
        }

        [Fact]
        public async Task DirectOnlyConventionDoesNotApplyTransitively()
        {
            var diagnostics = await AnalyzeAsync(@"
using NMolecules.Bricks;

[NameConvention(""Domain"", NamePosition.Prefix, DirectOnly = true)]
public interface IBaseEvent { }

public interface DomainDerivedEvent : IBaseEvent { }

public sealed class OrderPlaced : DomainDerivedEvent { }
");

            Assert.Empty(diagnostics);
        }

        private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync(string source)
        {
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new BrickNameConventionAnalyzer());
            var diagnostics = await BrickAnalyzerTestFixture.AnalyzeAsync(
                "NameConventionFixture",
                source,
                analyzers);

            return diagnostics.OrderBy(diagnostic => diagnostic.Id).ThenBy(diagnostic => diagnostic.GetMessage()).ToArray();
        }

        private static string[] DiagnosticIds(IEnumerable<Diagnostic> diagnostics) =>
            diagnostics.Select(diagnostic => diagnostic.Id).OrderBy(id => id).ToArray();
    }
}
