using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;
using SatorImaging.StaticMemberAnalyzer.CodeFixes.Providers;
using VerifyCS = StaticMemberAnalyzer.Test.CSharpCodeFixVerifier<
    SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers.EnumAnalyzer,
    SatorImaging.StaticMemberAnalyzer.CodeFixes.Providers.EnumObfuscationCodeFixProvider>;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class EnumObfuscationCodeFixProviderUnitTests
    {
        [TestMethod]
        public async Task TestSimpleEnum()
        {
            var test = @"
namespace Test
{
    public enum {|#0:ETest|} { Value }
}
";

            var fixtest = @"using System.Reflection;

namespace Test
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public enum ETest { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments("ETest");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        [TestMethod]
        public async Task TestGenericClassWithNestedEnum()
        {
            var test = @"
namespace Test
{
    public class CTest<T>
    {
        public enum {|#0:ETest|} { Value }
    }
}
";

            var fixtest = @"using System.Reflection;

namespace Test
{
    public class CTest<T>
    {
        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        public enum ETest { Value }
    }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments("ETest");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        [TestMethod]
        public async Task TestNestedEnum()
        {
            var test = @"
namespace Test
{
    public class CTest
    {
        public enum {|#0:ETest|} { Value }
    }
}
";

            var fixtest = @"using System.Reflection;

namespace Test
{
    public class CTest
    {
        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        public enum ETest { Value }
    }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments("ETest");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
