﻿using System;
using System.ComponentModel;
using System.Reflection;
using ShortAlias = System.Int16;

namespace AnalyzerCheck;

internal class EnumTests
{
    [Category]
    public enum EShort : ShortAlias { Value }
    public enum EUShort : ushort { Value }

    [Obfuscation()] public enum EByte : byte { Value }
    [Obfuscation] public enum ESByte : sbyte { Value }

    [Obfuscation(StripAfterObfuscation = true)]
    public enum ELong : long { Value }

    // unusual enum definition?
    [Obfuscation(ApplyToMembers = true)]
    public enum EULong : ulong
    {
        Value = 10,
        Other = 20,
    }

    // unusual definition check is disabled by adding 'Flags' attribute
    [Flags]
    [Obfuscation(Exclude = true)]
    public enum EUInt : uint
    {
        Value = 0,
        Other = 1,
    }

    // obfuscation have controlled?
    //   check both Exclude and ApplyToMembers are set to true
    //   expression resulting `true` are accepted
    [Obfuscation(ApplyToMembers = "A" != "B", Exclude = true)]
    public enum EInt : int
    {
        Value = 0,
        Other = 1,
    }


    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class AttrTest : Attribute
    {
        public AttrTest(EInt value = EInt.Value) { }
    }

    EInt MethodDefaultParams(EInt value = EInt.Value, EInt other = EInt.Other) => value;

    [AttrTest, AttrTest()]  // <-- no warning is expected even if default enum parameter is omitted
    void ImplicitCastHappensIfMethodDefaultParameterOmitted()
    {
        // expect: these lines must not get warning
        var value1 = MethodDefaultParams();
        var value2 = MethodDefaultParams(EInt.Value);
        var value3 = MethodDefaultParams(other: EInt.Value);
        var ctorCall = new AttrTest();

        // these lines get warning
        object obj = EInt.Value;
        Enum @enum = EInt.Other;
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

    void BasicTests(EInt value)
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

        // string conversion should be encapsulated and controlled only in
        // app's enum utility. it should not be done freely in user code
        string name = EInt.Value.ToString();
        string other = value.ToString();

        // only allow handling value as is
        _ = EnumConstraintGenericType(value);
    }

    int EnumConstraintGenericType<T>(T value) where T : Enum
    {
        _ = value.ToString();
        _ = (T)(object)(310 + 310);  // require intermediate cast
        _ = (T)(object)value;
        return (int)(object)value;
    }


    // expect no warnings
    int NonEnumGenericTypeParameter<T>(T value) where T : struct
    {
        _ = value.ToString();
        _ = (T)(object)310;
        return (int)(object)value;
    }

}



public enum EnumTypeShouldBeExcludedFromObfuscation
{
    Value = -1,
    Other = 1,
}
