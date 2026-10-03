using System.Collections.Generic;

namespace Scars.Clues
{
    public class ClueInventory
    {
        readonly HashSet<string> _ids = new HashSet<string>();

        public int Count => _ids.Count;

        public IEnumerable<string> All => _ids;

        // 새로 얻었으면 true, 이미 있거나 빈 id면 false
        public bool Add(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return _ids.Add(id);
        }

        public bool Has(string id) { return !string.IsNullOrEmpty(id) && _ids.Contains(id); }
    }
}
