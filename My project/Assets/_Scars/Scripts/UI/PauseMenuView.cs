using Scars.Controls;
using UnityEngine;

namespace Scars.UI
{
    // ESC 메뉴. 열고 닫기는 ScreenStack이 부른다 (InputRouter 칸에 넣는다)
    public class PauseMenuView : MonoBehaviour, IScreen
    {
        public bool CanClose => true;

        public string PauseReason => "pauseMenu";

        public void Open() { gameObject.SetActive(true); }

        public void Close() { gameObject.SetActive(false); }
    }
}
