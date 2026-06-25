using System;
using System.IO;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents json file data used by Bricks file IO helpers.
/// </summary>
public static class BrickJsonFile
    {
        public static BrickPolicyDocument LoadPolicy(string path) =>
            BrickPolicyJsonSerializer.Deserialize(File.ReadAllText(RequirePath(path)));

        public static BrickAdoptionDocument LoadAdoption(string path) =>
            BrickAdoptionJsonSerializer.Deserialize(File.ReadAllText(RequirePath(path)));

        public static void SaveAdoption(string path, BrickAdoptionDocument document) =>
            Write(path, BrickAdoptionJsonSerializer.Serialize(document));

        public static void SaveReport(string path, BrickReportDocument document) =>
            Write(path, BrickReportJsonSerializer.Serialize(document));

        public static void SaveRoleMap(string path, BrickRoleMapDocument document) =>
            Write(path, BrickExportJsonSerializer.Serialize(document));

        public static void SaveDependencyGraph(string path, BrickDependencyGraphDocument document) =>
            Write(path, BrickExportJsonSerializer.Serialize(document));

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
