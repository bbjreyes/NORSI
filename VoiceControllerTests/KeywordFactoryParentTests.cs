using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoiceController;

namespace VoiceControllerTests
{
    [TestClass]
    public class KeywordFactoryParentTests
    {
        [TestMethod]
        public void ParentsRetrievedNoChildrenTest()
        {
            List<ParentKeyword> parentWords = KeywordFactory.GetParents() as List<ParentKeyword>;

            var expected = "System";
            var actual = parentWords[0];

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void ParentsRetrievedCount()
        {
            List<ParentKeyword> parentWords = KeywordFactory.GetParents() as List<ParentKeyword>;

            var expected = 2;
            var actual = parentWords.Count;

            Assert.AreEqual(expected, actual);
        }
    }
}
