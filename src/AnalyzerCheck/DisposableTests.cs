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
using System.Threading.Tasks;
using NIE = System.NotImplementedException;

namespace AnalyzerCheck;

internal class DisposableTests
{
    class ClassNoInterface { public void Dispose() { } }
    class ClassWithInterface : IDisposable { public void Dispose() { } }
    class ClassDeriveDisposable : ClassWithInterface { }
    struct StructNoInterface { public void Dispose() { } }
    struct StructWithInterface : IDisposable { public void Dispose() { } }
    ref struct RefLikeHidden { void Dispose() { } }
    ref struct RefLikeAccessible { internal void Dispose() { } }

    class ClassAsyncDisposable : IAsyncDisposable { public ValueTask DisposeAsync() => throw new NIE(); }
    struct StructAsyncDisposable : IAsyncDisposable { public ValueTask DisposeAsync() => throw new NIE(); }
    ref struct RefLikeAsyncDisposable { public ValueTask DisposeAsync() => throw new NIE(); }
    ref struct RefLikeAsyncHidden { ValueTask DisposeAsync() => throw new NIE(); }


    // fields never get warning
    ClassNoInterface classNoInterface = new();
    ClassWithInterface classWithInterface = new();
    ClassDeriveDisposable classDeriveDisposable = new();
    StructNoInterface structNoInterface = new();
    StructWithInterface structWithInterface = new();
    ClassAsyncDisposable classAsyncDisposable = new();
    StructAsyncDisposable structAsyncDisposable = new();

    static object? ObjectField;
    static string? StringField;
    static IDisposable? IDisposableField;

    class Disposable : IDisposable
    {
        public static Disposable New => new();
        public Disposable Self => this;
        public Disposable GetSelf() => this;
        public void Dispose() { }
        public string Return() => string.Empty;
    }

    class ImplicitConversion : IDisposable
    {
        public void Dispose() { }
        public class NonDisposable();
        public static implicit operator string(ImplicitConversion? self) => string.Empty;
    }

#pragma warning disable CA1806
    static void Test()
    {
        // no error on method chaining (remove .Return() will show warning)
        _ = new Disposable().Return();
        _ = (((new Disposable()))).Return();
        _ = Disposable.New.Return();
        _ = new Disposable().Self.Return();
        _ = new Disposable().GetSelf().Return();

        // OK: left hand is not IDisposable
        ObjectField = new ImplicitConversion();
        string s = new ImplicitConversion();
        s = new ImplicitConversion();
        StringField = new ImplicitConversion();
        StringField = new Disposable();  // NG: no implicit cast operator

        IDisposableField = new ImplicitConversion();
        IDisposable d = new ImplicitConversion();
        d = new ImplicitConversion();


        //----------------------------------------------------------------------
        // NG: disposable not using-ed
        //----------------------------------------------------------------------

        new ClassWithInterface();
        var cwi = new ClassWithInterface();
        _ = Create<ClassWithInterface>();
        MethodArg(new ClassWithInterface());
        ObjectField = new ClassWithInterface();

        new ClassDeriveDisposable();
        var cdd = new ClassDeriveDisposable();
        _ = Create<ClassDeriveDisposable>();
        MethodArg(new ClassDeriveDisposable());
        ObjectField = new ClassDeriveDisposable();

        new StructWithInterface();
        var swi = new StructWithInterface();
        _ = Create<StructWithInterface>();
        MethodArg(new StructWithInterface());
        ObjectField = new StructWithInterface();

        new RefLikeAccessible();
        var rla = new RefLikeAccessible();
        _ = CreateRefLikeAccessible();
        MethodArg(new RefLikeAccessible());
        ObjectField = new RefLikeAccessible();


        //----------------------------------------------------------------------
        // OK: using-ed & method accepts variable
        //----------------------------------------------------------------------

        using (new ClassWithInterface()) { }
        using var cwiOK = new ClassWithInterface();
        using (Create<ClassWithInterface>()) { }
        using var cwiMethodOK = Create<ClassWithInterface>();
        MethodArg(cwiOK);
        MethodArg(cwiMethodOK);

        using (new ClassDeriveDisposable()) { }
        using var cddOK = new ClassDeriveDisposable();
        using (Create<ClassDeriveDisposable>()) { }
        using var cddMethodOK = Create<ClassDeriveDisposable>();
        MethodArg(cddOK);
        MethodArg(cddMethodOK);

        using (new StructWithInterface()) { }
        using var swiOK = new StructWithInterface();
        using (Create<StructWithInterface>()) { }
        using var swiMethodOK = Create<StructWithInterface>();
        MethodArg(swiOK);
        MethodArg(swiMethodOK);

        using (new RefLikeAccessible()) { }
        using var rlaOK = new RefLikeAccessible();
        using (CreateRefLikeAccessible()) { }
        using var rlaMethodOK = CreateRefLikeAccessible();
        MethodArg(rlaOK);
        MethodArg(rlaMethodOK);
    }


