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
