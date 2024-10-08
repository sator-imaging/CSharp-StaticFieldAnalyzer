﻿#pragma warning disable IDE0079
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

using System.ComponentModel;

namespace AnalyzerCheck;





public static class Test
{
    // error reading uninitialized static field
    public static int BeforeInit = IntValue;
    public static int IntValue = 310;

    // cross referencing across type *may* cause problem
    // due to changing initialization order implicitly
    public static int CrossRef = AnotherTest.Value;
}

public static class AnotherTest
{
    public static int CrossRef = Test.IntValue;
    public static int Value = 310;
}






public class UnderliningTest
{
    [Description("Set description for VS form designer w/o underlining")]
    public static int NO_UNDERLINE;

    [DescriptionAttribute("This message is shown when mouseover")]
    //          ^^^^^^^^^  add 'Attribute' suffix to draw underline
    public static int DRAW_UNDERLINE_ON_THIS_FIELD;


    [DescriptionAttribute]
    public void DefaultMsgWhenNoArgsSupplied() { }

    [DescriptionAttribute("!! exclamation-starting message shows warning")]
    public static int AttentionPlease;

    class DrawingTest
    {
        void Main()
        {
            _ = NO_UNDERLINE;
            _ = DRAW_UNDERLINE_ON_THIS_FIELD + 310;

            (new UnderliningTest()).DefaultMsgWhenNoArgsSupplied();

            _ = AttentionPlease;
        }
    }
}





public interface INumber<TSelf> where TSelf : INumber<TSelf> { }

public class GenericMath : INumber<GenericMath> { }
public class OtherClass : INumber<GenericMath> { }  // type arg 'TSelf' should be pointing to itself









public class VerbosityControlTest
{
    public class VerbosityControl
    {
        public class UnderlineTypes
        {
            // by default, line head/lead/end and keyword underlines are drawn.
            [DescriptionAttribute]
            static int FULL_UNDERLINING = 10;

            // to control underlining verbosity, use '#pragma' preprocessor directive or
            // add 'SuppressMessage' attribute to assembly, type or something
            #pragma warning disable SMA9020 // Underlining at Line Head
            #pragma warning disable SMA9021 // Underlining at Line Leading
            #pragma warning disable SMA9023 // Underlining at Line End
            [DescriptionAttribute]
            static int UNDERLINE_ONLY_ON_KEYWORD = 10;

            #pragma warning restore SMA9021 // Underlining at Line Leading
            [DescriptionAttribute]
            static int RESTORE_LINE_LEADING = 10;

            #pragma warning restore SMA9020 // Underlining at Line Head
            [DescriptionAttribute]
            static int RESTORE_LINE_HEAD = 10;

        }
    }
}
