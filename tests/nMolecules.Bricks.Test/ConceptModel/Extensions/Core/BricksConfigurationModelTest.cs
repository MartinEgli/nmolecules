using System;
using System.Linq;
using Xunit;

namespace NMolecules.Bricks.Test
{
    public class BricksConfigurationModelTest
    {
        [Fact]
        public void ConfigurationSourceRecordsKindAndPrecedence()
        {
            var source = new BrickConfigurationSource(
                "editorconfig",
                BrickConfigurationSourceKind.AnalyzerConfig,
                "Repository analyzer config.");

            Assert.Equal("editorconfig", source.Id);
            Assert.Equal(BrickConfigurationSourceKind.AnalyzerConfig, source.Kind);
            Assert.Equal("Repository analyzer config.", source.Description);
            Assert.Equal(5, source.Precedence);
        }

        [Fact]
        public void ConfigurationSourceNormalizesNullText()
        {
            var source = new BrickConfigurationSource(null, BrickConfigurationSourceKind.Generated, null);

            Assert.Equal(string.Empty, source.Id);
            Assert.Equal(string.Empty, source.Description);
            Assert.Equal(1, source.Precedence);
        }

        [Fact]
        public void ConfigurationEntryRecordsKeyValueAndSource()
        {
            var source = Source("policy", BrickConfigurationSourceKind.PolicyFile);
            var entry = new BrickConfigurationEntry("defaultDecision", "Deny", source);

            Assert.Equal("defaultDecision", entry.Key);
            Assert.Equal("Deny", entry.Value);
            Assert.Equal(source, entry.Source);
        }

        [Fact]
        public void ConfigurationEntryNormalizesNullValues()
        {
            var entry = new BrickConfigurationEntry(null, null, null);

            Assert.Equal(string.Empty, entry.Key);
            Assert.Equal(string.Empty, entry.Value);
            Assert.Equal(BrickConfigurationSource.Generated, entry.Source);
        }

        [Fact]
        public void ResolveChoosesHighestPrecedenceEntryPerKey()
        {
            var generated = Entry("enforcement", "Document", BrickConfigurationSourceKind.Generated);
            var package = Entry("enforcement", "Analyze", BrickConfigurationSourceKind.Package);
            var policy = Entry("enforcement", "Enforce", BrickConfigurationSourceKind.PolicyFile);
            var msbuild = Entry("enforcement", "Disabled", BrickConfigurationSourceKind.MSBuild);
            var analyzerConfig = Entry("enforcement", "Analyze", BrickConfigurationSourceKind.AnalyzerConfig);
            var annotation = Entry("enforcement", "Enforce", BrickConfigurationSourceKind.SourceAnnotation);

            var resolution = BrickConfigurationResolver.Resolve(new[] { generated, package, policy, msbuild, analyzerConfig, annotation });

            var entry = resolution.Entries.Single();
            Assert.Equal(annotation, entry.EffectiveEntry);
            Assert.Equal(new[] { generated, package, policy, msbuild, analyzerConfig }, entry.ShadowedEntries.ToArray());
        }

        [Fact]
        public void ResolveUsesLastEntryWhenPrecedenceTies()
        {
            var first = Entry("defaultDecision", "Allow", BrickConfigurationSourceKind.PolicyFile);
            var second = Entry("defaultDecision", "Deny", BrickConfigurationSourceKind.PolicyFile);

            var entry = BrickConfigurationResolver.Resolve(new[] { first, second }).Entries.Single();

            Assert.Equal(second, entry.EffectiveEntry);
            Assert.Equal(new[] { first }, entry.ShadowedEntries.ToArray());
        }

        [Fact]
        public void ResolveSortsKeysAndKeepsIndependentEntries()
        {
            var zeta = Entry("zeta", "Z", BrickConfigurationSourceKind.Generated);
            var alpha = Entry("alpha", "A", BrickConfigurationSourceKind.Generated);

            var resolution = BrickConfigurationResolver.Resolve(new[] { zeta, alpha });

            Assert.Equal(new[] { "alpha", "zeta" }, resolution.Entries.Select(entry => entry.Key).ToArray());
            Assert.Empty(resolution.Entries.SelectMany(entry => entry.ShadowedEntries));
            Assert.True(resolution.HasEntries);
        }

        [Fact]
        public void ResolveIgnoresEmptyKeysAndNormalizesNullInput()
        {
            var empty = new BrickConfigurationEntry(null, "ignored", Source("policy", BrickConfigurationSourceKind.PolicyFile));

            Assert.Empty(BrickConfigurationResolver.Resolve(new[] { empty }).Entries);
            Assert.Empty(BrickConfigurationResolver.Resolve(null).Entries);
            Assert.False(BrickConfigurationResolver.Resolve(null).HasEntries);
        }

        [Fact]
        public void ResolveIgnoresNullEntries()
        {
            var valid = Entry("enforcement", "Analyze", BrickConfigurationSourceKind.PolicyFile);

            var resolution = BrickConfigurationResolver.Resolve(new BrickConfigurationEntry[] { null, valid });

            Assert.Equal(valid, resolution.Entries.Single().EffectiveEntry);
        }

        [Fact]
        public void ConfigurationResolutionNormalizesNullEntries()
        {
            var resolution = new BrickConfigurationResolution(null);

            Assert.Empty(resolution.Entries);
            Assert.False(resolution.HasEntries);
        }

        [Fact]
        public void ConfigurationResolvedEntryNormalizesShadowedEntries()
        {
            var effective = Entry("enforcement", "Analyze", BrickConfigurationSourceKind.PolicyFile);
            var resolved = new BrickConfigurationResolvedEntry(null, effective, null);

            Assert.Equal(string.Empty, resolved.Key);
            Assert.Equal(effective, resolved.EffectiveEntry);
            Assert.Empty(resolved.ShadowedEntries);
        }

        [Fact]
        public void ConfigurationResolvedEntryRequiresEffectiveEntry()
        {
            Assert.Throws<ArgumentNullException>(() => new BrickConfigurationResolvedEntry("key", null, null));
        }

        private static BrickConfigurationEntry Entry(string key, string value, BrickConfigurationSourceKind kind) =>
            new BrickConfigurationEntry(key, value, Source(kind.ToString(), kind));

        private static BrickConfigurationSource Source(string id, BrickConfigurationSourceKind kind) =>
            new BrickConfigurationSource(id, kind, id);
    }
}
