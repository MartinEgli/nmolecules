using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksAreaGuideTest
    {
        [Fact]
        public void AreaGuideDocumentsEverySourceArea()
        {
            var sourceRoot = FindSourceRoot();
            var guide = File.ReadAllText(Path.Combine(sourceRoot, "docs", "area-guide.md"));
            var areas = Directory.GetDirectories(sourceRoot)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Where(name => name != "bin" && name != "obj" && name != "docs")
                .OrderBy(name => name)
                .ToArray();

            Assert.Contains("## Namespace", guide);

            foreach (var area in areas)
            {
                Assert.Contains($"## {area}", guide);
            }
        }

        private static string FindSourceRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "src", "nMolecules.Bricks");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find nMolecules.Bricks source root.");
        }
    }
}
