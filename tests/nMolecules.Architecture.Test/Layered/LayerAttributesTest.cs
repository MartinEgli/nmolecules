using System;
using System.Linq;
using System.Reflection;
using Xunit;

[assembly: global::NMolecules.Architecture.Layered.ApplicationLayer]
[module: global::NMolecules.Architecture.Layered.InfrastructureLayer]

namespace NMolecules.Architecture.Layered.Test
{
    [ApplicationLayer]
    public interface ITransferMoneyUseCase
    {
    }

    [ApplicationLayer]
    public class TransferMoney : ITransferMoneyUseCase
    {
    }

    [DomainLayer]
    public class BankAccount
    {
    }

    [InfrastructureLayer]
    public class SqlAccounts
    {
    }

    [UserInterfaceLayer]
    public struct AccountsApi
    {
    }

    public class LayerAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ApplicationLayerAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(DomainLayerAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(InfrastructureLayerAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(UserInterfaceLayerAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
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
            var attributeNames = typeof(ApplicationLayerAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(ApplicationLayerAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ApplicationLayerAttribute),
                nameof(DomainLayerAttribute),
                nameof(InfrastructureLayerAttribute),
                nameof(UserInterfaceLayerAttribute)
            }, attributeNames);
        }

        [Fact]
        public void LayerAttributesSupportClassesInterfacesStructsAssembliesAndModules()
        {
            var assembly = typeof(LayerAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(ITransferMoneyUseCase).IsDefined(typeof(ApplicationLayerAttribute), false));
            Assert.True(typeof(TransferMoney).IsDefined(typeof(ApplicationLayerAttribute), false));
            Assert.True(typeof(BankAccount).IsDefined(typeof(DomainLayerAttribute), false));
            Assert.True(typeof(SqlAccounts).IsDefined(typeof(InfrastructureLayerAttribute), false));
            Assert.True(typeof(AccountsApi).IsDefined(typeof(UserInterfaceLayerAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<ApplicationLayerAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(InfrastructureLayerAttribute), false).SingleOrDefault());
        }
    }
}
