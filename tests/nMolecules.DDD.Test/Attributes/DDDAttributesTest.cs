using System;
using System.Linq;
using System.Reflection;
using Xunit;

[assembly: global::NMolecules.DDD.BoundedContext(Id = "Banking", Name = "Banking", Description = "Core banking domain.", DependsOnContextIds = new[] { "SharedKernel" })]
[module: global::NMolecules.DDD.Module(Id = "Accounts", Name = "Accounts", BoundedContextId = "Banking", Description = "Account management module.")]

namespace NMolecules.DDD
{
    /// <summary>
    /// Sample value object used to verify that <see cref="ValueObjectAttribute"/>
    /// can be applied to structs.
    /// </summary>
    [ValueObject]
    public struct Iban
    {
    }

    /// <summary>
    /// Sample value object used to verify that <see cref="ValueObjectAttribute"/>
    /// can be applied to enums.
    /// </summary>
    [ValueObject]
    public enum Currency
    {
        Eur
    }

    /// <summary>
    /// Sample value object used to verify that <see cref="ValueObjectAttribute"/>
    /// can be applied to reference types as well.
    /// </summary>
    [ValueObject]
    public class Money
    {
    }

    /// <summary>
    /// Sample entity used by the attribute tests to validate entity discovery
    /// and <see cref="IdentityAttribute"/> usage on both fields and properties.
    /// </summary>
    [Entity]
    public class BankAccount
    {
        [Identity]
        private readonly Iban iban = default;

        [Identity]
        public Iban SecondaryId => iban;
    }

    /// <summary>
    /// Sample aggregate root that proves aggregate roots remain discoverable
    /// through the entity inheritance hierarchy used by the DDD attributes.
    /// </summary>
    [AggregateRoot]
    public class AccountAggregate
    {
        [Identity]
        public Iban Id => default;
    }

    /// <summary>
    /// Sample repository contract used to validate repository attribute targets.
    /// </summary>
    [Repository]
    public interface IAccountRepository
    {
    }

    /// <summary>
    /// Sample repository implementation used to validate repository attribute
    /// support for concrete classes.
    /// </summary>
    [Repository]
    public class AccountRepository : IAccountRepository
    {
    }

    /// <summary>
    /// Sample factory contract used to validate factory attribute targets.
    /// </summary>
    [Factory]
    public interface IAccountFactory
    {
    }

    /// <summary>
    /// Sample factory implementation used to validate factory attribute
    /// support for concrete classes.
    /// </summary>
    [Factory]
    public class AccountFactory : IAccountFactory
    {
    }

    /// <summary>
    /// Sample legacy service contract used to keep backward-compatible
    /// <see cref="ServiceAttribute"/> behavior covered.
    /// </summary>
    [Service]
    public interface IPricingService
    {
    }

    /// <summary>
    /// Sample legacy service implementation used by the attribute tests.
    /// </summary>
    [Service]
    public class PricingService : IPricingService
    {
    }

    /// <summary>
    /// Sample domain service contract used to validate the dedicated
    /// <see cref="DomainServiceAttribute"/>.
    /// </summary>
    [DomainService]
    public interface IExchangeRates
    {
    }

    /// <summary>
    /// Sample domain service implementation used by the attribute tests.
    /// </summary>
    [DomainService]
    public class ExchangeRates : IExchangeRates
    {
    }

    /// <summary>
    /// Sample application service contract used to validate the dedicated
    /// <see cref="ApplicationServiceAttribute"/>.
    /// </summary>
    [ApplicationService]
    public interface IMoneyTransferUseCase
    {
    }

    /// <summary>
    /// Sample application service implementation used by the attribute tests.
    /// </summary>
    [ApplicationService]
    public class TransferMoney : IMoneyTransferUseCase
    {
    }

    /// <summary>
    /// Sample composed repository marker used to verify that
    /// <see cref="AllowRepositoryCompositionAttribute"/> can be discovered
    /// together with <see cref="RepositoryAttribute"/>.
    /// </summary>
    [Repository]
    [AllowRepositoryComposition]
    public interface IComposedRepository
    {
    }

    /// <summary>
    /// Verifies the public DDD attribute surface, its declared attribute targets,
    /// and the most important metadata contracts exposed by the attribute model.
    /// </summary>
    public class DDDAttributesTest
    {
        /// <summary>
        /// Provides the expected <see cref="AttributeTargets"/> mask for every
        /// public DDD attribute shipped by the assembly.
        /// </summary>
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(AllowRepositoryCompositionAttribute), AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter },
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

        /// <summary>
        /// Ensures that each attribute advertises the exact target set that the
        /// analyzer and documentation rely on.
        /// </summary>
        [Theory]
        [MemberData(nameof(AttributeTargetsData))]
        public void DeclaresExpectedAttributeUsage(Type attributeType, AttributeTargets expectedTargets)
        {
            var usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

            Assert.NotNull(usage);
            Assert.Equal(expectedTargets, usage!.ValidOn);
        }

        /// <summary>
        /// Guards the exported DDD attribute inventory so that accidental additions,
        /// removals, or renames show up as an explicit contract change.
        /// </summary>
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
                nameof(AllowRepositoryCompositionAttribute),
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

