namespace Scars.Quiz
{
    public class QuizSession
    {
        public void Begin(string quizId) { }

        public void SetAnswer(int slot, string answer) { }

        public void ClearAnswer(int slot) { }

        public bool Submit() { return false; }
    }
}