    static async void TestAsync()
    {
        //----------------------------------------------------------------------
        // NG: not using
        //----------------------------------------------------------------------

        new ClassAsyncDisposable();
        var cad = new ClassAsyncDisposable();
        _ = Create<ClassAsyncDisposable>();
        MethodArg(new ClassAsyncDisposable());
        ObjectField = new ClassAsyncDisposable();

        new StructAsyncDisposable();
        var sad = new StructAsyncDisposable();
        _ = Create<StructAsyncDisposable>();
        MethodArg(new StructAsyncDisposable());
        ObjectField = new StructAsyncDisposable();

        new RefLikeAsyncDisposable();
        var rlad = new RefLikeAsyncDisposable();
        _ = CreateRefLikeAsyncDisposable();
        MethodArg(new RefLikeAsyncDisposable());
        ObjectField = new RefLikeAsyncDisposable();


        //----------------------------------------------------------------------
        // OK: await-using-ed
        //----------------------------------------------------------------------

        await using (new ClassAsyncDisposable()) { }
        await using var cadOK = new ClassAsyncDisposable();
        await using (Create<ClassAsyncDisposable>()) { }
        await using var cadMethodOK = Create<ClassAsyncDisposable>();
        MethodArg(cadOK);
        MethodArg(cadMethodOK);

        await using (new StructAsyncDisposable()) { }
        await using var sadOK = new StructAsyncDisposable();
        await using (Create<StructAsyncDisposable>()) { }
        await using var sadMethodOK = Create<StructAsyncDisposable>();
        MethodArg(sadOK);
        MethodArg(sadMethodOK);

        await using (new RefLikeAsyncDisposable()) { }
        await using var rladOK = new RefLikeAsyncDisposable();
        await using (CreateRefLikeAsyncDisposable()) { }
        await using var rladMethodOK = CreateRefLikeAsyncDisposable();
        MethodArg(rladOK);
        MethodArg(rladMethodOK);

    }


    static void TestNonDisposable()
    {
        //----------------------------------------------------------------------
        // OK: not disposable
        //----------------------------------------------------------------------

        new ClassNoInterface();
        var cni = new ClassNoInterface();
        _ = Create<ClassNoInterface>();
        MethodArg(new ClassNoInterface());
        ObjectField = new ClassNoInterface();

        new StructNoInterface();
        var sni = new StructNoInterface();
        _ = Create<StructNoInterface>();
        MethodArg(new StructNoInterface());
        ObjectField = new StructNoInterface();

        new RefLikeHidden();
        var rlh = new RefLikeHidden();
        _ = CreateRefLikeHidden();
        MethodArg(new RefLikeHidden());
        ObjectField = new RefLikeHidden();

        new RefLikeAsyncHidden();
        var rlah = new RefLikeAsyncHidden();
        _ = CreateRefLikeAsyncHidden();
        MethodArg(new RefLikeAsyncHidden());
        ObjectField = new RefLikeAsyncHidden();
    }


    static void MethodArg(object _) { }
    static void MethodArg(RefLikeHidden _) { }
    static void MethodArg(RefLikeAccessible _) { }
    static void MethodArg(RefLikeAsyncDisposable _) { }
    static void MethodArg(RefLikeAsyncHidden _) { }

    static T Create<T>() where T : new() => new();
    static RefLikeHidden CreateRefLikeHidden() => new();
    static RefLikeAccessible CreateRefLikeAccessible() => new();
    static RefLikeAsyncDisposable CreateRefLikeAsyncDisposable() => new();
    static RefLikeAsyncHidden CreateRefLikeAsyncHidden() => new();
}
