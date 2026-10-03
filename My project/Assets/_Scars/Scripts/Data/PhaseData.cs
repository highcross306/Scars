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
        public string nextPhaseId;
    }
}
