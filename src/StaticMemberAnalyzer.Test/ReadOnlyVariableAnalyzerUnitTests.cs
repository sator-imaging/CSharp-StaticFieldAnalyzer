// Licensed under the MIT License
// https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;
using VerifyCS = StaticMemberAnalyzer.Test.CSharpAnalyzerVerifier<
    SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers.ReadOnlyVariableAnalyzer>;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class ReadOnlyVariableAnalyzerUnitTests
    {
        [TestMethod]
        public async Task SimpleAssignment_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int foo = 0;
            {|#0:foo|} = 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task CompoundAssignment_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int foo = 0;
            {|#0:foo|} += 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task IncrementAssignment_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int foo = 0;
            {|#0:foo|}++;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task CoalesceAssignment_Local_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int? foo = null;
            {|#0:foo|} ??= 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task CoalesceAssignment_Parameter_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M(int? foo)
        {
            {|#0:foo|} ??= 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task DeconstructionAssignment_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            var tuple = (1, 2);
            int left = 0;
            int right = 0;
            ({|#0:left|}, {|#1:right|}) = tuple;
        }
    }
}
";

            var expected0 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("left");
            var expected1 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(1)
                .WithArguments("right");

            await VerifyWithRuleEnabledAsync(test, expected0, expected1);
        }

        [TestMethod]
        public async Task DeconstructionDeclaration_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            var tuple = (1, 2);
            var ({|#0:leftValue|}, {|#1:rightValue|}) = tuple;
        }
    }
}
";

            var expected0 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("leftValue");
            var expected1 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(1)
                .WithArguments("rightValue");

            await VerifyWithRuleEnabledAsync(test, expected0, expected1);
        }

        [TestMethod]
        public async Task SingleLetterLocal_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int i = 0;
            {|#0:i|} = 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("i");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task MutPrefixLocal_IsAllowed()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int mut_count = 0;
            mut_count = 1;
            mut_count += 2;
            mut_count++;
        }
    }
}
";

            await VerifyWithRuleEnabledAsync(test);
        }

        [TestMethod]
        public async Task MethodParameterAssignment_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M(int valueParam)
        {
            {|#0:valueParam|} = 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter)
                .WithLocation(0)
                .WithArguments("valueParam");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task IndexerAndSetterParameterAssignments_ReportDiagnostic()
        {
            var test = @"
namespace Test
{
    class MyType
    {
        int _x;

        public int MyProp
        {
            set
            {
                {|#0:value|} = 123;
                _x = value;
            }
        }

        public int this[int index]
        {
            get => _x + index;
            set
            {
                {|#1:index|} += 1;
                {|#2:value|} = index;
                _x = value;
            }
        }
    }
}
";

            var expected0 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter)
                .WithLocation(0)
                .WithArguments("value");
            var expected1 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter)
                .WithLocation(2)
                .WithArguments("value");
            var expected2 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter)
                .WithLocation(1)
                .WithArguments("index");

            await VerifyWithRuleEnabledAsync(test, expected0, expected2, expected1);
        }

        [TestMethod]
        public async Task MemberAccessRootedAtLocal_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Box
    {
        public Box Next { get; set; }
        public int Value { get; set; }
    }

    class Program
    {
        void M()
        {
            var foo = new Box { Next = new Box() };
            {|#0:foo.Next.Value|} = 310;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task MemberAccessRootedAtParameter_ReportsDiagnostic()
        {
            var test = @"
namespace Test
{
    class Box
    {
        public Box Next { get; set; }
        public int Value { get; set; }
    }

    class Program
    {
        void M(Box foo)
        {
            {|#0:foo.Next.Value|} = 310;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task MemberAccessRootedAtField_NoDiagnostic()
        {
            var test = @"
namespace Test
{
    class Box
    {
        public Box Next { get; set; }
        public int Value { get; set; }
    }

    class Program
    {
        private Box _foo = new Box { Next = new Box() };

        void M()
        {
            _foo.Next.Value = 310;
        }
    }
}
";

            await VerifyWithRuleEnabledAsync(test);
        }

        [TestMethod]
        public async Task ForStatementHeaderAssignments_AreAllowed()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M(int p)
        {
            int i = 0;
            for (i = 0, p += 1; (i += 1) < 10; i += 2, p--)
            {
            }
        }
    }
}
";

            await VerifyWithRuleEnabledAsync(test);
        }

        [TestMethod]
        public async Task ForStatementBodyAssignment_IsStillReported()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            for (int i = 0; i < 10; i++)
            {
                {|#0:i|} = 2;
            }
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("i");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task OutVarCall_NotReported_ButSubsequentAssignment_Reported()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void N()
        {
            int.TryParse(""1"", out var foo);
            {|#0:foo|} = 1;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal)
                .WithLocation(0)
                .WithArguments("foo");

            await VerifyWithRuleEnabledAsync(test, expected);
        }

        [TestMethod]
        public async Task RuleSuppressed_NoDiagnostic()
        {
            var test = @"
namespace Test
{
    class Program
    {
        void M()
        {
            int foo = 0;
            foo = 1;
        }
    }
}
";

            var verifier = new VerifyCS.Test
            {
                TestCode = test,
            };

            verifier.SolutionTransforms.Add((solution, projectId) =>
            {
                var project = solution.GetProject(projectId);
                var compilationOptions = project?.CompilationOptions;
                if (compilationOptions == null)
                    return solution;

                var specificOptions = compilationOptions.SpecificDiagnosticOptions.SetItem(
                    ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal,
                    ReportDiagnostic.Suppress);
                specificOptions = specificOptions.SetItem(
                    ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter,
                    ReportDiagnostic.Suppress);

                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(specificOptions);
                return solution.WithProjectCompilationOptions(projectId, compilationOptions);
            });

            await verifier.RunAsync();
        }

        private static async Task VerifyWithRuleEnabledAsync(string source, params Microsoft.CodeAnalysis.Testing.DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
            };

            test.SolutionTransforms.Add((solution, projectId) =>
            {
                var project = solution.GetProject(projectId);
                var compilationOptions = project?.CompilationOptions;
                if (compilationOptions == null)
                    return solution;

                var specificOptions = compilationOptions.SpecificDiagnosticOptions.SetItem(
                    ReadOnlyVariableAnalyzer.RuleId_ReadOnlyLocal,
                    ReportDiagnostic.Error);
                specificOptions = specificOptions.SetItem(
                    ReadOnlyVariableAnalyzer.RuleId_ReadOnlyParameter,
                    ReportDiagnostic.Error);

                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(specificOptions);
                return solution.WithProjectCompilationOptions(projectId, compilationOptions);
            });

            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }
    }
}
