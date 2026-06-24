using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Selects the output shape for AI-assisted violation comments.
    /// </summary>
    public enum BrickAiCommentFormat
    {
        /// <summary>Emit human-readable Markdown comments.</summary>
        Markdown = 0,
        /// <summary>Emit machine-readable JSON comments.</summary>
        Json = 1,
        /// <summary>Emit both Markdown and JSON comments.</summary>
        Both = 2
    }
}
