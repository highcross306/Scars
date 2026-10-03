namespace Scars.Hypothesis
{
    // [행위자] 은/는 [수단] 을/를 통해 [동기] 을/를 꾀했다
    public enum HypothesisSlot { Actor, Means, Motive }

    public enum HypothesisKind { Consistent, Contradiction }

    public class HypothesisResult
    {
        public HypothesisKind Kind;
        public string ResultKey;
        public string Sentence;
        public string Warning;
    }
}
