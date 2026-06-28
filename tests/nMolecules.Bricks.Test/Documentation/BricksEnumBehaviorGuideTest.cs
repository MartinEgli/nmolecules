using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksEnumBehaviorGuideTest
    {
        [Fact]
        public void EnumBehaviorGuideDocumentsEveryPublicEnumValue()
        {
            var guide = File.ReadAllText(FindGuidePath());
            var enumTypes = typeof(BrickElement)
                .Assembly
                .GetTypes()
                .Where(type => type.IsPublic && type.Namespace == "NMolecules.Bricks" && type.IsEnum)
                .OrderBy(type => type.Name)
                .ToArray();

            Assert.NotEmpty(enumTypes);
            Assert.Contains("Analyzer behavior labels", guide);

            foreach (var enumType in enumTypes)
            {
                Assert.Contains($"`{enumType.Name}", guide);

                foreach (var valueName in Enum.GetNames(enumType))
                {
                    Assert.Contains($"`{enumType.Name}.{valueName}`", guide);
                }
            }
        }

        private static string FindGuidePath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "src", "nMolecules.Bricks", "docs", "enum-behavior-guide.md");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find nMolecules.Bricks enum behavior guide.");
        }
    }
}
