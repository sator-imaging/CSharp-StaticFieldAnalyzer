﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;
using VerifyCS = StaticMemberAnalyzer.Test.CSharpCodeFixVerifier<
    SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers.EnumAnalyzer,
    SatorImaging.StaticMemberAnalyzer.CodeFixes.Providers.EnumObfuscationCodeFixProvider>;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class EnumObfuscationCodeFixUnitTest
    {
        const string TEST_ENUM_NAME = "ETest";

        string GetExpectedResult(string expectedAttrExpression = "[Obfuscation(Exclude = true, ApplyToMembers = true)]", string? additionalUsings = null)
        {
            // newline required
            string? result = @"
";

            if (additionalUsings != null)
            {
                result += additionalUsings.TrimEnd();
            }

            result += @$"
using System.Reflection;

namespace Test
{{
    {expectedAttrExpression}
    public enum {TEST_ENUM_NAME} {{ Value }}
}}
"
    .TrimStart();  //trim starting newline

            return result;
        }


        /*  tests  ================================================================ */

        //Diagnostic and CodeFix both triggered and checked for

        /* =      namespace      = */

        [TestMethod]
        public async Task BasicTest_NoNamespace()
        {
            var test = @"
namespace Test
{
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult().TrimStart());  // trim start is required due to codefix automatically remove starting newline
        }

        [TestMethod]
        public async Task BasicTest_WithNamespace()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult());
        }


        /* =      attr exists no args      = */

        [TestMethod]
        public async Task BasicTest_AttrExistsNoParentheses()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [Obfuscation]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult());
        }

        [TestMethod]
        public async Task BasicTest_AttrExistsNoArgs()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [Obfuscation()]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult());
        }


        /* =      arg exists      = */

        [TestMethod]
        public async Task BasicTest_AttrExists_ExcludeFalse()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [Obfuscation(Exclude = false)]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult());
        }

        [TestMethod]
        public async Task BasicTest_AttrExists_ApplyToMembersTrue()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [Obfuscation(ApplyToMembers = true)]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult());
        }

        [TestMethod]
        public async Task BasicTest_AttrExists_AllRequiredArgsExist_OrderChanged_BooleanExpression()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [Obfuscation(ApplyToMembers = 1 == 0, Exclude = 0 != 0)]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult());
        }


        /* =      other args      = */

        [TestMethod]
        public async Task BasicTest_AttrExists_OtherArg()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [Obfuscation(StripAfterObfuscation = true)]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult("[Obfuscation(Exclude = true, ApplyToMembers = true, StripAfterObfuscation = true)]"));
        }


        /* =      other attr      = */

        [TestMethod]
        public async Task BasicTest_AttrExists_OtherAttr()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [System.ComponentModel.Category]
    [Obfuscation(StripAfterObfuscation = true)]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult(@"[System.ComponentModel.Category]
    [Obfuscation(Exclude = true, ApplyToMembers = true, StripAfterObfuscation = true)]"));
        }


        /* =      attr namings      = */

        [TestMethod]
        public async Task BasicTest_AttrNaming_Full()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [System.Reflection.ObfuscationAttribute]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult("[System.Reflection.ObfuscationAttribute(Exclude = true, ApplyToMembers = true)]"));
        }

        [TestMethod]
        public async Task BasicTest_AttrNaming_FullNoSuffix()
        {
            var test = @"
using System.Reflection;

namespace Test
{
    [System.Reflection.Obfuscation()]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult("[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]"));
        }

        // NOTE: partial namespace not work...? (C# lang spec?)
        //[TestMethod]
        public async Task BasicTest_AttrNaming_PartialNamespace()
        {
            var test = @"
using System;

namespace Test
{
    [Reflection.ObfuscationAttribute()]
    public enum {|#0:" + TEST_ENUM_NAME + @"|} { Value }
}
";
            var expected = VerifyCS.Diagnostic(EnumAnalyzer.RuleId_EnumObfuscation).WithLocation(0).WithArguments(TEST_ENUM_NAME);
            await VerifyCS.VerifyCodeFixAsync(test, expected,
                GetExpectedResult("[Reflection.ObfuscationAttribute(Exclude = true, ApplyToMembers = true)]", "using System;"));
        }

    }
}
