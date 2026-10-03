using UnityEngine;

namespace Scars.Controls
{
    // 좌클릭: 조사·선택 / 우클릭: 뒤로(ScreenStack.Back) / Tab: 사건 일지·가설 / C: 인물 일지
    // 화면 버튼도 같은 명령을 부른다
    public class InputRouter : MonoBehaviour
    {
        void Update() { }

        public void ToggleCaseJournal() { }

        public void ToggleCharacterJournal() { }

        public void Back() { }
    }
}
