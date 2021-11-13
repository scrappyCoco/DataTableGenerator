using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Coding4fun.DataTools.Test.Infrastructure
{
    public class TestBase
    {
        private const string TestData = nameof(TestData);
        private readonly string _pathToTestData;

        protected TestBase()
        {
            // Assumes that test assembly name is equals to the root namespace.
            string assemblyFullName = GetType().Assembly.GetName().Name!;
            string testNamespace = GetType().Namespace!;
            List<string> pathComponents = new() { TestData };
            pathComponents.AddRange(testNamespace
                .Replace(assemblyFullName, "")
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
            );
            _pathToTestData = Path.Combine(pathComponents.ToArray());
        }

        protected async Task<string> LoadAsync(string fileName = "Source.cs", [CallerMemberName] string? methodName = null)
        {
            string pathToFile = Path.Combine(_pathToTestData, methodName!, fileName);
            string code = await File.ReadAllTextAsync(pathToFile);
            return code.Trim();
        }
    }
}