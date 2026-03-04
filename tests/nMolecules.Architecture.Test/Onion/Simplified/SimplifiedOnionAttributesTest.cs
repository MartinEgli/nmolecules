using System;
using System.Linq;
using System.Reflection;
using NMolecules.Architecture.Onion.Simplified;
using Xunit;

[assembly: global::NMolecules.Architecture.Onion.Simplified.DomainRing]
[module: global::NMolecules.Architecture.Onion.Simplified.InfrastructureRing]

namespace NMolecules.Architecture.Onion.Simplified.Test
{
    [DomainRing]
    public interface IAggregateModel
    {
    }

    [ApplicationRing]
    public class TransferMoneyUseCase
    {
    }

    [InfrastructureRing]
    public struct SqlAccounts
    {
    }

    public class SimplifiedOnionAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ApplicationRingAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(DomainRingAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
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
            var attributeNames = typeof(DomainRingAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(DomainRingAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ApplicationRingAttribute),
                nameof(DomainRingAttribute),
                nameof(InfrastructureRingAttribute)
            }, attributeNames);
        }

        [Fact]
        public void SupportsSimplifiedOnionRingsAcrossTypeAssemblyAndModuleTargets()
        {
            var assembly = typeof(SimplifiedOnionAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(IAggregateModel).IsDefined(typeof(DomainRingAttribute), false));
            Assert.True(typeof(TransferMoneyUseCase).IsDefined(typeof(ApplicationRingAttribute), false));
            Assert.True(typeof(SqlAccounts).IsDefined(typeof(InfrastructureRingAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<DomainRingAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(InfrastructureRingAttribute), false).SingleOrDefault());
        }
    }
}
