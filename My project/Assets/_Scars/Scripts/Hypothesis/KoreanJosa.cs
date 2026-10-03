namespace Scars.Hypothesis
{
    // 앞 글자 받침에 따라 은/는, 을/를 등을 고른다
    public static class KoreanJosa
    {
        // 받침 있음 / 없음 / 알 수 없음(한글·숫자가 아닌 글자로 끝남)
        public enum Ending { Batchim, NoBatchim, Unknown }

        // 0~9를 읽었을 때 받침: 영 일 삼 육 칠 팔은 받침 있음
        const string DigitBatchim = "1101001110";

        // 단어 뒤에 조사를 붙인다. 알 수 없으면 "은(는)"처럼 둘 다 쓴다
        public static string Attach(string word, string withBatchim, string withoutBatchim)
        {
            if (string.IsNullOrEmpty(word)) return word;
            switch (GetEnding(word))
            {
                case Ending.Batchim: return word + withBatchim;
                case Ending.NoBatchim: return word + withoutBatchim;
                default: return word + withBatchim + "(" + withoutBatchim + ")";
            }
        }

        // 끝의 괄호·따옴표·공백 등은 건너뛰고 마지막 한글·숫자로 판단한다
        public static Ending GetEnding(string word)
        {
            if (string.IsNullOrEmpty(word)) return Ending.Unknown;
            for (int i = word.Length - 1; i >= 0; i--)
            {
                char c = word[i];
                if (c >= '가' && c <= '힣')
                    return (c - 0xAC00) % 28 != 0 ? Ending.Batchim : Ending.NoBatchim;
                if (c >= '0' && c <= '9')
                    return DigitBatchim[c - '0'] == '1' ? Ending.Batchim : Ending.NoBatchim;
                if (char.IsLetter(c)) return Ending.Unknown;
            }
            return Ending.Unknown;
        }
    }
}
