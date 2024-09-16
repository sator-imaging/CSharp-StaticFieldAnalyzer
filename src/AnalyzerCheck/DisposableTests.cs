#pragma warning disable IDE0079
#pragma warning disable IDE0062
#pragma warning disable IDE0059
#pragma warning disable IDE0039
#pragma warning disable CS0168
#pragma warning disable IDE0060
#pragma warning disable IDE0052
#pragma warning disable IDE0044
#pragma warning disable IDE0051
#pragma warning disable CS8618
#pragma warning disable CA1822
#pragma warning disable CA2211
#pragma warning disable CA1825
#pragma warning disable IDE0300
#pragma warning disable CS0219

using System;

namespace AnalyzerCheck;

internal class DisposableTests
{
    private DisposableImpl disposableField = new();
    private DisposableImpl disposableProperty = new DisposableImpl();

    static void Main()
    {
        var disposableWithoutUsing = new DisposableImpl();  // NG
        using var disposableSimple = new DisposableImpl();
        using (var disposableBlock = new DisposableImpl()) { }
        DisposableImpl anonyDisposableWithoutUsing = new();  // NG
        using DisposableImpl anonyDisposableSimple = new();
        using (DisposableImpl anonyDisposableBlock = new()) { }

        var patternWithoutUsing = new DisposablePattern();  // NG
        using var patternSimple = new DisposablePattern();
        using (var patternBlock = new DisposablePattern()) { }
        DisposablePattern anonyPatternWithoutUsing = new();  // NG
        using DisposablePattern anonyPatternSimple = new();
        using (DisposablePattern anonyPatternBlock = new()) { }

        // OK: hidden disposable won't show 'NotUsing' diagnostics (but cannot be compiled)
        var hiddenWithoutUsing = new HiddenDisposable();
        HiddenDisposable anonyHiddenWithoutUsing = new();

        //using var hiddenSimple = new HiddenDisposable();
        //using (var hiddenBlock = new HiddenDisposable()) { }
        //using HiddenDisposable anonyHiddenSimple = new();
        //using (HiddenDisposable anonyHiddenBlock = new()) { }
    }
}

public class DisposableImpl : IDisposable
{
    public void Dispose() { }
}

public readonly ref struct DisposablePattern
{
    internal void Dispose() { }
}

public ref struct HiddenDisposable()
{
    void Dispose() { }
}
