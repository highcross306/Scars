using System;
using System.Collections.Generic;

namespace Scars.Clues
{
    // 가진 카드 목록. 얻은 순서를 유지하고, 업그레이드하면 같은 자리에서 바뀐다
    public class ClueInventory
    {
        readonly List<string> _order = new List<string>();
        readonly HashSet<string> _ids = new HashSet<string>();
        readonly HashSet<string> _upgradedAway = new HashSet<string>();

        // 새 카드를 얻었을 때 (id)
        public event Action<string> Added;

        // 카드가 업그레이드됐을 때 (원래 id, 새 id)
        public event Action<string, string> Upgraded;

        public int Count => _order.Count;

        public IEnumerable<string> All => _order;

        // 새로 얻었으면 true, 이미 있거나 빈 id거나 이미 업그레이드해서 없어진 카드면 false
        public bool Add(string id)
        {
            if (string.IsNullOrEmpty(id) || _upgradedAway.Contains(id) || !_ids.Add(id)) return false;
            _order.Add(id);
            Added?.Invoke(id);
            return true;
        }

        // 기본 카드를 업그레이드 카드로 바꾼다. 기본 카드가 없거나 새 카드를 이미 가졌으면 false
        public bool Upgrade(string fromId, string toId)
        {
            if (!Has(fromId) || string.IsNullOrEmpty(toId) || _ids.Contains(toId)) return false;
            _order[_order.IndexOf(fromId)] = toId;
            _ids.Remove(fromId);
            _ids.Add(toId);
            _upgradedAway.Add(fromId);
            Upgraded?.Invoke(fromId, toId);
            return true;
        }

        public bool Has(string id) { return !string.IsNullOrEmpty(id) && _ids.Contains(id); }
    }
}
