using System;
using System.Collections.Generic;

namespace Scars.Controls
{
    public interface IScreen
    {
        // false면 우클릭·토글로 닫히지 않는다 (시간 초과 뒤 가설 창: GameFlow.IsForcedHypothesis일 때 false)
        bool CanClose { get; }
        void Open();
        void Close();
    }

    // 열린 화면(확대 조사, 일지, 팝업)을 쌓아 두고 우클릭 때 맨 위부터 닫는다
    public class ScreenStack
    {
        readonly List<IScreen> _screens = new List<IScreen>();

        public event Action<IScreen> Opened;
        public event Action<IScreen> Closed;

        // 맨 위 화면. 없으면 null
        public IScreen Top => _screens.Count > 0 ? _screens[_screens.Count - 1] : null;

        public int Count => _screens.Count;

        public bool IsEmpty => _screens.Count == 0;

        public bool Contains(IScreen screen) { return screen != null && _screens.Contains(screen); }

        // 맨 위에 연다. 이미 열려 있으면 false
        public bool Push(IScreen screen)
        {
            if (screen == null || _screens.Contains(screen)) return false;
            _screens.Add(screen);
            screen.Open();
            Opened?.Invoke(screen);
            return true;
        }

        // 우클릭: 맨 위 화면을 닫는다. 닫았으면 true (맨 위가 닫을 수 없는 화면이면 false)
        public bool Back() { return Close(Top); }

        // 쌓인 위치와 상관없이 그 화면만 닫는다 (Tab·C로 다시 눌러 닫을 때). CanClose가 false면 그대로
        public bool Close(IScreen screen)
        {
            if (!Contains(screen) || !screen.CanClose) return false;
            Remove(screen);
            return true;
        }

        // 열려 있으면 닫고, 아니면 연다. 바뀌었으면 true
        public bool Toggle(IScreen screen)
        {
            return Contains(screen) ? Close(screen) : Push(screen);
        }

        // 모두 닫는다 (맨 위부터). 단계가 바뀔 때 GameFlow가 부르며, 닫을 수 없는 화면도 닫는다
        public void Clear()
        {
            while (_screens.Count > 0) Remove(Top);
        }

        void Remove(IScreen screen)
        {
            _screens.Remove(screen);
            screen.Close();
            Closed?.Invoke(screen);
        }
    }
}
