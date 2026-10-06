using Scars.Core;
using UnityEngine;

namespace Scars.Data
{
    public enum PhaseKind { Dialogue, Investigation, Hypothesis, Interrogation }

    // 게임 흐름의 한 단계 (의뢰 → 인물 대면 → 현장 조사 → 가설 조립 → 심문)
    [CreateAssetMenu(menuName = "Scars/Phase")]
    public class PhaseData : DataAsset
    {
        public string title;
        public PhaseKind kind;
        public string sceneName;
        public string startDialogueId;
        [Tooltip("단계에 들어올 때 (기본 지급 카드 등)")]
        public GameEffect onEnter = new GameEffect();
        [Tooltip("다음 단계. 조사 단계에서는 서재 문을 눌러 스스로 나갔을 때")]
        public string nextPhaseId;

        [Header("조사 제한 시간")]
        [Tooltip("초 단위 (7분 = 420). 0이면 제한 없음. 방향 조작이 가능해지는 순간부터 흐른다")]
        public float timeLimitSeconds;
        [Tooltip("시간 초과 때 나오는 대화 (타임아웃 연출)")]
        public string timeoutDialogueId;
        [Tooltip("시간 초과 대화가 끝나면 바로 들어갈 단계 (가설 조립). 이후 단서는 더 모을 수 없다")]
        public string timeoutPhaseId;

        public bool HasTimeLimit => timeLimitSeconds > 0f;
    }
}
