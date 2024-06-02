using System;

namespace AnalyzerCheck;

public class Program
{
    public readonly static Program Instance = new();

    public readonly static float F = StaticFields.F;    // error
    public readonly static int I = 310;

    public static int Property { get; } = AFTER;  // error reading uninitialized value (0) from static field in same type
    public readonly static byte BEFORE = AFTER;   // error 
    public readonly static byte AFTER = 155;      // <-- move this line above to fix problem
    public readonly static byte CORRECT = AFTER;


    public static void Main()
    {
        Console.WriteLine(BEFORE);  // 0 (wrong init)
        Console.WriteLine(AFTER);
        Console.WriteLine(CORRECT);
        Console.WriteLine(StaticFields.F);
        Console.WriteLine(StaticFields.I);  // 0 (cross ref)
                                            // note that this value is initialized correctly when 'this.F' is declared after 'this.I'
        Console.WriteLine(F);  // error reported but 'F' value is correct. cross referencing implicitly changes initialization order
        Console.WriteLine(I);  // and COULD cause invalid result. it's hard to detect perfectly. cross-referencing should be fixed anyway
    }

    static float CROSS_REF_MULTIPLE = 10 + StaticFields.F + 20;  // error must be reported on each access to same field and
                                                                 // analyzer must aware field initializer has nested operators

    // no error on const values
    const string CONST_MUST_BE_IGNORED = StaticFields.CONST_STR;
    static string CONST_ASSIGNMENT_TOO = StaticFields.CONST_STR;
    // no error on non-static field
    public readonly float OK = StaticFields.F;
    public byte B;
    // no error on static fields without initializer
    public static byte SB;
    public static readonly byte SRB;
    public static int FromNest = Nested.Value;
    // no error on nameof/typeof syntax
    public static Type TypeOf = typeof(StaticFields.Nested);
    public static string NameOf = "" + nameof(StaticFields.F) + "";
    // no error on static method access
    public static Action StaticAction = StaticMethod;
    public static Action StaticActionXR = StaticFields.StaticMethod;
    public static Func<bool> StaticFunc = StaticFuncBool;
    public static Func<bool> StaticFuncXR = StaticFields.StaticFuncBool;
    // no error on access in delegate
    public static Action ActionDef = () => { Console.WriteLine(StaticFields.I); };
    public static Func<int> FuncDef = () => StaticFields.I;
    // no error in setter/getter
    static float f;
    public static float Getter { get => f + StaticFields.F; }
    public static float Setter { set => f = StaticFields.F; }

    public static class Nested { public static int Value = -310; }
    public static void StaticMethod() { }
    public static bool StaticFuncBool() => true;

}

public class StaticFields
{
    public const string CONST_STR = "Hello, world.";
    public readonly static int I = Program.I;
    public readonly static double D = 3.10 + InterFileReferencing.PublicDouble;
    public readonly static float F = 0.31f;

    public static class Nested { }
    public static void StaticMethod() { }
    public static bool StaticFuncBool() => false;
}

public class CrazySource { public static float Value = CrazyDestination.Value; }

public class CrazyDestination { public static float Value = CrazySource.Value; }
