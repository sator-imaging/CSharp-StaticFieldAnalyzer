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

using System;
using System.ComponentModel;
using System.Reflection;

namespace AnalyzerCheck;

internal class EnumTests
{
    [Category]
    public enum EShort : short { Value }
    public enum EUShort : ushort { Value }

    [Obfuscation()] public enum EByte : byte { Value }
    [Obfuscation] public enum ESByte : sbyte { Value }

    [Obfuscation(StripAfterObfuscation = true)]
    public enum ELong : long { Value }

    [Obfuscation(ApplyToMembers = true)]
    public enum EInt : int
    {
        Value = -1,
        Other = 1,
    }

    [Obfuscation(Exclude = true)]
    public enum EUInt : uint
    {
        Value,
        Other,
    }

    // both Exclude and ApplyToMembers are set to true
    // * `true` or something produces boolean result are accepted
    [Obfuscation(Exclude = true, ApplyToMembers = "A" != "B")]
    public enum EULong : ulong
    {
        Value,
        Other,
    }


    // expect warnings on all lines
    void EnumCastTests()
    {
        // cast from enum
        _ = (sbyte)EInt.Other;
        _ = (byte)EInt.Other;
        _ = (short)EInt.Other;
        _ = (ushort)EInt.Other;
        _ = (int)((EInt.Other));
        _ = (uint)((EInt.Other));
        _ = (long)((EInt.Other));
        _ = (ulong)((EInt.Other));
        _ = (EULong)(EUInt.Value);
        _ = (EULong)(EInt)(EUInt.Value);
        _ = (EULong)(Enum)(EUInt.Value);
        _ = (EULong)(object)(EUInt.Value);
    }

    void BasicTests(EUInt value)
    {
        // cast to enum can lead invalid value creation
        _ = (EInt)310;
        _ = (EUInt)(-310 * -1);
        _ = (EULong)ulong.MaxValue;

        // cast from enum makes value untyped and untraceable
        _ = (int)EInt.Value;
        _ = (EULong)EUInt.Value;
        object obj = EInt.Value;
        Enum @enum = EInt.Other;

        // all of Enum methods get warning to avoid user-level conversion
        _ = Enum.GetName(EInt.Value);
        _ = Enum.ToObject(typeof(EInt), 0);
        _ = Enum.TryParse<EInt>("", out _);

        // string conversion should be encapsulated and centerized to
        // main app's enum utility. it should not be done freely in user code
        string name = EInt.Value.ToString();
        string generic = value.ToString();
    }

    int EnumConstraintGenericType<T>(T value) where T : Enum
    {
        // casting to generic enum type needs intermediate cast
        _ = (T)(object)(310 + 310);
        return (int)(object)value;
    }


    // expect no warnings
    int NonEnumGenericTypeParameter<T>(T value) where T : struct
    {
        _ = (T)(object)310;
        return (int)(object)value;
    }

}

public enum EnumTypeShouldBeExcludedFromObfuscation
{
    Value = -1,
    Other = 1,
}
