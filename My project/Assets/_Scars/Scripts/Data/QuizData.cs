using System.Collections.Generic;
using UnityEngine;

namespace Scars.Data
{
    // 방탈출식 퍼즐 한 개. 칸 N개에 값을 하나씩 넣어 정답과 맞춘다 (자물쇠 다이얼, 순서 맞추기, 판별 질문 등)
    [CreateAssetMenu(menuName = "Scars/Quiz")]
    public class QuizData : DataAsset
    {
        [TextArea] public string question;

        [Tooltip("칸마다 들어가야 할 값. 칸 수 = 길이")]
        [SerializeField] string[] solution = new string[0];

        [Tooltip("칸에 넣을 수 있는 값(모든 칸 공통). 비워 두면 아무 값이나 입력 가능")]
        [SerializeField] string[] options = new string[0];

        public IReadOnlyList<string> Solution => solution;

        public IReadOnlyList<string> Options => options;
    }
}
