using Scars.Controls;
using UnityEngine;

namespace Scars.UI
{
    // 인물 일지 (C). 열고 닫기는 ScreenStack이 부른다 (InputRouter 칸에 넣는다)
    public class CharacterJournalView : MonoBehaviour, IScreen
    {
        public bool CanClose => true;

        public string PauseReason => "journal";

        public void Open() { gameObject.SetActive(true); }

        public void Close() { gameObject.SetActive(false); }
    }
}
