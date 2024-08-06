# Static Field Analyzer for C# / .NET

[![NuGet](https://img.shields.io/nuget/v/SatorImaging.StaticMemberAnalyzer)](https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer)

Roslyn-based analyzer to provide diagnostics of static fields and properties initialization and more.

- Wrong order of static field and property declaration
- Partial type member reference across files
- [Cross-Referencing Problem](#cross-referencing-problem) of static field across type
- `TSelf` generic type argument analysis
- Annotating / Underlining field, property or etc with custom message


![Analyzer in Action](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/InAction.gif)

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
    - `B.Value = A.Other;`  // B.Value will be 0 because reading uninitialized `A.Other`
    - // then, assign `B.Other` value (620) to `A.Value`
- `A.Other = 310;`  // initialized here!! this value is not assigned to B.Value


When reading B value first, initialization order is changed and resulting value is also changed accordingly:

- `B.Other = 620;`
- `B.Value = A.Other;`
    - // 'A' initialization is started by member access
    - `A.Value = B.Other;`  // correct: B.Other is initialized before reading value
    - `A.Other = 310;`





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
