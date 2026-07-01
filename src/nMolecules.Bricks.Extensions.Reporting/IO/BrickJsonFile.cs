using System;
using System.IO;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents json file data used by Bricks file IO helpers.
/// </summary>
public static class BrickJsonFile
    {
        /// <summary>
        /// Executes the Load Policy operation for Bricks developer tooling.
        /// </summary>
        public static BrickPolicyDocument LoadPolicy(string path) =>
            BrickPolicyJsonSerializer.Deserialize(File.ReadAllText(RequirePath(path)));

        /// <summary>
        /// Executes the Load Adoption operation for Bricks developer tooling.
        /// </summary>
        public static BrickAdoptionDocument LoadAdoption(string path) =>
            BrickAdoptionJsonSerializer.Deserialize(File.ReadAllText(RequirePath(path)));

        /// <summary>
        /// Executes the Save Adoption operation for Bricks developer tooling.
        /// </summary>
        public static void SaveAdoption(string path, BrickAdoptionDocument document) =>
            Write(path, BrickAdoptionJsonSerializer.Serialize(document));

        /// <summary>
        /// Executes the Save Report operation for Bricks developer tooling.
        /// </summary>
        public static void SaveReport(string path, BrickReportDocument document) =>
            Write(path, BrickReportJsonSerializer.Serialize(document));

        /// <summary>
        /// Executes the Save Role Map operation for Bricks developer tooling.
        /// </summary>
        public static void SaveRoleMap(string path, BrickRoleMapDocument document) =>
            Write(path, BrickExportJsonSerializer.Serialize(document));

        /// <summary>
        /// Executes the Save Dependency Graph operation for Bricks developer tooling.
        /// </summary>
        public static void SaveDependencyGraph(string path, BrickDependencyGraphDocument document) =>
            Write(path, BrickExportJsonSerializer.Serialize(document));

        /// <summary>
        /// Executes the Save Resolution Trace operation for Bricks developer tooling.
        /// </summary>
        public static void SaveResolutionTrace(string path, BrickResolutionTraceDocument document) =>
            Write(path, BrickExportJsonSerializer.Serialize(document));

        private static void Write(string path, string json)
        {
            var targetPath = RequirePath(path);
            EnsureParentDirectory(targetPath);
            File.WriteAllText(targetPath, json);
        }

        private static string RequirePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return path;
        }

        private static void EnsureParentDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
