using System;
using System.Linq;
using System.Reflection;
using Xunit;

[assembly: global::NMolecules.Architecture.Mvvm.ViewModel]
[module: global::NMolecules.Architecture.Mvvm.View]

namespace NMolecules.Architecture.Mvvm.Test
{
    [Model]
    public class Account
    {
    }

    [ViewModel]
    public interface IAccountViewModel
    {
    }

    [View]
    public struct AccountPage
    {
    }

    public class MvvmAttributesTest
    {
        public static TheoryData<Type, AttributeTargets> AttributeTargetsData => new()
        {
            { typeof(ModelAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(ViewAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct },
            { typeof(ViewModelAttribute), AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct }
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
            var attributeNames = typeof(ModelAttribute).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(ModelAttribute).Namespace)
                .Where(type => type.Name.EndsWith("Attribute", StringComparison.Ordinal))
                .Where(type => typeof(Attribute).IsAssignableFrom(type))
                .Select(type => type.Name)
                .OrderBy(name => name)
                .ToArray();

            Assert.Equal(new[]
            {
                nameof(ModelAttribute),
                nameof(ViewAttribute),
                nameof(ViewModelAttribute)
            }, attributeNames);
        }

        [Fact]
        public void MvvmAttributesSupportTypesAssembliesAndModules()
        {
            var assembly = typeof(MvvmAttributesTest).Assembly;
            var module = assembly.ManifestModule;

            Assert.True(typeof(Account).IsDefined(typeof(ModelAttribute), false));
            Assert.True(typeof(IAccountViewModel).IsDefined(typeof(ViewModelAttribute), false));
            Assert.True(typeof(AccountPage).IsDefined(typeof(ViewAttribute), false));
            Assert.NotNull(assembly.GetCustomAttribute<ViewModelAttribute>());
            Assert.NotNull(module.GetCustomAttributes(typeof(ViewAttribute), false).SingleOrDefault());
        }
    }
}
