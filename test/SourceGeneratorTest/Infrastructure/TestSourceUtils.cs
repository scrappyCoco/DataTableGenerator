using System.IO;
using System.Threading.Tasks;

namespace Coding4fun.DataTools.SourceGeneratorTest.Infrastructure
{
    public static class TestSourceUtils
    {
        public static async Task<string> Loadsync(params string[] pathParts)
        {
            string pathToFile = Path.Combine(pathParts);
            string code = await File.ReadAllTextAsync(pathToFile);
            return code.Trim();
        }
    }
}