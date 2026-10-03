using System.Collections.Generic;
using Scars.Clues;

namespace Scars.Core
{
    public class GameState
    {
        readonly HashSet<string> _flags = new HashSet<string>();

        public ClueInventory Clues { get; } = new ClueInventory();

        public void SetFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) _flags.Add(flag);
        }

        public void ClearFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) _flags.Remove(flag);
        }

        public bool HasFlag(string flag) { return !string.IsNullOrEmpty(flag) && _flags.Contains(flag); }
    }
}
