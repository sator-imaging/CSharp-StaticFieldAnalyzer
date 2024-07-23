#pragma warning disable IDE0079

#pragma warning disable IDE0044
#pragma warning disable IDE0051
#pragma warning disable CA2211
#pragma warning disable CA1822

using System.ComponentModel;

namespace AnalyzerCheck;

internal class InterFileReferencing
{
    public readonly static int CrossRef = 10 + OtherClass.I + 20;
    public readonly static double PublicDouble = 99.99;
}

partial struct PartialStruct
{
    public readonly static int InAnotherFile = 310;
    public readonly static int Value = InMainFile;
    public readonly static int OK = Value + InAnotherFile;
}
