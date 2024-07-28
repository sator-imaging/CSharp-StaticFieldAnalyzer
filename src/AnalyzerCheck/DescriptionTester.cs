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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

//#pragma warning disable SMA9000
//#pragma warning disable SMA9001
//#pragma warning disable SMA9002
//#pragma warning disable SMA9100

namespace AnalyzerCheck.Tests
{
    // no underlining on xml document comment
    /// <summary>
    /// <see cref="INoUnderline{TWithDesc, TNoDesc}"/>
    /// <para>
    /// <para>
    /// Nested paragraph. <see cref="INoUnderline{TWithDesc, TNoDesc}"/>
    /// </para>
    /// </para>
    /// </summary>
    /// <inheritdoc cref="INoUnderline{TWithDesc, TNoDesc}"/>
    [Description("[Description] won't draw underline. Need `Attribute` suffix like this: [DescriptionAttribute(...)]")]
    public interface INoUnderline<[DescriptionAttribute] TWithDesc, TNoDesc> where TWithDesc : class, new() where TNoDesc : struct
    {
        TWithDesc Get(TNoDesc value);
    }

    // test for multiple attribute syntaxes
    [DescriptionAttribute, Category] public interface IBase { }
    [DescriptionAttribute("The " + nameof(Base) + " Class"), DisplayName]
    public class Base
    {
        [DescriptionAttribute("The " + nameof(BaseMethod) + " of " + nameof(Base) + " Class")]
        virtual public void BaseMethod() { }
    }

    interface ITSelfTester<TSelf> { }

    [Category("Category Attribute"), DisplayName("Display Name Attribute")]
    [DescriptionAttribute("Description for " + nameof(DescriptionTester) + "?" + "!")]
    [DebuggerDisplay("")]
    public class DescriptionTester : List<Base>, IBase, INoUnderline<Base, long>, ITSelfTester<AnalyzerCheck.Tests.DescriptionTester>
    {
        [DescriptionAttribute("??")] public DescriptionTester() { }

        [DescriptionAttribute] public struct NestedStruct : IBase { }

        [DescriptionAttribute] public void Method(Base bs) { Base localvar; localvar = new(); }
        [DescriptionAttribute] public static void StaticMethod(Base b) { }

        [DescriptionAttribute] public const string CONST_STR = "";
        [DescriptionAttribute] public readonly Base READONLY_STR, OTHER_READONLY;
        [DescriptionAttribute] public readonly static Base STATIC_READONLY_STR, OTHER_STATIC_READONLY;
        [DescriptionAttribute] public Base PROP { [DescriptionAttribute] get; [DescriptionAttribute] set; }
        [DescriptionAttribute] event Action<Base> E, Z;
        [DescriptionAttribute] Action<Base> F, Y;
        [DescriptionAttribute] DescriptionTester[] A, X;
        [DescriptionAttribute] List<Base> G, W;
        [DescriptionAttribute] new public Base this[[DescriptionAttribute] int i] { get => base[i]; }
        [DescriptionAttribute] delegate void BaseAction(Base b, [DescriptionAttribute] int value);
        [DescriptionAttribute] delegate Action<Base, Base> MyDelegate([DescriptionAttribute] Base obj, int value);
        [DescriptionAttribute] public int PrivateSetter { get; private set; }
        [DescriptionAttribute] public Base Get(long value) => new();
        public string AllowExpressionClause_MustNotGetUnderline => string.Empty;  // TEST: no underline on string.Empty

        [DescriptionAttribute("!! exclamation-starting message shows warning")]
        public static int WarningField;
        public static int Ref = WarningField;

        // whitespace in attribute syntax
        [
            /**/ DescriptionAttribute /**/ ("  > No Trivia <  ") /**/
        ]
        static NestedStruct TriviaMustBeIgnored;


        Action<Base> Act = StaticMethod;
        static string Str = CONST_STR + STATIC_READONLY_STR + OTHER_STATIC_READONLY;

        string WithClass = DescriptionTester.Str;
        string OnlyField = Str;
        Type D = typeof(DescriptionTester);

        Action<Base> test = b => { b = new(); };

        // TupleType and TupleElement should not get underline
        Action<long, (int value, float floatValue)> TupleAction = (val, data) =>
        {
            if (data.value > 0)
            {
                return;
            }
        };

        // derived class inherits description
        public class Derived
            : Base
        {
            public override void BaseMethod()
            {
                base.BaseMethod();

                [DescriptionAttribute("Local Func!!")]
                int NestedFunction(int depth)
                {
                    void NestNest()
                    {
                        void NestNestNest()
                        {
                        }
                        NestNestNest();
                    }
                    NestNest();

                    return 0;
                }

                NestedFunction(0);
            }
        }

        class CtorDescriptionBase
        {
            [DescriptionAttribute("base .ctor()")] protected CtorDescriptionBase() { }
        }
        sealed class CtorDescription : CtorDescriptionBase
        {
            [DescriptionAttribute("static ctor is always included")] static CtorDescription() { }
            [DescriptionAttribute(".ctor()")] public CtorDescription() : base() { }
            [DescriptionAttribute(".ctor(string)")] public CtorDescription(string value) : this() { }
            [DescriptionAttribute(".ctor(int, int=0)")] public CtorDescription(int value, int defValue = 0) : this("value") { }
        }
        [DescriptionAttribute]
        struct TestStruct { }

        public Base MethodTest() { throw new Exception(); }
        public (Base?, Base?, Base) MethodTest<[DescriptionAttribute] T>([DescriptionAttribute("Param bs")] ref Base bs,
                                                                         [DescriptionAttribute("Param other")] out T other)
            where T : struct
        {
            // creation expressions
            _ = new CtorDescription();   //objectCreation
            CtorDescription cd = new(310);  //implicitObjectCreation
            var anonymousObj = new { test = new CtorDescription("test") };
            var arrayCreation = new TestStruct[1];
            Span<TestStruct> stackAllocArray = stackalloc TestStruct[1];

            T test = default;
            other = test;

            Base? bl = new();
            var tmp = bl;

            Action<Base> act = (b) => { };
            act = b => { };
            F = Y = act;

            Action<Base, Base> act2 = (b1, b2) => { F(b1); Y(b2); };
            act2 = (b1, b2) => { F(b1); Y(b2); };
            act2.Invoke(bl, bs);

            // TODO: support delegate parameter types
            MyDelegate delegateAction = (b1, b2) => { _ = b1; return act2; };
            delegateAction.Invoke(bs, 310);

            goto LABEL;  // TEST: no underline on label statement
        LABEL:
            return (bl, tmp, bs);
        }

    }
}


// NOTE: this will show many of underlines (CompilationUnitSyntax)
//       must ignore
void Test() { }