        /// <summary>
        /// Verifies that the value object marker supports all supported CLR shape
        /// categories: class, struct, and enum.
        /// </summary>
        [Fact]
        public void ValueObjectAttributeCanMarkClassStructAndEnum()
        {
            Assert.True(typeof(Iban).IsDefined(typeof(ValueObjectAttribute), false));
            Assert.True(typeof(Currency).IsDefined(typeof(ValueObjectAttribute), false));
            Assert.True(typeof(Money).IsDefined(typeof(ValueObjectAttribute), false));
        }

        /// <summary>
        /// Verifies that entity and aggregate root markers remain discoverable and
        /// that aggregate roots still participate in the entity attribute hierarchy.
        /// </summary>
        [Fact]
        public void EntityAndAggregateRootAttributesAreDiscoverable()
        {
            Assert.True(typeof(BankAccount).IsDefined(typeof(EntityAttribute), false));
            Assert.True(typeof(AccountAggregate).IsDefined(typeof(AggregateRootAttribute), false));
            Assert.IsType<AggregateRootAttribute>(typeof(AccountAggregate).GetCustomAttribute<EntityAttribute>());
        }

        /// <summary>
        /// Verifies that identities can be declared on both backing fields and
        /// exposed properties, which is required for common modeling patterns.
        /// </summary>
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

        /// <summary>
        /// Verifies repository and factory markers on interfaces and concrete
        /// implementations, including the repository composition opt-in marker.
        /// </summary>
        [Fact]
        public void RepositoryAndFactoryAttributesSupportClassesAndInterfaces()
        {
            Assert.True(typeof(IAccountRepository).IsDefined(typeof(RepositoryAttribute), false));
            Assert.True(typeof(AccountRepository).IsDefined(typeof(RepositoryAttribute), false));
            Assert.True(typeof(IComposedRepository).IsDefined(typeof(AllowRepositoryCompositionAttribute), false));
            Assert.True(typeof(IAccountFactory).IsDefined(typeof(FactoryAttribute), false));
            Assert.True(typeof(AccountFactory).IsDefined(typeof(FactoryAttribute), false));
        }

        /// <summary>
        /// Verifies that legacy service, domain service, and application service
        /// markers all remain discoverable on both interfaces and classes.
        /// </summary>
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

        /// <summary>
        /// Verifies that bounded context metadata is available at assembly level
        /// and module metadata is available from the CLR module manifest.
        /// </summary>
        [Fact]
        public void AssemblyAndModuleAttributesAreDiscoverable()
        {
            var assembly = typeof(DDDAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.NotNull(assembly.GetCustomAttribute<BoundedContextAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(ModuleAttribute), false).SingleOrDefault());
        }

        /// <summary>
        /// Verifies constructor defaults and the richer metadata contract of
        /// <see cref="BoundedContextAttribute"/> including jmolecules-style values.
        /// </summary>
        [Fact]
        public void BoundedContextAttributeExposesJmoleculesStyleMetadata()
        {
            var attribute = new BoundedContextAttribute();
            var constructorAttribute = new BoundedContextAttribute("Payments");
            var assemblyAttribute = typeof(DDDAttributesTest).Assembly.GetCustomAttribute<BoundedContextAttribute>();

            Assert.Equal(string.Empty, attribute.Id);
            Assert.Equal(string.Empty, attribute.Name);
            Assert.Equal(string.Empty, attribute.Value);
            Assert.Equal(string.Empty, attribute.Description);
            Assert.Empty(attribute.DependsOnContextIds);
            Assert.Equal("Payments", constructorAttribute.Name);
            Assert.Equal("Payments", constructorAttribute.Value);

            Assert.NotNull(assemblyAttribute);
            Assert.Equal("Banking", assemblyAttribute!.Id);
            Assert.Equal("Banking", assemblyAttribute.Name);
            Assert.Equal(string.Empty, assemblyAttribute.Value);
            Assert.Equal("Core banking domain.", assemblyAttribute.Description);
            Assert.Equal(new[] { "SharedKernel" }, assemblyAttribute.DependsOnContextIds);
        }

        /// <summary>
        /// Verifies constructor defaults and module-level metadata of
        /// <see cref="ModuleAttribute"/>, including bounded-context linkage.
        /// </summary>
        [Fact]
        public void ModuleAttributeExposesJmoleculesStyleMetadata()
        {
            var attribute = new ModuleAttribute();
            var constructorAttribute = new ModuleAttribute("Payments");
            var assembly = typeof(DDDAttributesTest).Assembly;
            var module = assembly.ManifestModule;
            var moduleAttribute = module.GetCustomAttributes(typeof(ModuleAttribute), false).Cast<ModuleAttribute>().Single();

            Assert.Equal(string.Empty, attribute.Id);
            Assert.Equal(string.Empty, attribute.Name);
            Assert.Equal(string.Empty, attribute.Value);
            Assert.Equal(string.Empty, attribute.BoundedContextId);
            Assert.Equal(string.Empty, attribute.Description);
            Assert.Equal("Payments", constructorAttribute.Name);
            Assert.Equal("Payments", constructorAttribute.Value);

            Assert.Equal("Accounts", moduleAttribute.Id);
            Assert.Equal("Accounts", moduleAttribute.Name);
            Assert.Equal(string.Empty, moduleAttribute.Value);
            Assert.Equal("Banking", moduleAttribute.BoundedContextId);
            Assert.Equal("Account management module.", moduleAttribute.Description);
        }
    }
}
