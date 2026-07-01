using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
/// <summary>
/// Represents configuration precedence data used by configuration source precedence and resolved Bricks
/// settings.
/// </summary>
public static class BrickConfigurationPrecedence
    {
        /// <summary>
        /// Executes the Rank operation for Bricks developer tooling.
        /// </summary>
        public static int Rank(BrickConfigurationSourceKind kind)
        {
            switch (kind)
            {
                case BrickConfigurationSourceKind.SourceAnnotation:
                    return 6;
                case BrickConfigurationSourceKind.AnalyzerConfig:
                    return 5;
                case BrickConfigurationSourceKind.MSBuild:
                    return 4;
                case BrickConfigurationSourceKind.PolicyFile:
                    return 3;
                case BrickConfigurationSourceKind.Package:
                    return 2;
                default:
                    return 1;
            }
        }
    }
}
