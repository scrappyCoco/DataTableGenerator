using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    public class TestBase
    {
        private readonly Regex _newLineRegex = new Regex("(\r|\n)+");
        /// <summary>
        /// Folder name with test data.
        /// </summary>
        private const string TestData = nameof(TestData);

        /// <summary>
        /// Suffix of derived class.
        /// </summary>
        private const string Tests = nameof(Tests);
        
        /// <summary>
        /// Relative path to test data, depend on namespace of executing test class.
        /// </summary>
        private readonly string _pathToTestData;

        protected TestBase()
        {
            // Assumes that test assembly name is equals to the root namespace.
            string assemblyFullName = GetType().Assembly.GetName().Name!;
            string testNamespace = GetType().Namespace!;
            
            List<string> pathComponents = new List<string> { TestData };
            
            pathComponents.AddRange(testNamespace
                .Replace(assemblyFullName, "")
                .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)
            );
            
            pathComponents.Add(GetType().Name.Replace(Tests, ""));
            
            _pathToTestData = Path.Combine(pathComponents.ToArray());
        }

        protected Task<string> LoadAsync(string fileName = "Source.cs", [CallerMemberName] string? methodName = null)
        {
            string pathToFile = Path.Combine(_pathToTestData, methodName!, fileName);
            string code = File.ReadAllText(pathToFile);
            return Task.FromResult(code.Replace("\r", ""));
        }

        protected async Task<(string, string)[]> LoadXmlFiles([CallerMemberName] string methodName = null!)
        {
            List<(string, string)> files = new List<(string, string)>();
            string pathToDirectory = Path.Combine(_pathToTestData, methodName);
            string[] xmlFiles = Directory.GetFiles(pathToDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string xmlFilePath in xmlFiles)
            {
                string xmlContent = await File.ReadAllTextAsync(xmlFilePath);
                string fileName = Path.GetFileName(xmlFilePath);
                files.Add((fileName, xmlContent));
            }

            return files.ToArray();
        }
    }
}