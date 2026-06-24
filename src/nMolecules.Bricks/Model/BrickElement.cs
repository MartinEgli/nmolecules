using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
public sealed class BrickElement
    {
        public BrickElement(
            BrickElementId id,
            BrickElementKind kind,
            string displayName,
            string assemblyName = null,
            string namespaceName = null,
            string fullName = null,
            BrickElementOrigin origin = BrickElementOrigin.Unknown,
            BrickElementSource source = BrickElementSource.Unknown)
        {
            Id = id;
            Kind = kind;
            DisplayName = displayName ?? string.Empty;
            AssemblyName = assemblyName;
            NamespaceName = namespaceName;
            FullName = fullName;
            Origin = origin;
            Source = source;
        }

        public BrickElementId Id { get; }
        public BrickElementKind Kind { get; }
        public string DisplayName { get; }
        public string AssemblyName { get; }
        public string NamespaceName { get; }
        public string FullName { get; }
        public BrickElementOrigin Origin { get; }
        public BrickElementSource Source { get; }
    }
}
