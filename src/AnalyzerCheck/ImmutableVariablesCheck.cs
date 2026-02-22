using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzerCheck
{
    internal class ImmutableLocals
    {
        public void Test(int param)
        {
            var locals = new ImmutableLocals();
            locals = new();

            param = 310;
            param++;
            (param, var x) = (param, 42);
            param += x;
        }
    }
}
