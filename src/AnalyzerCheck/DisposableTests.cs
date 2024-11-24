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


    // field initializer never get warning
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

    // no error on return statement
    Disposable MethodReturn1() => new();
    Disposable MethodReturn2() { return new Disposable(); }

    static void Test(Disposable methodParam)
    {
        // no error, using statement
        using Disposable localVar = new Disposable();

        // no error on method chaining (remove .Return() will show warning)
        _ = new Disposable().Return();
        _ = (((new Disposable()))).Return();
        _ = Disposable.New.Return();
        _ = new Disposable().Self.Return();
        _ = new Disposable().GetSelf().Return();

        // cast operation: won't show warning if both type are disposable
        _ = localVar as IDisposable;
        _ = (IDisposable)localVar;
        _ = methodParam as IDisposable;
        _ = (IDisposable)methodParam;

        // show warning
        IDisposable notUsing1 = ((new object() as IDisposable))!;
        IDisposable notUsing2 = new object() as IDisposable;
        _ = (new object()) as Disposable;
        _ = (IDisposable)(new object());
        _ = (new Disposable()) as object;

        // field assignment won't show warning
        IDisposableField = (((new object()) as IDisposable)!);
        IDisposableField = (new object()) as Disposable;
        IDisposableField = (IDisposable)(new object());
        //IDisposableField = (new Disposable()) as object;
        ObjectField = ((new object() as IDisposable)!);
        ObjectField = (new object()) as Disposable;
        ObjectField = (IDisposable)(new object());
        ObjectField = new Disposable() as object;   // warn: it's casted from disposable to non-disposable

        // OK: left hand is field or property
        IDisposableField = new Disposable();
        IDisposableField = new ImplicitConversion();
        ObjectField = new ImplicitConversion();
        ObjectField = new Disposable();
        // implicit string conversion
        string s = new ImplicitConversion();
        s = new ImplicitConversion();
        StringField = new ImplicitConversion();

        // show warning
        IDisposable d = new ImplicitConversion();
        d = new ImplicitConversion();
        d = new Disposable();

        // assignment won't get warning including implicit cast
        d = localVar;
        d = notUsing1;

        IDisposableField = new ClassWithInterface();
        IDisposableField = new ClassDeriveDisposable();
        IDisposableField = new StructWithInterface();
        //IDisposableField = new RefLikeAccessible();
        //ObjectField = new RefLikeAccessible();
        ObjectField = new ClassWithInterface();
        ObjectField = new ClassDeriveDisposable();
        ObjectField = new StructWithInterface();

        //----------------------------------------------------------------------
        // NG: disposable not using-ed
        //----------------------------------------------------------------------

        new ClassWithInterface();
        var cwi = new ClassWithInterface();
        _ = Create<ClassWithInterface>();
        MethodArg(new ClassWithInterface());

        new ClassDeriveDisposable();
        var cdd = new ClassDeriveDisposable();
        _ = Create<ClassDeriveDisposable>();
        MethodArg(new ClassDeriveDisposable());

        new StructWithInterface();
        var swi = new StructWithInterface();
        _ = Create<StructWithInterface>();
        MethodArg(new StructWithInterface());

        new RefLikeAccessible();
        var rla = new RefLikeAccessible();
        _ = CreateRefLikeAccessible();
        MethodArg(new RefLikeAccessible());


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
        //IDisposableField = new ClassAsyncDisposable();
        //IDisposableField = new StructAsyncDisposable();
        //IDisposableField = new RefLikeAsyncDisposable();
        //ObjectField = new RefLikeAsyncDisposable();
        ObjectField = new ClassAsyncDisposable();
        ObjectField = new StructAsyncDisposable();

        //----------------------------------------------------------------------
        // NG: not using
        //----------------------------------------------------------------------

        new ClassAsyncDisposable();
        var cad = new ClassAsyncDisposable();
        _ = Create<ClassAsyncDisposable>();
        MethodArg(new ClassAsyncDisposable());

        new StructAsyncDisposable();
        var sad = new StructAsyncDisposable();
        _ = Create<StructAsyncDisposable>();
        MethodArg(new StructAsyncDisposable());

        new RefLikeAsyncDisposable();
        var rlad = new RefLikeAsyncDisposable();
        _ = CreateRefLikeAsyncDisposable();
        MethodArg(new RefLikeAsyncDisposable());


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
        //IDisposableField = new ClassNoInterface();
        //IDisposableField = new StructNoInterface();
        //IDisposableField = new RefLikeHidden();
        //IDisposableField = new RefLikeAsyncHidden();
        //ObjectField = new RefLikeHidden();
        //ObjectField = new RefLikeAsyncHidden();
        ObjectField = new ClassNoInterface();
        ObjectField = new StructNoInterface();

        //----------------------------------------------------------------------
        // OK: not disposable
        //----------------------------------------------------------------------

        new ClassNoInterface();
        var cni = new ClassNoInterface();
        _ = Create<ClassNoInterface>();
        MethodArg(new ClassNoInterface());

        new StructNoInterface();
        var sni = new StructNoInterface();
        _ = Create<StructNoInterface>();
        MethodArg(new StructNoInterface());

        new RefLikeHidden();
        var rlh = new RefLikeHidden();
        _ = CreateRefLikeHidden();
        MethodArg(new RefLikeHidden());

        new RefLikeAsyncHidden();
        var rlah = new RefLikeAsyncHidden();
        _ = CreateRefLikeAsyncHidden();
        MethodArg(new RefLikeAsyncHidden());
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
