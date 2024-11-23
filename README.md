# Static Field Analyzer for C# / .NET

[![NuGet](https://img.shields.io/nuget/v/SatorImaging.StaticMemberAnalyzer)](https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer)
<sup>[Devnote](#devnote)</sup>

Roslyn-based analyzer to provide diagnostics of static fields and properties initialization and more.

- Wrong order of static field and property declaration
- Partial type member reference across files
- [Cross-Referencing Problem](#cross-referencing-problem) of static field across type
- [`Enum` type analysis](#enum-analyzer-and-code-fix-provider) to prevent user-level value conversion & [more](#kotlin-like-enum-pattern)
- `struct` parameter-less constructor misuse analysis
- `Disposable` missing using statement analysis
- `TSelf` generic type argument & type constraint analysis
- Annotating and underlining field, property or etc with custom message
- Find out all diagnostic rules: [RULES.md](RULES.md)


## Static Field Analysis

![Analyzer in Action](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/InAction.gif)

## Enum Type Analysis

Restrict both cast from/to integer number! Disallow user-level enum value conversion completely!!

![Enum Analyzer](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumAnalyzer.png)

## `TSelf` Type Argument Analysis

Analyze `TSelf` type argument mismatch and `where` clause mismatch.

![TSelf Type Argument](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/GenericTypeArgTSelf.png)



## Annotation for Type, Field and Property 💯

There is fancy extra feature to take your attention while coding in Visual Studio. No more need to use `Obsolete` attribute in case of annotating types, methods, fields and properties.

See [the following section](#annotating--underlining) for details.


![Draw Underline](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/DrawUnderline.png)





# Installation

- NuGet
	- https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer
    - ```
      PM> Install-Package SatorImaging.StaticMemberAnalyzer
      ```


## Visual Studio 2019 or Earlier

Analyzer is tested on Visual Studio 2022.

You could use this analyzer on older versions of Visual Studio. To do so, update `Vsix` project file by following instructions written in memo and build project.





# Unity Integration

This analyzer can be used with Unity 2020.2 or above. See the following page for detail.

[SatorImaging.StaticMemberAnalyzer.Unity/](SatorImaging.StaticMemberAnalyzer.Unity)





# Cross-Referencing Problem

It is a design bug makes all things complex. Not only that but also it causes initialization error only when meet a specific condition.

So it must be fixed even if app works correctly at a moment, to prevent simple but complicated potential bug which is hard to find in large code base by hand. As you know static fields will never report error when initialization failed!!


```cs
class A {
    public static int Value = B.Other;
    public static int Other = 310;
}

class B {
    public static int Other = 620;
    public static int Value = A.Other;  // will be '0' not '310'
}

public static class Test
{
    public static void Main()
    {
        System.Console.WriteLine(A.Value);  // 620
        System.Console.WriteLine(A.Other);  // 310
        System.Console.WriteLine(B.Value);  // 0   👈👈👈
        System.Console.WriteLine(B.Other);  // 620

        // when changing class member access order, it works correctly 🤣
        // see the following section for detailed explanation
        //System.Console.WriteLine(B.Value);  // 310  👈 correct!!
        //System.Console.WriteLine(B.Other);  // 620
        //System.Console.WriteLine(A.Value);  // 620
        //System.Console.WriteLine(A.Other);  // 310
    }
}
```


**C# Compiler Initialization Sequence**

- `A.Value = B.Other;`
    - // 'B' initialization is started by member access
    - `B.Other = 620;`
    - `B.Value = A.Other;`  // BUG: B.Value will be 0 because reading uninitialized `A.Other`
    - // then, assign `B.Other` value (620) to `A.Value`
- `A.Other = 310;`  // initialized here!! this value is not assigned to B.Value


When reading B value first, initialization order is changed and resulting value is also changed accordingly:

- `B.Other = 620;`
- `B.Value = A.Other;`
    - // 'A' initialization is started by member access
    - `A.Value = B.Other;`  // correct: B.Other is initialized before reading value
    - `A.Other = 310;`





# `Enum` Analyzer and Code Fix Provider

Enum type handling is really headaching. To make enum operation under control, good to avoid user-level enum handling such as converting to integer or string, parse from string and etc.

This analyzer will help centerizing and encapsulating enum handling in app's central enum utility.

![Enum Analyzer](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumAnalyzer.png)


## Excluding Enum Type from Obfuscation

Helpful annotation and code fix for enum types to prevent modification of string representation by obfuscation tool.

![Enum Code Fix](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumCodeFix.png)

> [!NOTE]
> `Obfuscation` attribute is from C# base library and it does NOT provide feature to obfuscate compiled assembly. It just provides configuration option to obfuscation tools which recognizing this attribute.


## Kotlin-like Enum Pattern

Analysis to help implementing Kotlin-style enum class.

```cs
public class EnumLike
//           ^^^^^^^^ should have `sealed` modifier and constructor should
//                    be `private` or `protected`
//                    * annotation appears only if type has 'Entries' field
{
    public static readonly EnumLike A = new();
    public static readonly EnumLike B = new();

    // 'Entries' should have all of 'public static readonly' field of declaring type
    public static readonly EnumLike[] Entries; //= new[] { A, B };
    //                                ~~~~~~~

    // 'ReadOnlyMemory<T>' can be used instead of array
    public static readonly ReadOnlyMemory<EnumLike> Entries = new(new[] { A, B });
}
```


<p><details lang="en" --open><summary>Benefits</summary>

Kotlin-like enum (algebraic data type) can prevent invalid value creation.

```cs
var invalid = Activator.CreateInstance(typeof(EnumLike));

if (EnumLike.A == invalid || EnumLike.B == invalid)
{
    // this code path won't be reached
    // each enum like entry is a class instance and ReferenceEquals match required
}
```


Unfortunately, use in `switch` statement is a bit weird.

```cs
var val = EnumLike.A;

switch (val)
{
    // pattern matching with case guard...!!
    case EnumLike when val == EnumLike.A:
        System.Console.WriteLine(val);
        break;

    case EnumLike when val == EnumLike.B:
        System.Console.WriteLine(val);
        break;
}

// this pattern generates same AOT compiled code
switch (val)
{
    // typeless case guard
    case {} when val == EnumLike.A:
        System.Console.WriteLine(val);
        break;

    case {} when val == EnumLike.B:
        System.Console.WriteLine(val);
        break;
}
```

<!------- End of Details EN Tag -------></details></p>





# Annotating / Underlining

There is optional feature to draw underline on selected types, fields, properties, generic type/method arguments and parameters of method, delegate and lambda function.

As of Visual Studio's UX design, `Info` severity diagnostic underlines are drawn only on a few leading chars, not drawn whole marked area. So for workaround, underline on keyword is dashed.


![Draw Underline](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/DrawUnderline.png)

> [!TIP]
> `!`-starting message will add warning annotation on keyword instead of info diagnostic annotation.


## How to Use

To avoid dependency to this analyzer, required attribute for underlining is chosen from builtin `System.ComponentModel` assembly so that syntax is little bit weird.

Analyzer is checking identifier keyword in C# source code, not checking actual C# type. `DescriptionAttribute` in C# attribute syntax is the only keyword to draw underline. Omitting `Attribute` or adding namespace are not recognized.

```cs
using System.ComponentModel;

[DescriptionAttribute("Draw underline for IDE environment and show this message")]
//          ^^^^^^^^^ `Attribute` suffix is required to draw underline
public class WithUnderline
{
    [DescriptionAttribute]  // parameter-less will draw underline with default message
    public static void Method() { }
}

// C# language spec allows to omit `Attribute` suffix but when omitted, underline won't be drawn
// to avoid conflict with originally designed usage for VS form designer
[Description("No Underline")]
public class NoUnderline { }

// underline won't be drawn when namespace is specified
[System.ComponentModel.DescriptionAttribute("...")]
public static int Underline_Not_Drawn = 0;

// this code will draw underline. 'Trivia' is allowed to being added in attribute syntax
[ /**/  DescriptionAttribute   (   "Underline will be drawn" )   /* hello, world. */   ]
public static int Underline_Drawn = 310;
```

> [!TIP]
> `CategoryAttribute` can be used instead of `DescriptionAttribute`. It will draw underline only on type which has Category attribute. ie. inherited type won't get underline.


## Verbosity Control

There are 4 types of underline, line head, line leading, line end and keyword.

By default, static field analyzer will draw most verbose underline.
You can omit specific type of underline by using `#pragma` preprocessor directive or adding `SuppressMessage` attribute or etc.


![Verbosity Control](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/VerbosityControl.png)



## Unity Tips

Underlining is achieved by using [Description](https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute) attribute designed for Visual Studio's visual designer, formerly known as form designer.

To remove unnecessary attribute from Unity build, add the following `link.xml` file in Unity project's `Assets` folder.

```xml
<linker>
    <assembly fullname="System.ComponentModel">
        <type fullname="System.ComponentModel.DescriptionAttribute" preserve="nothing"/>
    </assembly>
</linker>
```



&nbsp;  
&nbsp;  

# Devnote

Steps to publish new version of nuget package
- update nuget package version in `.props`
- upload source code to github
- run build action for test
- merge pull request sent from build action
- create github release
- run nuget packaging action to push new version


## TODO

This case will lead invalid initialization but cannot be detected.

```cs
// NG: auto getter/setter
static int BEFORE { get; set; } = AFTER;
static int AFTER { get; set; } = 310;

// OK: can detect
static int BEFORE { get; set; } = AFTER;
static int AFTER = 310;
```

### Optimization

- Implement `IViewTaggerProvider` for underlining analyzer.
- Separate analyzer method as possible. registering small actions instead of one god method could improve performance.





<!--

&nbsp;  
&nbsp;  


# Off-topic: Why not `const`?

## Effective C#

In Effective C#, it describes that runtime constant `readonly static` is better than compile-time constant `const`.

For example, when there are 2 libralies, MyLib.dll and External.dll
- External.dll has public constant value `10.1f`
- MyLib.dll read that value and compiled as managed assembly
- Then, replacing External.dll which has updated constant value `20.2f`

In this case, MyLib.dll will continue to use it's compile-time constant value `10.1f` read from old External.dll, until it is recompiled.

> ie. constant values are "burned" into compiled assembly.


So, using runtime constant is better than `const` in shared libraries.



## `const string` can be easily listed up

When you store your api end point (costs each access) or api key or something secret as `const string`, those are easily retrieved by `strings YourApp.exe` command, or by C# decompilers when compiled as managed code assembly.

Of course using `readonly static string` won't solve the problem perfectly, but worth to consider use to obfuscate secrets keys/values.

-->
