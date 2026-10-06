using System;
using Scars.Data;
using Scars.Dialogue;
using Scars.Hypothesis;
using Scars.Investigation;

namespace Scars.Core
{
    // 게임 진행 로직 (GameManager가 들고 있고, 매 프레임 Tick을 부른다)
    // 단계(PhaseData)를 넘기고, 단계의 대화·조사 제한 시간·가설 확정·엔딩을 묶는다. 추리에 실패해도 게임오버 없이 이어진다
    public class GameFlow
    {
        // 가설 확정 때 남기는 플래그. 심문 대화의 선택지 조건(Condition.requiredFlags)으로 쓴다
        public const string HypothesisFlagPrefix = "hypothesis:";
        // 시간 초과가 났을 때 남는 플래그
        public const string TimeoutFlag = "investigation-timeout";
        const string DialoguePause = "dialogue";

        readonly GameDatabase _database;
        bool _inTimeoutDialogue;

        public GameFlow(GameDatabase database)
        {
            _database = database;
        }

        // 단계가 바뀌었을 때 (씬 전환은 GameManager가 sceneName을 보고 한다)
        public event Action<PhaseData> PhaseChanged;
        // 새 대화가 시작됐을 때 (DialogueView가 Dialogue를 그린다)
        public event Action<DialogueRunner> DialogueStarted;
        // 조사 시간이 끝났을 때 (타임아웃 연출 시작 신호)
        public event Action TimedOut;
        public event Action<string> EndingReached;

        public GameState State { get; private set; } = new GameState();

        public PhaseData CurrentPhase { get; private set; }

        // 지금 진행 중인 대화. 없으면 null
        public DialogueRunner Dialogue { get; private set; }

        // 제한 시간이 있는 단계에서만 있다. 시작(Start)은 방향 조작이 가능해지는 순간 화면 쪽이 부른다
        public InvestigationTimer Timer { get; private set; }

        public bool IsTimedOut { get; private set; }

        // 단서를 모을 수 있는 상태. 시간 초과 뒤에는 false
        public bool CanInvestigate => !IsEnded && CurrentPhase != null && CurrentPhase.kind == PhaseKind.Investigation && !IsTimedOut;

        public string EndingId { get; private set; }

        public bool IsEnded => !string.IsNullOrEmpty(EndingId);

        // 상태를 새로 만들고 첫 단계로 간다
        public bool NewGame(string firstPhaseId)
        {
            State = new GameState();
            CurrentPhase = null;
            Dialogue = null;
            Timer = null;
            IsTimedOut = false;
            EndingId = null;
            _inTimeoutDialogue = false;
            return GoTo(firstPhaseId);
        }

        // 없는 단계거나 엔딩 뒤면 false
        public bool GoTo(string phaseId)
        {
            if (IsEnded || _database == null) return false;
            var phase = _database.Find<PhaseData>(phaseId);
            if (phase == null) return false;

            Timer?.Stop();
            Timer = null;
            Dialogue = null;
            IsTimedOut = false;
            _inTimeoutDialogue = false;
            CurrentPhase = phase;

            if (phase.HasTimeLimit)
            {
                Timer = new InvestigationTimer(phase.timeLimitSeconds);
                Timer.Expired += OnTimerExpired;
            }

            phase.onEnter?.Apply(State);
            PhaseChanged?.Invoke(phase);
            if (phase.onEnter != null && phase.onEnter.HasEnding)
            {
                ReachEnding(phase.onEnter.endingId);
                return true;
            }
            StartDialogue(phase.startDialogueId);
            return true;
        }

        // 지금 단계의 다음 단계로 (대화 단계를 마칠 때, 조사 단계에서 서재 문으로 스스로 나갈 때)
        public bool CompletePhase()
        {
            if (CurrentPhase == null || IsTimedOut) return false;
            return GoTo(CurrentPhase.nextPhaseId);
        }

        // 대화 시작. 제한 시간 중이면 대화하는 동안 멈춘다
        public bool StartDialogue(string dialogueId)
        {
            if (IsEnded || _database == null) return false;
            var data = _database.Find<DialogueData>(dialogueId);
            if (data == null) return false;

            var runner = new DialogueRunner();
            runner.EndingReached += ReachEnding;
            Dialogue = runner;
            Timer?.Pause(DialoguePause);
            DialogueStarted?.Invoke(runner);
            runner.Start(data, State);
            if (Dialogue == runner && runner.IsFinished) FinishDialogue();
            return true;
        }

        // 매 프레임. 시간을 흘리고, 끝난 대화를 정리한다
        public void Tick(float deltaTime)
        {
            if (IsEnded) return;
            if (Dialogue != null && Dialogue.IsFinished) FinishDialogue();
            Timer?.Tick(deltaTime);
        }

        // 가설 확정 (정합이든 모순 밀어붙이기든). 결과를 플래그로 남기고 다음 단계(심문)로 간다
        public bool ConfirmHypothesis(HypothesisResult result)
        {
            if (result == null || IsEnded || CurrentPhase == null) return false;
            State.SetFlag(HypothesisFlagPrefix + result.Kind);
            if (!string.IsNullOrEmpty(result.ResultKey)) State.SetFlag(HypothesisFlagPrefix + result.ResultKey);
            GoTo(CurrentPhase.nextPhaseId);
            return true;
        }

        // 엔딩 진입. 이후 진행은 멈춘다 (한 번만)
        public void ReachEnding(string endingId)
        {
            if (IsEnded || string.IsNullOrEmpty(endingId)) return;
            EndingId = endingId;
            Timer?.Stop();
            EndingReached?.Invoke(endingId);
        }

        void OnTimerExpired()
        {
            IsTimedOut = true;
            State.SetFlag(TimeoutFlag);
            TimedOut?.Invoke();
            if (IsEnded) return;
            // 타임아웃 대화가 없으면 바로 가설 단계로
            _inTimeoutDialogue = true;
            if (StartDialogue(CurrentPhase.timeoutDialogueId)) return;
            _inTimeoutDialogue = false;
            GoTo(CurrentPhase.timeoutPhaseId);
        }

        void FinishDialogue()
        {
            Dialogue = null;
            Timer?.Resume(DialoguePause);
            if (!_inTimeoutDialogue || IsEnded) return;
            _inTimeoutDialogue = false;
            GoTo(CurrentPhase.timeoutPhaseId);
        }
    }
}
