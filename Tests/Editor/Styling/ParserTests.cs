using NUnit.Framework;
using ReactUnity.Styling.Parsers;

namespace ReactUnity.Editor.Tests
{
    [TestFixture]
    public class ParserTests
    {
        [TestCase("", ',', new[] { "" })]
        [TestCase("0", ',', new[] { "0" })]
        [TestCase("hello world bye", ',', new[] { "hello world bye" })]
        [TestCase("hello world   bye", ' ', new[] { "hello", "world", "bye" })]
        [TestCase("hello , world ,   bye ", ',', new[] { "hello", "world", "bye" })]
        [TestCase("slidein 3s steps( 5, end ) infinite ", ' ', new[] { "slidein", "3s", "steps( 5, end )", "infinite" })]
        [TestCase("slidein 3s steps( 5, end ) infinite ,  hello something(a,b) ", ',', new[] { "slidein 3s steps( 5, end ) infinite", "hello something(a,b)" })]
        public void Split(string input, char separator, string[] expected)
        {
            Assert.AreEqual(expected, ParserHelpers.Split(input, separator));
        }


        [TestCase("", null, null)]
        [TestCase(" hello ", null, null)]
        [TestCase(" hello() ", "hello", new string[0])]
        [TestCase(" rgba(120, 364 , 22.5, 0.8  )  ", "rgba", new[] { "120", "364", "22.5", "0.8" })]
        [TestCase(" rgba(120, 364 , 22.5, 0.8  ))  ", null, null)]
        [TestCase(" var( --textColor )  ", "var", new[] { "--textColor" })]
        [TestCase("linear-gradient(0.25turn, #3f87a6, #ebf8e1, #f69d3c)", "linear-gradient", new[] { "0.25turn", "#3f87a6", "#ebf8e1", "#f69d3c" })]
        [TestCase("url(data:image/png; base64,iRxVB0)", "url", new[] { "data:image/png; base64", "iRxVB0" })]
        public void ParseFunction(string input, string expectedName, string[] expectedArgs)
        {
            var (name, args) = ParserHelpers.ParseFunction(input);
            Assert.AreEqual(expectedName, name);
            Assert.AreEqual(expectedArgs, args);
        }
    }
}
