using Coding4fun.DataTools.Analyzers.StringUtil;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test
{
    public class StringExtensionsTest
    {
        [Test]
        public void ChangeCase()
        {
            string expectedText = "bi-test-1";
            string kebabCase = "_BITest___1_".ChangeCase(CaseRules.ToLowerCase, "-")!;
            Assert.AreEqual(expectedText, kebabCase);
        }

        [Test]
        public void EmptyText()
        {
            string? expectedText = null;
            string kebabCase = "".ChangeCase(CaseRules.ToLowerCase, "-")!;
            Assert.AreEqual(expectedText, kebabCase);
        }
    }
}