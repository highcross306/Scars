using Scars.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scars.Core
{
    // 테스트용 화면: 진짜 UI가 생기기 전까지 GameFlow를 화면에서 직접 돌려 본다 (IMGUI)
    // GameManager와 같은 오브젝트에 두면 씬이 바뀌어도 따라온다. 진짜 View가 생기면 지운다
    public class FlowDebugPanel : MonoBehaviour
    {
        const float ReferenceHeight = 1080f;
        const float Width = 560f;
        const float Height = 400f;

        Vector2 _scroll;

        void OnGUI()
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.Flow == null) return;
            var flow = manager.Flow;

            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.box.fontSize = 24;
            GUI.skin.label.wordWrap = true;

            GUILayout.BeginArea(new Rect(20f, 20f, Width, Height), GUI.skin.box);
            _scroll = GUILayout.BeginScrollView(_scroll);

            var phase = flow.CurrentPhase;
            GUILayout.Label($"씬: {SceneManager.GetActiveScene().name}");
            GUILayout.Label(phase == null ? "단계: 없음" : $"단계: {phase.Id} ({phase.kind})");
            var clues = flow.State.Clues;
            GUILayout.Label($"보유 단서 {clues.Count}개" + (clues.Count > 0 ? ": " + string.Join(", ", clues.All) : ""));
            GUILayout.Space(10f);

            if (flow.IsEnded)
            {
                GUILayout.Label($"엔딩: {flow.EndingId}");
                if (GUILayout.Button("새 게임")) manager.NewGame();
            }
            else if (phase == null)
            {
                GUILayout.Label("시작 화면의 플레이 버튼으로 시작");
            }
            else if (flow.Dialogue != null)
            {
                DrawDialogue(flow);
            }
            else
            {
                DrawPhase(manager, flow, phase);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        static void DrawDialogue(GameFlow flow)
        {
            var node = flow.Dialogue.Current;
            if (node == null) return;
            GUILayout.Label(string.IsNullOrEmpty(node.speakerId) ? "(내레이션)" : $"[{node.speakerId}]");
            GUILayout.Label(node.text);
            GUILayout.Space(10f);

            var choices = flow.Dialogue.AvailableChoices();
            if (choices.Count == 0)
            {
                if (GUILayout.Button("다음")) flow.Dialogue.Advance();
                return;
            }
            for (int i = 0; i < choices.Count; i++)
                if (GUILayout.Button(choices[i].text)) flow.Dialogue.Choose(i);
        }

        static void DrawPhase(GameManager manager, GameFlow flow, PhaseData phase)
        {
            switch (phase.kind)
            {
                case PhaseKind.Investigation:
                    var timer = flow.Timer;
                    if (timer != null)
                    {
                        int s = timer.DisplaySeconds;
                        GUILayout.Label($"남은 시간: {s / 60}:{s % 60:00} ({timer.Stage}{(timer.IsPaused ? ", 멈춤" : "")})");
                        if (!timer.IsStarted && GUILayout.Button("조사 시작 (타이머)")) timer.Start();
                    }
                    if (GUILayout.Button("서재 문으로 나가기")) manager.CompletePhase();
                    break;

                case PhaseKind.Hypothesis:
                    GUILayout.Label(flow.IsForcedHypothesis ? "시간 초과: 가설 조립 창에서 제출 (닫기 불가)" : "가설 조립 창에서 제출 (Tab으로 다시 열기)");
                    break;

                default:
                    if (GUILayout.Button("단계 마치기")) manager.CompletePhase();
                    break;
            }
        }
    }
}
