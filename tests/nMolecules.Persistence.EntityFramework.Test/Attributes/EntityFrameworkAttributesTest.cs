using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NMolecules.Persistence.EntityFramework.Test
{
    [EfDbContext("BankingWriteContext", BoundedContextId = "Banking", ModuleId = "Accounts")]
    public class BankingWriteContext
    {
    }

    [EfOwnedValueObject(Owner = "BankAccount")]
    public readonly struct Money
    {
    }

    [EfEntityType("bank_accounts", Schema = "billing")]
    public class BankAccountRecord
    {
        [EfConcurrencyToken(Strategy = "rowversion")]
        public byte[] Version { get; set; } = Array.Empty<byte>();

        [EfValueConverter(typeof(MoneyConverter))]
        [EfBackingField("_balance")]
        public Money Balance
        {
            get => _balance;
            set => _balance = value;
        }

        [EfIgnore(Reason = "Calculated from booking table")]
        public decimal CurrentBalance { get; set; }

        private Money _balance;
    }

    public class MoneyConverter
    {
    }

    public class EntityFrameworkAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(EfDbContextAttribute), AttributeTargets.Class },
            { typeof(EfEntityTypeAttribute), AttributeTargets.Class },
            { typeof(EfOwnedValueObjectAttribute), AttributeTargets.Class | AttributeTargets.Struct },
            { typeof(EfBackingFieldAttribute), AttributeTargets.Property },
            { typeof(EfConcurrencyTokenAttribute), AttributeTargets.Property | AttributeTargets.Field },
            { typeof(EfValueConverterAttribute), AttributeTargets.Property | AttributeTargets.Field },
            { typeof(EfIgnoreAttribute), AttributeTargets.Property | AttributeTargets.Field }
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
        public void ExposesExpectedEntityFrameworkAttributeSet()
        {
            var attributeNames = typeof(EfDbContextAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(EfDbContextAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(EfBackingFieldAttribute),
                nameof(EfConcurrencyTokenAttribute),
                nameof(EfDbContextAttribute),
                nameof(EfEntityTypeAttribute),
                nameof(EfIgnoreAttribute),
                nameof(EfOwnedValueObjectAttribute),
                nameof(EfValueConverterAttribute)
            }, attributeNames);
        }

        [Fact]
        public void ExposesExpectedDbContextMetadata()
        {
            var attribute = new EfDbContextAttribute();
            var named = new EfDbContextAttribute("BankingReadContext");
            var discovered = typeof(BankingWriteContext).GetCustomAttribute<EfDbContextAttribute>();

            Assert.Equal(string.Empty, attribute.Name);
            Assert.Equal(string.Empty, attribute.BoundedContextId);
            Assert.Equal(string.Empty, attribute.ModuleId);
            Assert.Equal("BankingReadContext", named.Name);

            Assert.NotNull(discovered);
            Assert.Equal("BankingWriteContext", discovered!.Name);
            Assert.Equal("Banking", discovered.BoundedContextId);
            Assert.Equal("Accounts", discovered.ModuleId);
        }

        [Fact]
        public void ExposesExpectedEntityTypeAndMemberMetadata()
        {
            var entityType = typeof(BankAccountRecord).GetCustomAttribute<EfEntityTypeAttribute>();
            var concurrencyToken = typeof(BankAccountRecord).GetProperty(nameof(BankAccountRecord.Version))!
                .GetCustomAttribute<EfConcurrencyTokenAttribute>();
            var backingField = typeof(BankAccountRecord).GetProperty(nameof(BankAccountRecord.Balance))!
                .GetCustomAttribute<EfBackingFieldAttribute>();
            var converter = typeof(BankAccountRecord).GetProperty(nameof(BankAccountRecord.Balance))!
                .GetCustomAttribute<EfValueConverterAttribute>();
            var ignored = typeof(BankAccountRecord).GetProperty(nameof(BankAccountRecord.CurrentBalance))!
                .GetCustomAttribute<EfIgnoreAttribute>();

            Assert.NotNull(entityType);
            Assert.Equal("bank_accounts", entityType!.Table);
            Assert.Equal("billing", entityType.Schema);
            Assert.False(entityType.Keyless);

            Assert.NotNull(concurrencyToken);
            Assert.Equal("rowversion", concurrencyToken!.Strategy);

            Assert.NotNull(backingField);
            Assert.Equal("_balance", backingField!.FieldName);

            Assert.NotNull(converter);
            Assert.Equal(typeof(MoneyConverter), converter!.ConverterType);

            Assert.NotNull(ignored);
            Assert.Equal("Calculated from booking table", ignored!.Reason);
        }

        [Fact]
        public void ExposesExpectedOwnedValueObjectMetadata()
        {
            var attribute = new EfOwnedValueObjectAttribute();
            var discovered = typeof(Money).GetCustomAttribute<EfOwnedValueObjectAttribute>();

            Assert.Equal(string.Empty, attribute.Owner);
            Assert.NotNull(discovered);
            Assert.Equal("BankAccount", discovered!.Owner);
        }
    }
}
