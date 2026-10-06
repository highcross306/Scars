using Scars.Data;
using Scars.Hypothesis;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scars.Core
{
    // 게임 진행: 단계(PhaseData)를 차례로 넘긴다. 추리에 실패해도 게임오버 없이 다음 단계로 간다
    // 진행 로직은 GameFlow에 있고, 여기서는 씬 전환과 매 프레임 Tick만 한다. 씬이 바뀌어도 유지된다
    public class GameManager : MonoBehaviour
    {
        [SerializeField] GameDatabase database;
        [SerializeField] string firstPhaseId;

        public static GameManager Instance { get; private set; }

        // View는 여기서 상태·대화·타이머를 읽고 이벤트를 구독한다
        public GameFlow Flow { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Flow = new GameFlow(database);
            Flow.PhaseChanged += LoadSceneOf;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            Flow?.Tick(Time.deltaTime);
        }

        public void NewGame() { Flow.NewGame(firstPhaseId); }

        public void GoTo(string phaseId) { Flow.GoTo(phaseId); }

        // 대화 단계를 마칠 때, 서재 문으로 스스로 나갈 때
        public void CompletePhase() { Flow.CompletePhase(); }

        public void ConfirmHypothesis(HypothesisResult result) { Flow.ConfirmHypothesis(result); }

        public void ReachEnding(string endingId) { Flow.ReachEnding(endingId); }

        // 단계의 씬이 지금 씬과 다를 때만 불러온다 (서재 조사 → 가설 조립처럼 같은 씬이면 그대로)
        static void LoadSceneOf(PhaseData phase)
        {
            if (string.IsNullOrEmpty(phase.sceneName)) return;
            if (SceneManager.GetActiveScene().name == phase.sceneName) return;
            SceneManager.LoadScene(phase.sceneName);
        }
    }
}
