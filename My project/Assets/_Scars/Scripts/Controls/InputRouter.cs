using Scars.Core;
using UnityEngine;

namespace Scars.Controls
{
    // 좌클릭: 조사·선택 / 우클릭: 뒤로(ScreenStack.Back) / Tab: 사건 일지·가설 / C: 인물 일지 / ESC: 메뉴
    // 화면 버튼도 같은 명령을 부른다. 규칙은 InputCommands에 있고, 여기서는 키만 읽는다
    public class InputRouter : MonoBehaviour
    {
        // IScreen을 구현한 View를 넣는다 (Unity는 인터페이스 칸을 직렬화하지 못해 MonoBehaviour로 받는다)
        [SerializeField] MonoBehaviour caseJournal;
        [SerializeField] MonoBehaviour characterJournal;
        [SerializeField] MonoBehaviour pauseMenu;

        InputCommands _commands;

        void Start()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            _commands = new InputCommands(manager.Flow.Screens)
            {
                CaseJournal = caseJournal as IScreen,
                CharacterJournal = characterJournal as IScreen,
                PauseMenu = pauseMenu as IScreen,
            };
        }

        void Update()
        {
            if (_commands == null) return;
            if (Input.GetMouseButtonDown(1)) Back();
            if (Input.GetKeyDown(KeyCode.Tab)) ToggleCaseJournal();
            if (Input.GetKeyDown(KeyCode.C)) ToggleCharacterJournal();
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu();
        }

        public void ToggleCaseJournal() { _commands?.ToggleCaseJournal(); }

        public void ToggleCharacterJournal() { _commands?.ToggleCharacterJournal(); }

        public void TogglePauseMenu() { _commands?.TogglePauseMenu(); }

        public void Back() { _commands?.Back(); }
    }
}
