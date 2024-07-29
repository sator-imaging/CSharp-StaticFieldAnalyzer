#pragma warning disable IDE0079

#pragma warning disable IDE0044
#pragma warning disable IDE0051
#pragma warning disable CA2211
#pragma warning disable CA1822

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
    public static int Other = AttentionPlease;
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
            [DescriptionAttribute] static int FULL_UNDERLINING = 10;

            // to control underlining verbosity, use '#pragma' preprocessor directive or
            // add 'SuppressMessage' attribute to assembly, type or something
            #pragma warning disable SMA9020 // Underlining at Line Head
            #pragma warning disable SMA9021 // Underlining at Line Leading
            #pragma warning disable SMA9023 // Underlining at Line End
            [DescriptionAttribute] static int UNDERLINE_ONLY_ON_KEYWORD = 10;
        }
    }
}
