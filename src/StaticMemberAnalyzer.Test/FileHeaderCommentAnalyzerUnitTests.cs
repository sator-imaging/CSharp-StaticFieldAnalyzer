using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using System.Threading;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class FileHeaderCommentAnalyzerUnitTests
    {
        public class CustomTest : CSharpAnalyzerTest<FileHeaderCommentAnalyzer, MSTestVerifier>
        {
            public CustomTest() { }
        }

        [TestMethod]
        public async Task TestEmptyFile_ShouldHaveNoDiagnostics()
        {
            var test = @"";
            var customTestRunner = new CustomTest { TestCode = test };
            await customTestRunner.RunAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task TestSingleLineCommentHeader_ShouldHaveNoDiagnostics()
        {
            var test = @"// This is a valid header.
using System;

namespace Test
{
}";
            var customTestRunner = new CustomTest { TestCode = test };
            await customTestRunner.RunAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task TestMultiLineCommentHeader_ShouldHaveNoDiagnostics()
        {
            var test = @"/* This is a valid
 * multi-line header. */
using System;

namespace Test
{
}";
            var customTestRunner = new CustomTest { TestCode = test };
            await customTestRunner.RunAsync(CancellationToken.None);
        }

        [TestMethod]
        public async Task TestValidHeaderWithNestedAndGenericTypes_ShouldHaveNoDiagnostics()
        {
            var test = @"// Valid header
namespace Test
{
    public class Outer<T>
    {
        public class Nested
        {
        }
    }
}";
            var customTestRunner = new CustomTest { TestCode = test };
            await customTestRunner.RunAsync(CancellationToken.None);
        }
    }
}
