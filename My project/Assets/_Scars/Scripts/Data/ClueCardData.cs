using UnityEngine;

namespace Scars.Data
{
    public enum ClueKind { Clue, Item }

    // 사건 일지에 들어가는 카드. 업그레이드된 카드도 별도 카드로 만든다
    [CreateAssetMenu(menuName = "Scars/Clue Card")]
    public class ClueCardData : DataAsset
    {
        public ClueKind kind;
        public string title;
        [TextArea] public string description;
    }
}
