using System;
using System.Collections.Generic;
using Scars.Investigation;

namespace Scars.Controls
{
    public interface IScreen
    {
        // false면 우클릭·토글로 닫히지 않는다 (시간 초과 뒤 가설 창: GameFlow.IsForcedHypothesis일 때 false)
        bool CanClose { get; }
        // 열려 있는 동안 조사 시간을 멈추는 이유 (예: "journal", "clueDetail", "hypothesis", "pauseMenu", "judgment"). 멈추지 않으면 null
        string PauseReason { get; }
        void Open();
        void Close();
    }

    // 열린 화면(확대 조사, 일지, 팝업)을 쌓아 두고 우클릭 때 맨 위부터 닫는다
    // 화면의 PauseReason으로 조사 타이머를 멈추고 다시 흘린다 (View가 직접 Pause/Resume하지 않는다)
    public class ScreenStack
    {
        // 타이머의 다른 이유(GameFlow의 "dialogue")와 겹치지 않게 붙인다
        const string PausePrefix = "screen:";

        readonly List<IScreen> _screens = new List<IScreen>();
        InvestigationTimer _timer;

        public event Action<IScreen> Opened;
        public event Action<IScreen> Closed;

        // 맨 위 화면. 없으면 null
        public IScreen Top => _screens.Count > 0 ? _screens[_screens.Count - 1] : null;

        public int Count => _screens.Count;

        public bool IsEmpty => _screens.Count == 0;

        public bool Contains(IScreen screen) { return screen != null && _screens.Contains(screen); }

        // 멈출 타이머. GameFlow가 단계마다 넣는다(제한 시간 없는 단계는 null). 바꾸면 열린 화면의 멈춤도 옮겨 간다
        public InvestigationTimer Timer
        {
            get => _timer;
            set
            {
                if (_timer == value) return;
                foreach (var reason in OpenReasons()) _timer?.Resume(reason);
                _timer = value;
                foreach (var reason in OpenReasons()) _timer?.Pause(reason);
            }
        }

        // 맨 위에 연다. 이미 열려 있으면 false
        public bool Push(IScreen screen)
        {
            if (screen == null || _screens.Contains(screen)) return false;
            _screens.Add(screen);
            var reason = ReasonOf(screen);
            if (reason != null) _timer?.Pause(reason);
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
            // 같은 이유로 열린 화면이 남아 있으면 계속 멈춘다
            var reason = ReasonOf(screen);
            if (reason != null && !HasReason(reason)) _timer?.Resume(reason);
            screen.Close();
            Closed?.Invoke(screen);
        }

        bool HasReason(string reason)
        {
            foreach (var screen in _screens)
                if (ReasonOf(screen) == reason) return true;
            return false;
        }

        IEnumerable<string> OpenReasons()
        {
            foreach (var screen in _screens)
            {
                var reason = ReasonOf(screen);
                if (reason != null) yield return reason;
            }
        }

        static string ReasonOf(IScreen screen)
        {
            return string.IsNullOrEmpty(screen.PauseReason) ? null : PausePrefix + screen.PauseReason;
        }
    }
}
