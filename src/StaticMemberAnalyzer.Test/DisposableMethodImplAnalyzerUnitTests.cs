// Licensed under the MIT License
// https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers;
using System.Threading.Tasks;
using VerifyCS = StaticMemberAnalyzer.Test.CSharpCodeFixVerifier<
    SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers.DisposableMethodImplAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SatorImaging.StaticMemberAnalyzer.Test
{
    [TestClass]
    public class DisposableMethodImplAnalyzerUnitTests
    {
        [TestMethod]
        public async Task UndisposedField_ReportsDiagnostic()
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
        private MyDisposable {|#0:_disposable|};

        public void Dispose()
        {
        }
    }
}
";
            var expected = VerifyCS.Diagnostic(DisposableMethodImplAnalyzer.RuleId)
                .WithLocation(0)
                .WithArguments("_disposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task DisposedField_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    class Program : IDisposable
    {
        private MyDisposable _disposable;

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task DisposedField_ExplicitCast_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    class Program : IDisposable
    {
        private MyDisposable _disposable;

        public void Dispose()
        {
            ((IDisposable)_disposable).Dispose();
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task DisposedField_ConditionalAccess_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class MyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    class Program : IDisposable
    {
        private MyDisposable _disposable;

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task DuckTypedDisposable_Undisposed_ReportsDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class DuckDisposable
    {
        public void Dispose() { }
    }

    class Program
    {
        private DuckDisposable {|#0:_disposable|};

        public void Dispose()
        {
        }
    }
}
";
            var expected = VerifyCS.Diagnostic(DisposableMethodImplAnalyzer.RuleId)
                .WithLocation(0)
                .WithArguments("_disposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task DuckTypedDisposable_Disposed_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class DuckDisposable
    {
        public void Dispose() { }
    }

    class Program
    {
        private DuckDisposable _disposable;

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task UndisposedProperty_ReportsDiagnostic()
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
        private MyDisposable {|#0:DisposableProperty|} { get; set; }

        public void Dispose()
        {
        }
    }
}
";
            var expected = VerifyCS.Diagnostic(DisposableMethodImplAnalyzer.RuleId)
                .WithLocation(0)
                .WithArguments("DisposableProperty");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task NonDisposableField_ReportsNoDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class Program
    {
        private string _notDisposable;

        public void Dispose()
        {
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task IDisposableTypedField_Undisposed_ReportsDiagnostic()
        {
            var test = @"
using System;

namespace Test
{
    class Program
    {
        private IDisposable {|#0:_disposable|};

        public void Dispose()
        {
        }
    }
}
";
            var expected = VerifyCS.Diagnostic(DisposableMethodImplAnalyzer.RuleId)
                .WithLocation(0)
                .WithArguments("_disposable");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task PropertyWithBody_ReportsNoDiagnostic()
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
        private MyDisposable _disposable;
        public MyDisposable Prop => _disposable;

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
