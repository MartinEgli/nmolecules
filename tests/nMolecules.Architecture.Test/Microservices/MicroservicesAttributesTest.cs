using System;
using System.Linq;
using System.Reflection;
using NMolecules.Architecture.Microservices;
using Xunit;

[assembly: global::NMolecules.Architecture.Microservices.Microservice]
[module: global::NMolecules.Architecture.Microservices.IntegrationEvent]

namespace NMolecules.Architecture.Microservices.Test
{
    [Microservice(Name = "Billing", Description = "Service boundary for billing workflows.")]
    public class BillingServiceBoundary
    {
    }

    [ApiGateway(Name = "PublicGateway", Description = "Public API entry point.")]
    public class PublicGateway
    {
    }

    [BackendForFrontend(Name = "PortalBff", Description = "Frontend-specific API composition.")]
    public interface IPortalBff
    {
    }

    [ServiceContract(Name = "Payments.Contract.V1", Description = "Payment contract between services.")]
    public interface IPaymentsContract
    {
    }

    [IntegrationEvent(Name = "InvoiceCreated", Description = "Raised after invoice creation.")]
    public class InvoiceCreated
    {
    }

    [SagaOrchestrator(Name = "OrderSaga", Description = "Coordinates order fulfillment.")]
    public class OrderSaga
    {
    }

    [SagaParticipant(Name = "InventoryParticipant", Description = "Participates in order saga.")]
    public struct InventoryParticipant
    {
    }

    public class MicroservicesAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ApiGatewayAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(BackendForFrontendAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(IntegrationEventAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(MicroserviceAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(SagaOrchestratorAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(SagaParticipantAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(ServiceContractAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
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
            var attributeNames = typeof(MicroserviceAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(MicroserviceAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ApiGatewayAttribute),
                nameof(BackendForFrontendAttribute),
                nameof(IntegrationEventAttribute),
                nameof(MicroserviceAttribute),
                nameof(SagaOrchestratorAttribute),
                nameof(SagaParticipantAttribute),
                nameof(ServiceContractAttribute)
            }, attributeNames);
        }

        [Fact]
        public void SupportsMicroservicesMarkersAcrossTypesAssembliesAndModules()
        {
            var assembly = typeof(MicroservicesAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(BillingServiceBoundary).IsDefined(typeof(MicroserviceAttribute), false));
            Assert.True(typeof(PublicGateway).IsDefined(typeof(ApiGatewayAttribute), false));
            Assert.True(typeof(IPortalBff).IsDefined(typeof(BackendForFrontendAttribute), false));
            Assert.True(typeof(IPaymentsContract).IsDefined(typeof(ServiceContractAttribute), false));
            Assert.True(typeof(InvoiceCreated).IsDefined(typeof(IntegrationEventAttribute), false));
            Assert.True(typeof(OrderSaga).IsDefined(typeof(SagaOrchestratorAttribute), false));
            Assert.True(typeof(InventoryParticipant).IsDefined(typeof(SagaParticipantAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<MicroserviceAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(IntegrationEventAttribute), false).SingleOrDefault());
        }

        [Fact]
        public void MicroservicesMetadataDefaultsToEmptyStrings()
        {
            var apiGateway = new ApiGatewayAttribute();
            var backendForFrontend = new BackendForFrontendAttribute();
            var integrationEvent = new IntegrationEventAttribute();
            var microservice = new MicroserviceAttribute();
            var sagaOrchestrator = new SagaOrchestratorAttribute();
            var sagaParticipant = new SagaParticipantAttribute();
            var serviceContract = new ServiceContractAttribute();

            Assert.Equal(string.Empty, apiGateway.Name);
            Assert.Equal(string.Empty, apiGateway.Description);
            Assert.Equal(string.Empty, backendForFrontend.Name);
            Assert.Equal(string.Empty, backendForFrontend.Description);
            Assert.Equal(string.Empty, integrationEvent.Name);
            Assert.Equal(string.Empty, integrationEvent.Description);
            Assert.Equal(string.Empty, microservice.Name);
            Assert.Equal(string.Empty, microservice.Description);
            Assert.Equal(string.Empty, sagaOrchestrator.Name);
            Assert.Equal(string.Empty, sagaOrchestrator.Description);
            Assert.Equal(string.Empty, sagaParticipant.Name);
            Assert.Equal(string.Empty, sagaParticipant.Description);
            Assert.Equal(string.Empty, serviceContract.Name);
            Assert.Equal(string.Empty, serviceContract.Description);
        }
    }
}
