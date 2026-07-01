using System;
using System.IO;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksPackageBoundaryGuideTest
    {
        [Fact]
        public void PackageBoundaryGuideClassifiesEverySourceArea()
        {
            var sourceRoot = FindSourceRoot();
            var guide = File.ReadAllText(Path.Combine(sourceRoot, "docs", "package-boundaries.md"));
            var areas = Directory.GetDirectories(sourceRoot)
                .Select(Path.GetFileName)
                .Where(name => name != null)
                .Where(name => name != "bin" && name != "obj" && name != "docs")
                .OrderBy(name => name)
                .ToArray();

            Assert.Contains("`NMolecules.Bricks`", guide);
            Assert.Contains("`NMolecules.Bricks.Analyzers`", guide);
            Assert.Contains("`NMolecules.Bricks.Extensions.Assessment`", guide);
            Assert.Contains("`NMolecules.Bricks.Extensions.Core`", guide);
            Assert.Contains("`NMolecules.Bricks.Extensions.Reporting`", guide);
            Assert.Contains("`NMolecules.Bricks.Extensions.Runtime`", guide);
            Assert.Contains("`NMolecules.Bricks.Ai`", guide);

            foreach (var area in areas)
            {
                Assert.Contains($"`{area}`", guide);
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
