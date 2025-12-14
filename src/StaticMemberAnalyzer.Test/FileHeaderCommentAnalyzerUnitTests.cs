using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;

// The custom test runner is used because the default roslyn verifier runs a
// `#pragma warning disable` test automatically, but this analyzer is not
// affected by `#pragma`.
using VerifyCS = StaticMemberAnalyzer.Test.FileHeaderCommentAnalyzerVerifier;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class FileHeaderCommentAnalyzerUnitTests
    {
        [TestMethod]
        public async Task TestNoHeaderComment()
        {
            var test = @"{|#0:using System;|}

namespace Test
{
    class MyClass { }
}
";
            var expected = VerifyCS.Diagnostic(FileHeaderCommentAnalyzer.RuleId_MissingFileHeaderComment).WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task TestWithNestedAndGenericTypes()
        {
            var test = @"{|#0:using System;|}

namespace Test
{
    class MyClass<T>
    {
        class NestedClass { }
    }
}
";
            var expected = VerifyCS.Diagnostic(FileHeaderCommentAnalyzer.RuleId_MissingFileHeaderComment).WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task TestWithSingleLineComment()
        {
            var test = @"// this is a comment
using System;

namespace Test
{
    class MyClass { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestWithMultiLineComment()
        {
            var test = @"/* this is a multi-line comment */
using System;

namespace Test
{
    class MyClass { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestStartsWithBlankLine()
        {
            var test = @"
{|#0:using|} System;

namespace Test
{
    class MyClass { }
}
";
            var expected = VerifyCS.Diagnostic(FileHeaderCommentAnalyzer.RuleId_MissingFileHeaderComment).WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task TestWithPragmaWarningDisable()
        {
            var test = @"{|#0:#pragma warning disable CS8618|}

namespace Test
{
    class MyClass { }
}
";
            var expected = VerifyCS.Diagnostic(FileHeaderCommentAnalyzer.RuleId_MissingFileHeaderComment).WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
