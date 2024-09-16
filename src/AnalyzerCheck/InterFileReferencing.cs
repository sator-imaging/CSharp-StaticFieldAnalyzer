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

internal class InterFileReferencing
{
    public readonly static int CrossRef = 10 + OtherClass.I + 20;
    public readonly static Double PublicDouble = 99.99;
}

partial struct PartialStruct
{
    public readonly static int InAnotherFile = 310;
    public readonly static int Value = InMainFile;
    public readonly static int OK = Value + InAnotherFile;
}
