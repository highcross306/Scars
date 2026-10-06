using System.Collections.Generic;
using Scars.Data;

namespace Scars.Quiz
{
    // 퍼즐 하나를 푸는 동안의 입력 상태. 제출하면 맞은 칸 수를 돌려준다
    public class QuizSession
    {
        string[] _solution = new string[0];
        HashSet<string> _options = new HashSet<string>();
        string[] _answers = new string[0];

        public int SlotCount => _solution.Length;

        public void Begin(QuizData quiz)
        {
            if (quiz == null) Begin(null, null);
            else Begin(quiz.Solution, quiz.Options);
        }

        // options가 비어 있으면 아무 값이나 받는다
        public void Begin(IReadOnlyList<string> solution, IReadOnlyList<string> options)
        {
            _solution = Copy(solution);
            _options = options == null ? new HashSet<string>() : new HashSet<string>(options);
            _answers = new string[_solution.Length];
        }

        // 칸 번호가 범위 밖이거나 보기에 없는 값이면 넣지 않고 false
        public bool SetAnswer(int slot, string answer)
        {
            if (!IsValidSlot(slot) || string.IsNullOrEmpty(answer)) return false;
            if (_options.Count > 0 && !_options.Contains(answer)) return false;
            _answers[slot] = answer;
            return true;
        }

        public void ClearAnswer(int slot)
        {
            if (IsValidSlot(slot)) _answers[slot] = null;
        }

        // 칸이 비었거나 범위 밖이면 null
        public string GetAnswer(int slot) { return IsValidSlot(slot) ? _answers[slot] : null; }

        // 정답과 같은 칸의 개수. 빈 칸은 틀린 것으로 센다
        public int Submit()
        {
            int correct = 0;
            for (int i = 0; i < _solution.Length; i++)
                if (_answers[i] != null && _answers[i] == _solution[i]) correct++;
            return correct;
        }

        bool IsValidSlot(int slot) { return slot >= 0 && slot < _answers.Length; }

        // List를 거치지 않고 배열로 한 번만 복사한다
        static string[] Copy(IReadOnlyList<string> source)
        {
            if (source == null) return new string[0];
            var copy = new string[source.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = source[i];
            return copy;
        }
    }
}
