using System;
using System.Linq;
using System.Reflection;
using NMolecules.Architecture.EventStorming;
using Xunit;

[assembly: global::NMolecules.Architecture.EventStorming.Actor]
[module: global::NMolecules.Architecture.EventStorming.DomainEvent]

namespace NMolecules.Architecture.EventStorming.Test
{
    [Actor(Name = "Customer", Description = "Initiates checkout.")]
    public interface ICustomerActor
    {
    }

    [Command(Name = "PlaceOrder", Description = "Requests a new order.")]
    public class PlaceOrderCommand
    {
    }

    [Aggregate(Name = "Order", Description = "Order aggregate boundary.")]
    public class OrderAggregateBoundary
    {
    }

    [Policy(Name = "ReserveInventory", Description = "Reserves stock after order placed.")]
    public class ReserveInventoryPolicy
    {
    }

    [ReadModel(Name = "OrderSummary", Description = "Read-side projection for order status.")]
    public struct OrderSummaryReadModel
    {
    }

    [ExternalSystem(Name = "PaymentGateway", Description = "External payment provider.")]
    public interface IPaymentGateway
    {
    }

    [DomainEvent(Name = "OrderPlaced", Description = "Raised when an order is placed.")]
    public class OrderPlacedEvent
    {
    }

    public class EventStormingAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ActorAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(AggregateAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(CommandAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(DomainEventAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(ExternalSystemAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(PolicyAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(ReadModelAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
        };

        [Theory]
        [MemberData(nameof(AttributeTargetsData))]
        public void DeclaresExpectedAttributeUsage(Type attributeType, AttributeTargets expectedTargets)
        {
            var usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

            Assert.NotNull(usage);
            Assert.Equal(expectedTargets, usage!.ValidOn);
        }

        [Fact]
        public void ExposesExpectedArchitectureAttributeSet()
        {
            var attributeNames = typeof(ActorAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(ActorAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ActorAttribute),
                nameof(AggregateAttribute),
                nameof(CommandAttribute),
                nameof(DomainEventAttribute),
                nameof(ExternalSystemAttribute),
                nameof(PolicyAttribute),
                nameof(ReadModelAttribute)
            }, attributeNames);
        }

        [Fact]
        public void SupportsEventStormingMarkersAcrossTypesAssembliesAndModules()
        {
            var assembly = typeof(EventStormingAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(ICustomerActor).IsDefined(typeof(ActorAttribute), false));
            Assert.True(typeof(PlaceOrderCommand).IsDefined(typeof(CommandAttribute), false));
            Assert.True(typeof(OrderAggregateBoundary).IsDefined(typeof(AggregateAttribute), false));
            Assert.True(typeof(ReserveInventoryPolicy).IsDefined(typeof(PolicyAttribute), false));
            Assert.True(typeof(OrderSummaryReadModel).IsDefined(typeof(ReadModelAttribute), false));
            Assert.True(typeof(IPaymentGateway).IsDefined(typeof(ExternalSystemAttribute), false));
            Assert.True(typeof(OrderPlacedEvent).IsDefined(typeof(DomainEventAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<ActorAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(DomainEventAttribute), false).SingleOrDefault());
        }

        [Fact]
        public void EventStormingMetadataDefaultsToEmptyStrings()
        {
            var actor = new ActorAttribute();
            var aggregate = new AggregateAttribute();
            var command = new CommandAttribute();
            var domainEvent = new DomainEventAttribute();
            var externalSystem = new ExternalSystemAttribute();
            var policy = new PolicyAttribute();
            var readModel = new ReadModelAttribute();

            Assert.Equal(string.Empty, actor.Name);
            Assert.Equal(string.Empty, actor.Description);
            Assert.Equal(string.Empty, aggregate.Name);
            Assert.Equal(string.Empty, aggregate.Description);
            Assert.Equal(string.Empty, command.Name);
            Assert.Equal(string.Empty, command.Description);
            Assert.Equal(string.Empty, domainEvent.Name);
            Assert.Equal(string.Empty, domainEvent.Description);
            Assert.Equal(string.Empty, externalSystem.Name);
            Assert.Equal(string.Empty, externalSystem.Description);
            Assert.Equal(string.Empty, policy.Name);
            Assert.Equal(string.Empty, policy.Description);
            Assert.Equal(string.Empty, readModel.Name);
            Assert.Equal(string.Empty, readModel.Description);
        }
    }
}
