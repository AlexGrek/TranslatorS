using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Translator;
using System.Collections.Generic;

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
            CollectionAssert.AreEqual(lexer.Output, new List<int> { 12, 4, 5, 13 });
        }
    }
}
