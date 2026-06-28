using System;
using System.IO;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksLayerPlacementGuideTest
    {
        [Fact]
        public void LayerGuidesKeepPackDefinitionsSeparateFromUseCases()
        {
            var sourceRoot = FindSourceRoot();
            var layer2 = File.ReadAllText(Path.Combine(sourceRoot, "docs", "layer2", "bricks-layer2-building-blocks.md"));
            var layer3Readme = File.ReadAllText(Path.Combine(sourceRoot, "docs", "layer3", "README.md"));

            Assert.Contains("Clean Architecture Pack", layer2);
            Assert.Contains("Hexagonal Architecture Pack", layer2);
            Assert.Contains("## Layer Placement Decision", layer3Readme);
            Assert.Contains("Layer 2", layer3Readme);
            Assert.Contains("Layer 3", layer3Readme);
            Assert.Contains("bricks-layer3-usecases-clean-architecture.md", layer3Readme);
            Assert.Contains("bricks-layer3-usecases-hexagonal.md", layer3Readme);
            Assert.Contains("bricks-layer3-usecases-ddd.md", layer3Readme);
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
