using System;
using System.Linq;
using System.Reflection;
using NMolecules.Architecture.Cqrs;
using Xunit;

namespace NMolecules.Architecture.Cqrs.Test
{
    [Command(Name = "TransferMoney", Namespace = "Banking.Payments")]
    public interface ITransferMoney
    {
    }

    [Command]
    public struct TransferMoney : ITransferMoney
    {
    }

    [QueryModel]
    public class AccountBalanceReadModel
    {
    }

    public class TransferMoneyHandlers
    {
        [CommandHandler(Name = "TransferMoney", Namespace = "Banking.Payments")]
        public TransferMoneyHandlers()
        {
        }

        [CommandDispatcher(Dispatches = "Banking.Payments.TransferMoney")]
        public void Dispatch(TransferMoney command)
        {
        }

        [CommandHandler]
        public void Handle(TransferMoney command)
        {
        }
    }

    public class CqrsAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(CommandAttribute), AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(CommandDispatcherAttribute), AttributeTargets.Method },
            { typeof(CommandHandlerAttribute), AttributeTargets.Method | AttributeTargets.Constructor },
            { typeof(QueryModelAttribute), AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
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
            var attributeNames = typeof(CommandAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(CommandAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(CommandAttribute),
                nameof(CommandDispatcherAttribute),
                nameof(CommandHandlerAttribute),
                nameof(QueryModelAttribute)
            }, attributeNames);
        }

        [Fact]
        public void SupportsCommandsQueryModelsHandlersAndDispatchers()
        {
            var constructor = typeof(TransferMoneyHandlers).GetConstructors().Single();
            var dispatchMethod = typeof(TransferMoneyHandlers).GetMethod(nameof(TransferMoneyHandlers.Dispatch));
            var handleMethod = typeof(TransferMoneyHandlers).GetMethod(nameof(TransferMoneyHandlers.Handle));

            Assert.True(typeof(ITransferMoney).IsDefined(typeof(CommandAttribute), false));
            Assert.True(typeof(TransferMoney).IsDefined(typeof(CommandAttribute), false));
            Assert.True(typeof(AccountBalanceReadModel).IsDefined(typeof(QueryModelAttribute), false));
            Assert.NotNull(constructor.GetCustomAttribute<CommandHandlerAttribute>());
            Assert.NotNull(dispatchMethod!.GetCustomAttribute<CommandDispatcherAttribute>());
            Assert.NotNull(handleMethod!.GetCustomAttribute<CommandHandlerAttribute>());
        }

        [Fact]
        public void CQRSMetadataDefaultsToEmptyStrings()
        {
            var command = new CommandAttribute();
            var dispatcher = new CommandDispatcherAttribute();
            var handler = new CommandHandlerAttribute();

            Assert.Equal(string.Empty, command.Name);
            Assert.Equal(string.Empty, command.Namespace);
            Assert.Equal(string.Empty, dispatcher.Dispatches);
            Assert.Equal(string.Empty, handler.Name);
            Assert.Equal(string.Empty, handler.Namespace);
        }
    }
}
