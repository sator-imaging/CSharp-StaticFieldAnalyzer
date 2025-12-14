// Licensed under the MIT License
// https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;
using VerifyCS = StaticMemberAnalyzer.Test.CSharpCodeFixVerifier<
    SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers.DisposableAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class DisposableAnalyzerUnitTests
    {
        [TestMethod]
        public async Task SimpleDisposable_WithoutUsing_ReportsDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    class Program
    {
        void Method()
        {
            var d = {|#0:new MyDisposable()|};
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(DisposableAnalyzer.RuleId_MissingUsing)
                .WithLocation(0)
                .WithArguments("MyDisposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task SimpleDisposable_WithUsing_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    class Program
    {
        void Method()
        {
            using var d = new MyDisposable();
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task GenericDisposable_WithoutUsing_ReportsDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable<T> : IDisposable
    {
        public void Dispose() { }
    }

    class Program
    {
        void Method()
        {
            var d = {|#0:new MyDisposable<int>()|};
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(DisposableAnalyzer.RuleId_MissingUsing)
                .WithLocation(0)
                .WithArguments("MyDisposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task GenericDisposable_WithUsing_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable<T> : IDisposable
    {
        public void Dispose() { }
    }

    class Program
    {
        void Method()
        {
            using var d = new MyDisposable<int>();
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NestedDisposable_WithoutUsing_ReportsDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class Outer
    {
        public class NestedDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    class Program
    {
        void Method()
        {
            var d = {|#0:new Outer.NestedDisposable()|};
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(DisposableAnalyzer.RuleId_MissingUsing)
                .WithLocation(0)
                .WithArguments("NestedDisposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task NestedDisposable_WithUsing_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class Outer
    {
        public class NestedDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    class Program
    {
        void Method()
        {
            using var d = new Outer.NestedDisposable();
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task AsyncDisposable_WithoutUsing_ReportsDiagnostic()
        {
            var test = @"
using System;
using System.Threading.Tasks;

namespace Test
{
    class MyAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => default;
    }

    class Program
    {
        async Task Method()
        {
            var d = {|#0:new MyAsyncDisposable()|};
            await Task.CompletedTask;
        }
    }
}
";

            var expected = VerifyCS.Diagnostic(DisposableAnalyzer.RuleId_MissingUsing)
                .WithLocation(0)
                .WithArguments("MyAsyncDisposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task AsyncDisposable_WithUsing_ReportsNoDiagnostic()
        {
            var test = @"
using System;
using System.Threading.Tasks;

namespace Test
{
    class MyAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => default;
    }

    class Program
    {
        async Task Method()
        {
            await using var d = new MyAsyncDisposable();
            await Task.CompletedTask;
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
