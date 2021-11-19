using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

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
        /// Relative path to test data, depend on namespace of executing test class.
        /// </summary>
        private readonly string _pathToTestData;

        protected TestBase()
        {
            // Assumes that test assembly name is equals to the root namespace.
            string assemblyFullName = GetType().Assembly.GetName().Name;
            string testNamespace = GetType().Namespace;
            List<string> pathComponents = new List<string> { TestData };
            pathComponents.AddRange(testNamespace
                .Replace(assemblyFullName, "")
                .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)
            );
            _pathToTestData = Path.Combine(pathComponents.ToArray());
        }

        protected Task<string> LoadAsync(string fileName = "Source.cs", [CallerMemberName] string methodName = null)
        {
            string pathToFile = Path.Combine(_pathToTestData, methodName, fileName);
            string code = File.ReadAllText(pathToFile);
            return Task.FromResult(code.Trim().Replace("\r", ""));
        }

        protected void AssertSourceCode(string expected, string actual)
        {
            expected = _newLineRegex.Replace(expected, "\n");
            actual = _newLineRegex.Replace(actual, "\n");
            Assert.AreEqual(expected, actual);
        }
    }
}