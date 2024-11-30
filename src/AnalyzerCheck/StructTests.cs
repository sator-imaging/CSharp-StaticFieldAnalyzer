﻿using System;

namespace AnalyzerCheck;

internal class StructTests
{
    struct HasCtor
    {
        //// uncomment following line will show error
        private HasCtor(Int32 _) { }
        public HasCtor(UInt32 _) { }
        internal HasCtor(Char _) { }
    }

    struct NoCtor { }

    static void Tests()
    {
        // WARNING
        var @struct = new HasCtor();
        HasCtor anony = new();

        // OK
        var okay = new HasCtor(0);
        HasCtor okay2 = new('A');

        // allowed due to no constructor is declared
        var allowed = new NoCtor();
        NoCtor allowed2 = new();
    }
}
