using System;
using System.Collections.Generic;
using System.Linq;

namespace NMolecules.Bricks
{
    /// <summary>
    /// Provides the policy and elements available to role assignment providers.
    /// </summary>
    public sealed class BrickModelContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrickModelContext"/> class.
        /// </summary>
        /// <param name="policy">The active policy.</param>
        /// <param name="elements">The elements in the model.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
        public BrickModelContext(BrickPolicy policy, IEnumerable<BrickElement> elements)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Elements = (elements ?? Enumerable.Empty<BrickElement>()).ToArray();
        }

        /// <summary>
        /// Gets the active policy.
        /// </summary>
        public BrickPolicy Policy { get; }

        /// <summary>
        /// Gets the elements in the model.
        /// </summary>
        public IReadOnlyList<BrickElement> Elements { get; }
    }
}
