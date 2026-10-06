using Scars.Core;
using Scars.Data;
using Scars.Quiz;

namespace Scars.Investigation
{
    // 조사 대상 하나: 획득 단계(기본 카드) → 인터랙션 및 심층 검식(도구 → 조작 순서 → 판별) → 업그레이드 카드
    // 도구 사용·조작 진행은 확대 화면을 나가면 사라지고, 판별 결과(업그레이드 카드 획득 / 기본 카드 유지)는 GameState에 남는다
    public class InspectionSession
    {
        InspectableData _target;
        GameState _state;
        bool _itemUsed;
        int _step;
        string _keptBaseFlag;
        readonly QuizSession _quiz = new QuizSession();

        public void Begin(InspectableData target, GameState state)
        {
            _target = target;
            _state = state;
            _itemUsed = false;
            _step = 0;
            // 화면이 매 프레임 IsBaseCardKept를 읽어도 문자열을 새로 만들지 않도록 한 번만 만든다
            _keptBaseFlag = target != null ? "inspect-kept-base:" + target.Id : null;
            _quiz.Begin(target != null ? target.judgment : null);
        }

        // 획득 단계: 기본 카드를 새로 얻었으면 true
        public bool Collect() { return IsReady && _state.Clues.Add(_target.grantClueId); }

        public bool HasDeepInspection => IsReady && !string.IsNullOrEmpty(_target.upgradeClueId);

        // 업그레이드 카드를 획득한 상태
        public bool IsUpgraded => HasDeepInspection && _state.Clues.Has(_target.upgradeClueId);

        // 판별에서 겉보기 해석을 골라 기본 카드 유지로 확정된 상태 (판별은 한 번)
        public bool IsBaseCardKept => HasDeepInspection && _state.HasFlag(KeptBaseFlag);

        // 기본 카드가 있는 대상은 기본 카드를 먼저 가져야 한다
        public bool CanDeepInspect()
        {
            if (!HasDeepInspection || IsUpgraded || IsBaseCardKept) return false;
            return string.IsNullOrEmpty(_target.grantClueId) || _state.Clues.Has(_target.grantClueId);
        }

        public bool NeedsItem => IsReady && !string.IsNullOrEmpty(_target.requiredItemId);

        public bool ItemReady => !NeedsItem || _itemUsed;

        // 맞는 도구를 가지고 있으면 true
        public bool UseItem(string itemId)
        {
            if (!CanDeepInspect() || !NeedsItem || _itemUsed) return false;
            if (itemId != _target.requiredItemId || !_state.Clues.Has(itemId)) return false;
            _itemUsed = true;
            FinishIfNoJudgment();
            return true;
        }

        public int StepCount => IsReady && _target.steps != null ? _target.steps.Length : 0;

        public int StepProgress => _step;

        public bool StepsDone => IsReady && _step >= StepCount;

        // 다음 순서의 대상이면 true. 다른 대상이면 진행은 그대로
        public bool ClickStep(string stepId)
        {
            if (!CanDeepInspect() || !ItemReady || StepsDone) return false;
            if (string.IsNullOrEmpty(stepId) || stepId != _target.steps[_step]) return false;
            _step++;
            FinishIfNoJudgment();
            return true;
        }

        // 판별: 심층 진실을 고르면 업그레이드 카드 획득(true), 겉보기 해석을 고르면 기본 카드 유지(false). 판별은 한 번
        // 빈 값·보기에 없는 값은 판별로 치지 않는다 (false, 기본 카드 유지로 확정되지 않음)
        public bool Judge(string answer)
        {
            if (!CanDeepInspect() || !ItemReady || !StepsDone || _target.judgment == null) return false;
            _quiz.ClearAnswer(0);
            if (!_quiz.SetAnswer(0, answer)) return false;
            if (_quiz.Submit() == _quiz.SlotCount)
            {
                Finish();
                return true;
            }
            _state.SetFlag(KeptBaseFlag);
            return false;
        }

        bool IsReady => _target != null && _state != null;

        string KeptBaseFlag => _keptBaseFlag;

        void FinishIfNoJudgment()
        {
            if (_target.judgment == null && ItemReady && StepsDone) Finish();
        }

        void Finish()
        {
            if (!string.IsNullOrEmpty(_target.grantClueId) && _state.Clues.Has(_target.grantClueId))
                _state.Clues.Upgrade(_target.grantClueId, _target.upgradeClueId);
            else
                _state.Clues.Add(_target.upgradeClueId);
        }
    }
}
