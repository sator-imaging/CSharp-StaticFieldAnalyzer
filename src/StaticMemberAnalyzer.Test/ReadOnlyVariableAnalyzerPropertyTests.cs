using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Linq;
using System.Threading.Tasks;
using VerifyCS = StaticMemberAnalyzer.Test.CSharpAnalyzerVerifier<
    SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers.ReadOnlyVariableAnalyzer>;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class ReadOnlyVariableAnalyzerPropertyTests
    {
        [TestMethod]
        public async Task GetterOnlyProperty_Argument_ReportsSMA0063()
        {
            var test = @"
using System;

public class MyRef {}
public class MyObj
{
    public MyRef GetterOnly => new MyRef();
    public MyRef Normal { get; set; }
    public int ValueGetterOnly => 1;
}

class Test
{
    static void Method(MyRef s) {}
    static void Method(int i) {}

    void Run()
    {
        var obj = new MyObj();
        Method({|#0:obj.GetterOnly|});
        Method({|#1:obj.Normal|});
        Method({|#2:obj.ValueGetterOnly|});
    }
}
";
            var expected0 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyProperty)
                .WithLocation(0)
                .WithArguments("obj.GetterOnly");

            // Normal property with reference type: SMA0062
            var expected1 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyArgument)
                .WithLocation(1)
                .WithArguments("obj");

            // ValueGetterOnly (int) was exempted, but now it's SMA0063
            var expected2 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyProperty)
                .WithLocation(2)
                .WithArguments("obj.ValueGetterOnly");

            await VerifyWithRuleEnabledAsync(test, expected0, expected1, expected2);
        }

        [TestMethod]
        public async Task GetterOnlyIndexer_Argument_ReportsSMA0063()
        {
            var test = @"
public class MyObj
{
    public int this[int i] => i;
    public int this[string s] { get => 0; set {} }
}

class Test
{
    static void Method(int i) {}

    void Run()
    {
        var obj = new MyObj();
        Method({|#0:obj[0]|});
        Method(obj[""test""]);
    }
}
";
            var expected0 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyProperty)
                .WithLocation(0)
                .WithArguments("obj[0]");

            // obj["test"] has a setter, so it's not reported (it's a value type int)

            await VerifyWithRuleEnabledAsync(test, expected0);
        }

        [TestMethod]
        public async Task MutPrefixRoot_GetterOnlyProperty_IsAllowed()
        {
            var test = @"
public class MyObj
{
    public int GetterOnly => 1;
}

class Test
{
    static void Method(int i) {}

    void Run()
    {
        var mut_obj = new MyObj();
        Method(mut_obj.GetterOnly);
    }
}
";
            await VerifyWithRuleEnabledAsync(test);
        }

        [TestMethod]
        public async Task StaticGetterOnlyProperty_Argument_ReportsSMA0063()
        {
            var test = @"
public static class StaticClass
{
    public static string GetterOnly => ""test"";
}

class Test
{
    static void Method(string s) {}

    void Run()
    {
        Method({|#0:StaticClass.GetterOnly|});
    }
}
";
            var expected0 = VerifyCS.Diagnostic(ReadOnlyVariableAnalyzer.RuleId_ReadOnlyProperty)
                .WithLocation(0)
                .WithArguments("StaticClass.GetterOnly");

            await VerifyWithRuleEnabledAsync(test, expected0);
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
                specificOptions = specificOptions.SetItem(
                    ReadOnlyVariableAnalyzer.RuleId_ReadOnlyArgument,
                    ReportDiagnostic.Error);
                specificOptions = specificOptions.SetItem(
                    ReadOnlyVariableAnalyzer.RuleId_ReadOnlyProperty,
                    ReportDiagnostic.Error);

                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(specificOptions);
                return solution.WithProjectCompilationOptions(projectId, compilationOptions);
            });

            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }
    }
}
