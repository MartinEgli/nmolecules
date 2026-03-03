using System;
using System.Linq;
using System.Reflection;
using Xunit;

[assembly: global::NMolecules.DDD.BoundedContext]
[module: global::NMolecules.DDD.Module]

namespace NMolecules.DDD
{
    [ValueObject]
    public struct Iban
    {
    }

    [ValueObject]
    public enum Currency
    {
        Eur
    }

    [ValueObject]
    public class Money
    {
    }

    [Entity]
    public class BankAccount
    {
        [Identity]
        private readonly Iban iban = default;

        [Identity]
        public Iban SecondaryId => iban;
    }

    [AggregateRoot]
    public class AccountAggregate
    {
        [Identity]
        public Iban Id => default;
    }

    [Repository]
    public interface IAccountRepository
    {
    }

    [Repository]
    public class AccountRepository : IAccountRepository
    {
    }

    [Factory]
    public interface IAccountFactory
    {
    }

    [Factory]
    public class AccountFactory : IAccountFactory
    {
    }

    [Service]
    public interface IPricingService
    {
    }

    [Service]
    public class PricingService : IPricingService
    {
    }

    [DomainService]
    public interface IExchangeRates
    {
    }

    [DomainService]
    public class ExchangeRates : IExchangeRates
    {
    }

    [ApplicationService]
    public interface IMoneyTransferUseCase
    {
    }

    [ApplicationService]
    public class TransferMoney : IMoneyTransferUseCase
    {
    }

    public class DDDAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(AggregateRootAttribute), AttributeTargets.Class },
            { typeof(ApplicationServiceAttribute), AttributeTargets.Class | AttributeTargets.Interface },
            { typeof(BoundedContextAttribute), AttributeTargets.Assembly | AttributeTargets.Module },
            { typeof(DomainServiceAttribute), AttributeTargets.Class | AttributeTargets.Interface },
            { typeof(EntityAttribute), AttributeTargets.Class },
            { typeof(FactoryAttribute), AttributeTargets.Class | AttributeTargets.Interface },
            { typeof(IdentityAttribute), AttributeTargets.Property | AttributeTargets.Field },
            { typeof(ModuleAttribute), AttributeTargets.Assembly | AttributeTargets.Module },
            { typeof(RepositoryAttribute), AttributeTargets.Class | AttributeTargets.Interface },
            { typeof(ServiceAttribute), AttributeTargets.Class | AttributeTargets.Interface },
            { typeof(ValueObjectAttribute), AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct }
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
        public void ExposesExpectedDddAttributeSet()
        {
            var attributeNames = typeof(EntityAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(EntityAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(AggregateRootAttribute),
                nameof(ApplicationServiceAttribute),
                nameof(BoundedContextAttribute),
                nameof(DomainServiceAttribute),
                nameof(EntityAttribute),
                nameof(FactoryAttribute),
                nameof(IdentityAttribute),
                nameof(ModuleAttribute),
                nameof(RepositoryAttribute),
                nameof(ServiceAttribute),
                nameof(ValueObjectAttribute)
            }, attributeNames);
        }

        [Fact]
        public void ValueObjectAttributeCanMarkClassStructAndEnum()
        {
            Assert.True(typeof(Iban).IsDefined(typeof(ValueObjectAttribute), false));
            Assert.True(typeof(Currency).IsDefined(typeof(ValueObjectAttribute), false));
            Assert.True(typeof(Money).IsDefined(typeof(ValueObjectAttribute), false));
        }

        [Fact]
        public void EntityAndAggregateRootAttributesAreDiscoverable()
        {
            Assert.True(typeof(BankAccount).IsDefined(typeof(EntityAttribute), false));
            Assert.True(typeof(AccountAggregate).IsDefined(typeof(AggregateRootAttribute), false));
            Assert.IsType<AggregateRootAttribute>(typeof(AccountAggregate).GetCustomAttribute<EntityAttribute>());
        }

        [Fact]
        public void IdentityAttributeCanMarkFieldsAndProperties()
        {
            var field = typeof(BankAccount).GetField("iban", BindingFlags.Instance | BindingFlags.NonPublic);
            var property = typeof(BankAccount).GetProperty(nameof(BankAccount.SecondaryId), BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(field);
            Assert.NotNull(property);
            Assert.True(field!.IsDefined(typeof(IdentityAttribute), false));
            Assert.True(property!.IsDefined(typeof(IdentityAttribute), false));
        }

        [Fact]
        public void RepositoryAndFactoryAttributesSupportClassesAndInterfaces()
        {
            Assert.True(typeof(IAccountRepository).IsDefined(typeof(RepositoryAttribute), false));
            Assert.True(typeof(AccountRepository).IsDefined(typeof(RepositoryAttribute), false));
            Assert.True(typeof(IAccountFactory).IsDefined(typeof(FactoryAttribute), false));
            Assert.True(typeof(AccountFactory).IsDefined(typeof(FactoryAttribute), false));
        }

        [Fact]
        public void ServiceRoleAttributesSupportClassesAndInterfaces()
        {
            Assert.True(typeof(IPricingService).IsDefined(typeof(ServiceAttribute), false));
            Assert.True(typeof(PricingService).IsDefined(typeof(ServiceAttribute), false));
            Assert.True(typeof(IExchangeRates).IsDefined(typeof(DomainServiceAttribute), false));
            Assert.True(typeof(ExchangeRates).IsDefined(typeof(DomainServiceAttribute), false));
            Assert.True(typeof(IMoneyTransferUseCase).IsDefined(typeof(ApplicationServiceAttribute), false));
            Assert.True(typeof(TransferMoney).IsDefined(typeof(ApplicationServiceAttribute), false));
        }

        [Fact]
        public void AssemblyAndModuleAttributesAreDiscoverable()
        {
            var assembly = typeof(DDDAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.NotNull(assembly.GetCustomAttribute<BoundedContextAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(ModuleAttribute), false).SingleOrDefault());
        }
    }
}
