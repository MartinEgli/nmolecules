using System;
using System.Linq;
using System.Reflection;
using NMolecules.Architecture.Hexagonal;
using Xunit;

[assembly: global::NMolecules.Architecture.Hexagonal.Application]
[module: global::NMolecules.Architecture.Hexagonal.SecondaryPort]

namespace NMolecules.Architecture.Hexagonal.Test
{
    [Application]
    public interface IBankingCore
    {
    }

    [PrimaryPort(Name = "TransferMoney", Description = "Use-case entry point.")]
    public interface ITransferMoney
    {
    }

    [PrimaryAdapter(Name = "Http", Description = "HTTP controller adapter.")]
    public class AccountsController : ITransferMoney
    {
    }

    [SecondaryPort(Name = "Accounts", Description = "Persistence port.")]
    public interface IAccounts
    {
    }

    [SecondaryAdapter(Name = "Sql", Description = "SQL persistence adapter.")]
    public struct SqlAccounts
    {
    }

    public class HexagonalAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ApplicationAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(PrimaryAdapterAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(PrimaryPortAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(SecondaryAdapterAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(SecondaryPortAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
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
            var attributeNames = typeof(ApplicationAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(ApplicationAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ApplicationAttribute),
                nameof(PrimaryAdapterAttribute),
                nameof(PrimaryPortAttribute),
                nameof(SecondaryAdapterAttribute),
                nameof(SecondaryPortAttribute)
            }, attributeNames);
        }

        [Fact]
        public void SupportsApplicationsAdaptersPortsAssembliesAndModules()
        {
            var assembly = typeof(HexagonalAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(IBankingCore).IsDefined(typeof(ApplicationAttribute), false));
            Assert.True(typeof(ITransferMoney).IsDefined(typeof(PrimaryPortAttribute), false));
            Assert.True(typeof(AccountsController).IsDefined(typeof(PrimaryAdapterAttribute), false));
            Assert.True(typeof(IAccounts).IsDefined(typeof(SecondaryPortAttribute), false));
            Assert.True(typeof(SqlAccounts).IsDefined(typeof(SecondaryAdapterAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<ApplicationAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(SecondaryPortAttribute), false).SingleOrDefault());
        }

        [Fact]
        public void HexagonalMetadataDefaultsToEmptyStrings()
        {
            var primaryAdapter = new PrimaryAdapterAttribute();
            var primaryPort = new PrimaryPortAttribute();
            var secondaryAdapter = new SecondaryAdapterAttribute();
            var secondaryPort = new SecondaryPortAttribute();

            Assert.Equal(string.Empty, primaryAdapter.Name);
            Assert.Equal(string.Empty, primaryAdapter.Description);
            Assert.Equal(string.Empty, primaryPort.Name);
            Assert.Equal(string.Empty, primaryPort.Description);
            Assert.Equal(string.Empty, secondaryAdapter.Name);
            Assert.Equal(string.Empty, secondaryAdapter.Description);
            Assert.Equal(string.Empty, secondaryPort.Name);
            Assert.Equal(string.Empty, secondaryPort.Description);
        }
    }
}
