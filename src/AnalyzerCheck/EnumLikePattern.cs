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

//[assembly: AnalyzerCheck.EnumLikePattern("Entries")]

namespace AnalyzerCheck;

//[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
//public sealed class EnumLikePatternAttribute : Attribute { public EnumLikePatternAttribute(string memberName) { } }

internal record EnumLikeRecord { readonly static EnumLikeRecord[] Entries; }
internal struct EnumLikeStruct { readonly static EnumLikeStruct[] Entries; }
internal interface IEnumLikePattern { readonly static IEnumLikePattern[] Entries; }
internal abstract class EnumLikeAbstract { readonly static EnumLikeAbstract[] Entries; }
// not able --> internal static class EnumLikeStatic { readonly static EnumLikeStatic[] Entries; }

sealed
internal class EnumLikePattern
{
    //protected
    EnumLikePattern()
    {
    }

    //// only private and protected ctor is allowed
    //public EnumLikePattern(ulong _) { }
    //internal EnumLikePattern(int _) { }

    internal class EnumLikeGeneric<T> { readonly static EnumLikeGeneric<T>[] Entries; }  // not public 'Entries'

    // all of `readonly static` field must be included in `Entries` array initializer
    public readonly static EnumLikePattern A = new();
    public readonly static EnumLikePattern B = new();
    //// uncomment the following line to report error
    //public readonly static EnumLikePattern C = new();

    // ERROR: implict array creation
    public readonly static EnumLikePattern[] OK_Entries = new[] { A, B };  // OK
    public readonly static EnumLikePattern[] NG_entries = new[] { B, A };  // invalid order
    public readonly static EnumLikePattern[] _entries;
    public readonly static EnumLikePattern[] _Entries = [];
    public readonly static EnumLikePattern[] _0_entries = [null!];
    public readonly static EnumLikePattern[] _1_Entries = new[] { A };

    // ERROR: explicitly typed array creation
    public readonly static EnumLikePattern[] ___OK_Entries = new EnumLikePattern[] { A, B };  // OK
    public readonly static EnumLikePattern[] ___NG_entries = new EnumLikePattern[] { A, A };  // invalid order
    public readonly static EnumLikePattern[] __entries;
    public readonly static EnumLikePattern[] __Entries = new EnumLikePattern[0];
    public readonly static EnumLikePattern[] ___entries = new EnumLikePattern[0] { };
    public readonly static EnumLikePattern[] ___0_entries = new EnumLikePattern[] { };
    public readonly static EnumLikePattern[] ___1_Entries = new EnumLikePattern[] { A };

    // ERROR: ReadOnlyMemoby
    public readonly static ReadOnlyMemory<EnumLikePattern> entries;  // not starting with 'E'ntries but ending with 'e'ntries
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries;
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries_ = new EnumLikePattern[0];
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries__ = new EnumLikePattern[0] { };
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries___ = new(new EnumLikePattern[] { A, A, B });

    // OK: ReadOnlyMemory creation
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries_______ = new[] { A, B };
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries______ = new(new[] { A, B });
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries_____ = new EnumLikePattern[] { A, B };
    public readonly static ReadOnlyMemory<EnumLikePattern> Entries____ = new(new EnumLikePattern[] { A, B });

    // OK: type doesn't match
    public readonly static int[] ______entries = new int[0];
    public readonly static ReadOnlyMemory<int> _______entries = new int[0];

    // OK: name doesn't match
    public readonly static ReadOnlyMemory<EnumLikePattern> entries___;  // targetting only when field name starting with 'E'ntries
    public readonly static ReadOnlyMemory<EnumLikePattern> ____entries2 = new EnumLikePattern[0];
    public readonly static ReadOnlyMemory<EnumLikePattern> the_entries2 = new EnumLikePattern[0];
    public readonly static ReadOnlyMemory<EnumLikePattern> ____Entries2 = new EnumLikePattern[0];
    public readonly static ReadOnlyMemory<EnumLikePattern> the_Entries2 = new EnumLikePattern[0];

}
