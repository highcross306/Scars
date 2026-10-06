using System;
using System.Collections.Generic;

namespace Scars.Investigation
{
    // 회중시계 단계: 정상 → 경고(테두리 맥동) → 위험(비네팅)
    public enum TimerStage { Normal, Warning, Danger }

    // 조사 제한 시간. 시간은 바깥(Update)이 Tick으로 넣어 준다 (Time.timeScale은 쓰지 않는다)
    // 일지·가설·판별 질문 등이 열린 동안은 이유별로 멈춘다. 겹쳐 열어도 모두 닫혀야 다시 흐른다
    public class InvestigationTimer
    {
        // 단계 경계는 화면에 보이는 초(올림) 기준
        const int WarningAt = 119;    // 01:59
        const int LastMinuteAt = 59;  // 00:59 (사운드)
        const int DangerAt = 29;      // 00:29

        readonly HashSet<string> _pauseReasons = new HashSet<string>();
        float _remaining;
        bool _started;
        bool _stopped;

        public InvestigationTimer(float seconds)
        {
            Duration = Math.Max(0f, seconds);
            _remaining = Duration;
            Stage = StageOf(DisplaySeconds);
            IsLastMinute = DisplaySeconds <= LastMinuteAt;
        }

        public event Action<TimerStage> StageChanged;
        public event Action LastMinuteStarted;
        // 시간 초과. 한 번만 온다
        public event Action Expired;

        public float Duration { get; }

        public float Remaining => _remaining;

        // 화면에 보이는 남은 초 (07:00 → 420, 끝나야 0)
        public int DisplaySeconds => (int)Math.Ceiling(_remaining);

        public TimerStage Stage { get; private set; }

        public bool IsLastMinute { get; private set; }

        public bool IsStarted => _started;

        public bool IsPaused => _pauseReasons.Count > 0;

        public bool IsExpired => _started && _remaining <= 0f;

        // 스스로 나가서 멈춘 상태
        public bool IsStopped => _stopped;

        public bool IsRunning => _started && !_stopped && !IsPaused && !IsExpired;

        // 방향 조작이 가능해지는 순간 부른다. 두 번째부터는 무시
        public void Start() { _started = true; }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || deltaTime <= 0f) return;

            _remaining = Math.Max(0f, _remaining - deltaTime);
            int shown = DisplaySeconds;

            var stage = StageOf(shown);
            if (stage != Stage)
            {
                Stage = stage;
                StageChanged?.Invoke(stage);
            }

            if (!IsLastMinute && shown <= LastMinuteAt)
            {
                IsLastMinute = true;
                LastMinuteStarted?.Invoke();
            }

            if (_remaining <= 0f) Expired?.Invoke();
        }

        // reason 예: "journal", "clueDetail", "hypothesis", "pauseMenu", "dialogue", "judgment"
        public void Pause(string reason)
        {
            if (!string.IsNullOrEmpty(reason)) _pauseReasons.Add(reason);
        }

        public void Resume(string reason)
        {
            if (!string.IsNullOrEmpty(reason)) _pauseReasons.Remove(reason);
        }

        // 서재 문을 클릭해 스스로 나갈 때. 이후 Tick은 무시
        public void Stop() { _stopped = true; }

        static TimerStage StageOf(int shown)
        {
            if (shown <= DangerAt) return TimerStage.Danger;
            if (shown <= WarningAt) return TimerStage.Warning;
            return TimerStage.Normal;
        }
    }
}
