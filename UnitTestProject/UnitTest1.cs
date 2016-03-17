using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SignalTranslatorCore;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DelimiterHell()
        {
            var inp = new StringAsFileBuffer("<= +- > <>> <=>");
            var lexer = new LexAn();
            lexer.Scan(inp);
            CollectionAssert.AreEqual(lexer.Output, new List<int> { 12, 4, 5, 13 , 15, 13, 12, 13});
        }

        [TestMethod]
        public void Identifiers()
        {
            var inp = new StringAsFileBuffer("exe > x program >= 1488;x54");
            var lexer = new LexAn();
            lexer.Scan(inp);
            CollectionAssert.AreEqual(lexer.Output, new List<int> { 500, 13, 501, 300, 11, 400, 3, 502  });
        }
    }
}
