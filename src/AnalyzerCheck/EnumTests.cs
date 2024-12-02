﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using ShortAlias = System.Int16;

namespace AnalyzerCheck;

file static class Extensions
{
    public static string ToStringNoWarn<T>(this T value) where T : Enum => string.Empty;
}

file static class ThrowHelper
{
    public static Exception CreateException(string value) => new Exception(value);
}

internal class EnumTests
{
    // expect: warning on enum underlying values
    public enum EShort : ShortAlias { Value }
    public enum EUShort : ushort { Value }

    [Obfuscation()] public enum EByte : byte { Value }
    [Obfuscation] public enum ESByte : sbyte { Value }

    [Obfuscation(StripAfterObfuscation = true)]
    public enum ELong : long { Value }

    public enum EInt : int  // <-- int is allowed
    {
        // warning: unusual enum definition is not allowed if no 'Flags' attribute
        Value = 0,

        [EnumMember(Value = "Name")]  // expect: no warn
        Other = 1,
    }

    // obfuscation have controlled?
    //   check both Exclude and ApplyToMembers are set to true
    //   expression resulting `true` are accepted
    [Obfuscation(ApplyToMembers = "A" != "B", Exclude = true)]
    public enum EULong : ulong
    {
        Value = 10,
        Other = 20,
    }

    [Flags]  // <-- unusual definition check is disabled by adding 'Flags' attribute
    [Obfuscation(Exclude = true)]
    public enum EUInt : uint
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

    class EnumHolder { public EInt Value; }
    EInt[] EnumArrayField = new[] { EInt.Value, EInt.Other };
    List<EInt> EnumListField = [EInt.Value, EInt.Other];
    EnumHolder[] EnumHolderArrayField = new EnumHolder[2];
    object ObjectField;

    [AttrTest, AttrTest()]  // <-- no warning is expected even if default enum parameter is omitted
    string ImplicitCastHappensIfMethodDefaultParameterOmitted(EInt value, string? test = null)
    {
        // expect: these lines must not get warning
        var value1 = MethodDefaultParams();
        var value2 = MethodDefaultParams(EInt.Value);
        var value3 = MethodDefaultParams(other: EInt.Value);
        var ctorCall = new AttrTest();

        var valFromArray = EnumArrayField[0];
        valFromArray = EnumHolderArrayField[0].Value;

        // these lines get warning
        object obj = EInt.Value;
        Enum @enum = EInt.Other;
        var valueCast2 = MethodDefaultParams((EInt)EUInt.Value);
        var valueCast3 = MethodDefaultParams(other: (EInt)EUInt.Value);

        // expect: warn
        object valObj = value switch
        {
            EInt.Value => EUInt.Value,
            _ => throw new Exception(value.ToStringNoWarn()),
        };

        // expect: warn
        this.ObjectField = value switch
        {
            EInt.Value => EUInt.Value,
            _ => throw new Exception(value.ToStringNoWarn()),
        };

        // expect: no warn after this line except for switch arm statement
        switch (value)
        {
            case EInt.Value:
                break;
            default:
                throw ThrowHelper.CreateException(value.ToStringNoWarn());
        }

        EnumArrayField[0] = value switch
        {
            EInt.Value => EInt.Value,
            _ => throw new Exception(value.ToStringNoWarn()),
        };

        EnumListField[0] = value switch
        {
            EInt.Value => EInt.Value,
            _ => throw new Exception(value.ToStringNoWarn()),
        };

        EUInt val = value switch
        {
            EInt.Value => EUInt.Value,
            _ => throw new Exception(value.ToStringNoWarn()),  // check: this line could get warning
                                                               //        * internally, cast operation happens from exception to enum value
        };

        // expect: ToString get warning
        return value switch
        {
            EInt.Value => value.ToString(),
            EInt.Other => value.ToStringNoWarn(),
            _ => throw new Exception(value.ToString())
        };
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

    // expect: warn on 'default'
    static EInt GetEnum(string text, StringComparison other = StringComparison.Ordinal, EInt value = default) => value;
    static TEnum GetGenericEnum<TEnum>(string text, StringComparison other = StringComparison.Ordinal, TEnum value = default)
        where TEnum : Enum => value;

    void BasicTests<TEnum>(EInt value) where TEnum : Enum, new()
    {
        // cast to enum can lead invalid value creation
        _ = (EInt)310;
        _ = (EUInt)(-310 * -1);
        _ = (EULong)ulong.MaxValue;

        // struct creation
        _ = new EInt();
        EInt val1 = new();
        EInt val2 = default;
        EInt val4 = default(EInt);
        _ = new TEnum();
        TEnum gen1 = new();
        TEnum gen2 = default;
        TEnum gen4 = default(TEnum);

        // expect: no warn on method invocation which has generic enum default clause on parameter
        _ = GetEnum("");
        _ = GetGenericEnum<TEnum>("");
        _ = GetGenericEnum<EUShort>("");

        // expect: warn on 'default' clause
        _ = GetEnum("", new(), new());
        _ = GetEnum("", default, default);
        _ = GetEnum("", default(StringComparison), default(EInt));
        _ = GetGenericEnum<TEnum>("", new(), new());
        _ = GetGenericEnum<TEnum>("", default, default);
        _ = GetGenericEnum<TEnum>("", default(StringComparison), default(TEnum));
        _ = GetGenericEnum<EUShort>("", new(), new());
        _ = GetGenericEnum<EUShort>("", default, default);
        _ = GetGenericEnum<EUShort>("", default(StringComparison), default(EUShort));

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
        string concat = "value: " + value;
        string interpolate = $"value: {value} {"" + value + 0} {value.ToString()}" + $@"value: {value}";

        // only allow handling value as is
        _ = EnumConstraintGenericType(value);
    }

    int EnumConstraintGenericType<T>(T value) where T : Enum
    {
        _ = value.ToString();
        _ = value.Equals(null);
        _ = (T)(object)(310 + 310);  // require intermediate cast
        _ = (T)(object)value;
        _ = "value: " + value;
        _ = $"value: {value} {"" + value + 0} {value.ToString()} {(((value)))}";
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
