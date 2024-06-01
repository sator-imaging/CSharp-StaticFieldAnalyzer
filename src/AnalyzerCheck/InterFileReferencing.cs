using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzerCheck;

internal class InterFileReferencing
{
    public readonly static int CrossRef = 10 + StaticFields.I + 20;
    public readonly static double PublicDouble = 99.99;
}




public static class Test
{
    // error reading uninitialized static field
    public static int BeforeInit = IntValue;
    public static int IntValue = 310;

    // cross referencing across type *may* cause problem
    // due to changing initialization order implicitly
    public static int CrossRef = OtherClass.Value;
}

public static class OtherClass
{
    public static int CrossRef = Test.IntValue;
    public static int Value = 310;
}
