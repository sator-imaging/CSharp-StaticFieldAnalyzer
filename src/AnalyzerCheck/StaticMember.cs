﻿using System;

namespace AnalyzerCheck;

public partial struct PartialStruct
{
    public static int InMainFile = InAnotherFile;  // error reading field declared in another file
    public static int OkToRead = InMainFile;  // can read static field declared in same file
    public readonly static byte AFTER = 155;  // testing full member name check is done or not.
                                              // if checking is NOT correct, error shown on AFTER in other type
}

public class StaticMember
{
    public readonly static float F = OtherClass.F;  // error cross-ref
    public readonly static int I = 310;

    public static int Property { get; } = AFTER;  // error reading uninitialized value (0) from static field in same type
    public readonly static byte BEFORE = AFTER;   // error namespace and class must be considered (AFTER field in other type must be ignored)
    public readonly static byte AFTER = 155;      // <-- move this line above to fix problem
    public readonly static byte CORRECT = AFTER;

    public static void Main()
    {
        Console.WriteLine(BEFORE);  // 0 (wrong init)
        Console.WriteLine(AFTER);
        Console.WriteLine(CORRECT);
        Console.WriteLine(OtherClass.F);
        Console.WriteLine(OtherClass.I);  // 0 (cross ref)
                                          // note that this value is initialized correctly when 'this.F' is declared after 'this.I'
        Console.WriteLine(F);  // error reported but 'F' value is correct. cross referencing implicitly changes initialization order
        Console.WriteLine(I);  // and COULD cause invalid result. it's hard to detect perfectly. cross-referencing should be fixed anyway
    }

    static float CROSS_REF_MULTIPLE = 10 + OtherClass.F + 20;   // error must be reported on each access to same field and
                                                                // analyzer must aware field initializer has nested operators

    // no error on reading const
    const string CONST_MUST_BE_IGNORED = OtherClass.CONST_STR;
    static string CONST_ASSIGNMENT_TOO = OtherClass.CONST_STR;
    // no error on non-static field
    public readonly float OK = OtherClass.F;
    public byte B;
    // no error on static fields without initializer
    public static byte SB;
    public static readonly byte SRB;
    public static int FromNest = Nested.Value;
    // no error on nameof/typeof syntax
    public static Type TypeOf = typeof(OtherClass.Nested);
    public static string NameOf = "" + nameof(OtherClass.F) + "";
    // no error on static method access
    public static Action StaticAction = StaticMethod;
    public static Action StaticActionXR = OtherClass.StaticMethod;
    public static Func<bool> StaticFunc = StaticFuncBool;
    public static Func<bool> StaticFuncXR = OtherClass.StaticFuncBool;
    // no error on access in delegate
    public static Action ActionDef = () => { Console.WriteLine(OtherClass.I); };
    public static Func<int> FuncDef = () => OtherClass.I;
    // no error in setter/getter
    static float f;
    public static float Getter { get => f + OtherClass.F; }
    public static float Setter { set => f = OtherClass.F; }

    public static class Nested { public static int Value = -310; }
    public static void StaticMethod() { }
    public static bool StaticFuncBool() => true;

}

public class OtherClass
{
    public const string CONST_STR = "Hello, world.";
    public readonly static int I = StaticMember.I;
    public readonly static double D = 3.10 + InterFileReferencing.PublicDouble;
    public readonly static float F = 0.31f;

    public static class Nested { }
    public static void StaticMethod() { }
    public static bool StaticFuncBool() => false;
}

public class CrazySource { public static float Value = CrazyDestination.Value; }

public class CrazyDestination { public static float Value = CrazySource.Value; }
