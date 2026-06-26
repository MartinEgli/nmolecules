using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksApiCatalogTest
    {
        [Fact]
        public void ApiCatalogDocumentsNamespaceAndEveryPublicBricksType()
        {
            var catalog = File.ReadAllText(FindCatalogPath());
            var publicTypes = typeof(BrickElement)
                .Assembly
                .GetTypes()
                .Where(type => type.IsPublic && type.Namespace == "NMolecules.Bricks")
                .OrderBy(type => type.Name)
                .ToArray();

            Assert.Contains("| `NMolecules.Bricks` |", catalog);
            Assert.NotEmpty(publicTypes);

            foreach (var type in publicTypes)
            {
                Assert.Contains($"| `{type.Name}` |", catalog);
            }
        }

        private static string FindCatalogPath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "src", "nMolecules.Bricks", "docs", "api-catalog.md");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not find nMolecules.Bricks API catalog.");
        }
    }
}
