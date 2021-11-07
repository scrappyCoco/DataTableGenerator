using System.IO;
using System.Linq;
using System.Text;
using Coding4fun.DataTools.Analyzers;
using Coding4fun.DataTools.Analyzers.Template;
using NUnit.Framework;

namespace SourceGeneratorTest
{
    [TestFixture]
    public class TemplateBuilderTest
    {
        [Test]
        public void Test()
        {
            string templateXml = @"
<!-- @formatter:off
     --><root><!--
     --><table><!--
     -->{
<!-- -->  new TableBuilder&lt;<className />&gt;();
<!-- -->}
<!-- --></table><!--
     --></root><!--
-->".Trim();

            string expectedText = @"
{
  new TableBuilder<User>();
}
{
  new TableBuilder<Job>();
}
".Trim();
            
            XmlTemplateParser xmlTemplateParser = new();
            CodeTemplate? codeTemplate = xmlTemplateParser.ParseXml(new StringReader(templateXml));
            
            // StringBuilder stringBuilder = new();
            // codeTemplate.BuildCode(stringBuilder, new DataBuilderRootResolver());
            // {
            //     if (resolver.Name == "table")
            //     {
            //         return new object[]
            //         {
            //             new TableDescription("User"),
            //             new TableDescription("Job")
            //         };
            //     }
            //
            //     if (template.Name == "className")
            //     {
            //         var tableDescription = (TableDescription)context.Last();
            //         return new object[] { tableDescription.ClassName };
            //     }
            //
            //     return new object[] { template.Name ?? template.Text };
            // });
            
            // Assert.AreEqual(expectedText, stringBuilder.ToString().Trim());
        }
    }
}