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

internal class StructCtorTests
{
    struct Struct
    {
        //// uncomment following line will show error
        private Struct(Int32 _) { }
        public Struct(UInt32 _) { }
        internal Struct(Char _) { }
    }

    struct Allowed { }

    static void Tests()
    {
        // WARNING
        var @struct = new Struct();
        Struct anony = new();

        // OK
        var okay = new Struct(0);
        Struct okay2 = new('A');

        // allowed due to no constructor is declared
        var allowed = new Allowed();
        Allowed allowed2 = new();
    }
}
