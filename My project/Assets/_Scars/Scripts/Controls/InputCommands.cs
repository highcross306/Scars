namespace Scars.Controls
{
    // 키·버튼 명령 → ScreenStack (InputRouter가 키를 읽어 여기로 넘긴다)
    // 우클릭: 뒤로 / Tab: 사건 일지·가설 / C: 인물 일지 / ESC: 메뉴
    // ESC 메뉴가 열려 있는 동안 Tab·C는 무시한다 (메뉴 위에 일지가 겹쳐 열리지 않게)
    public class InputCommands
    {
        readonly ScreenStack _screens;

        public InputCommands(ScreenStack screens)
        {
            _screens = screens;
        }

        public IScreen CaseJournal { get; set; }

        public IScreen CharacterJournal { get; set; }

        public IScreen PauseMenu { get; set; }

        public bool IsPauseMenuOpen => _screens != null && _screens.Contains(PauseMenu);

        public bool Back() { return _screens != null && _screens.Back(); }

        public bool ToggleCaseJournal() { return ToggleBelowMenu(CaseJournal); }

        public bool ToggleCharacterJournal() { return ToggleBelowMenu(CharacterJournal); }

        public bool TogglePauseMenu() { return _screens != null && _screens.Toggle(PauseMenu); }

        bool ToggleBelowMenu(IScreen screen)
        {
            if (_screens == null || IsPauseMenuOpen) return false;
            return _screens.Toggle(screen);
        }
    }
}
