using Scars.Controls;
using UnityEngine;

namespace Scars.UI
{
    // 사건 일지·가설 (Tab). 열고 닫기는 ScreenStack이 부른다 (InputRouter 칸에 넣는다)
    public class NotebookView : MonoBehaviour, IScreen
    {
        public bool CanClose => true;

        public string PauseReason => "journal";

        public void Open() { gameObject.SetActive(true); }

        public void Close() { gameObject.SetActive(false); }
    }
}
