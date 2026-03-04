using System;
using System.Linq;
using System.Reflection;
using NMolecules.Architecture.Onion.Classic;
using Xunit;

[assembly: global::NMolecules.Architecture.Onion.Classic.DomainModelRing]
[module: global::NMolecules.Architecture.Onion.Classic.InfrastructureRing]

namespace NMolecules.Architecture.Onion.Classic.Test
{
    [DomainModelRing]
    public interface IAggregateModel
    {
    }

    [DomainServiceRing]
    public class PricingService
    {
    }

    [ApplicationServiceRing]
    public struct TransferMoneyUseCase
    {
    }

    [InfrastructureRing]
    public class SqlAccounts
    {
    }

    public class ClassicOnionAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ApplicationServiceRingAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(DomainModelRingAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(DomainServiceRingAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(InfrastructureRingAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
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
            var attributeNames = typeof(DomainModelRingAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(DomainModelRingAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ApplicationServiceRingAttribute),
                nameof(DomainModelRingAttribute),
                nameof(DomainServiceRingAttribute),
                nameof(InfrastructureRingAttribute)
            }, attributeNames);
        }

        [Fact]
        public void SupportsClassicOnionRingsAcrossTypeAssemblyAndModuleTargets()
        {
            var assembly = typeof(ClassicOnionAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(IAggregateModel).IsDefined(typeof(DomainModelRingAttribute), false));
            Assert.True(typeof(PricingService).IsDefined(typeof(DomainServiceRingAttribute), false));
            Assert.True(typeof(TransferMoneyUseCase).IsDefined(typeof(ApplicationServiceRingAttribute), false));
            Assert.True(typeof(SqlAccounts).IsDefined(typeof(InfrastructureRingAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<DomainModelRingAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(InfrastructureRingAttribute), false).SingleOrDefault());
        }
    }
}
