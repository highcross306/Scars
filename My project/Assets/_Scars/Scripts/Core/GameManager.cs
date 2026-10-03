using Scars.Hypothesis;
using UnityEngine;

namespace Scars.Core
{
    // 게임 진행: 단계(PhaseData)를 차례로 넘긴다. 추리에 실패해도 게임오버 없이 다음 단계로 간다
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        void Awake() { }

        public void NewGame() { }

        public void GoTo(string phaseId) { }

        public void ConfirmHypothesis(HypothesisResult result) { }

        public void ReachEnding(string endingId) { }
    }
}
