using NUnit.Framework;
using Scars.Hypothesis;

namespace Scars.Tests
{
    public class KoreanJosaTests
    {
        [TestCase("고양이", "는")]
        [TestCase("집사", "는")]
        [TestCase("강아지", "는")]
        [TestCase("곰", "은")]
        [TestCase("밀실", "은")]
        [TestCase("열쇠", "는")]
        public void EunNeun_ByBatchim(string word, string expected)
        {
            Assert.AreEqual(word + expected, KoreanJosa.Attach(word, "은", "는"));
        }

        [TestCase("통로", "를")]
        [TestCase("조작", "을")]
        [TestCase("가로채기", "를")]
        [TestCase("함정", "을")]
        public void EulReul_ByBatchim(string word, string expected)
        {
            Assert.AreEqual(word + expected, KoreanJosa.Attach(word, "을", "를"));
        }

        [TestCase("3", "을")]
        [TestCase("2", "를")]
        [TestCase("10", "을")]
        public void Digits_UseKoreanReading(string word, string expected)
        {
            Assert.AreEqual(word + expected, KoreanJosa.Attach(word, "을", "를"));
        }

        [Test]
        public void TrailingBracketOrQuote_IsSkipped()
        {
            Assert.AreEqual("상자(나무)를", KoreanJosa.Attach("상자(나무)", "을", "를"));
            Assert.AreEqual("'금고'를", KoreanJosa.Attach("'금고'", "을", "를"));
        }

        [Test]
        public void UnknownEnding_WritesBoth()
        {
            Assert.AreEqual("Key은(는)", KoreanJosa.Attach("Key", "은", "는"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Empty_ReturnsAsIs(string word)
        {
            Assert.AreEqual(word, KoreanJosa.Attach(word, "은", "는"));
        }
    }
}
