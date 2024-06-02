# Static Field Analyzer for C# / .NET

Roslyn-based analyzer to provide diagnostics of static fields and properties initialization.

- Wrong order of static field and property declaration
- [Cross-Referencing Problem](#cross-referencing-problem) of static field across type


![Analyzer in Action](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/InAction.gif)





# Installation

- NuGet
	- https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer/
- Visual Studio Marketplace
	- Install as an extension per user local machine
	- https://marketplace.visualstudio.com/items?itemName=sator-imaging.StaticMemberAnalyzer





# Unity Integration

This analyzer can be used with Unity 2020.2 or above. See the following page for detail.

https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer/tree/main/unity





# Visual Studio 2019 or Earlier

Analyzer is tested on Visual Studio 2022.

You could use this analyzer on older versions of Visual Studio. To do so, update `Vsix` project file by following instructions written in memo and build project.





# Cross-Referencing Problem

It is a design bug makes all things complex. Not only that but also it causes initialization error only when meet a specific condition.

So it must be fixed even if app works correctly at a moment, to prevent simple but complicated bug which is hard to find in large code base by hand. As you know static fields will never report error when initialization failed!!

> It could be happened when reordering field declarations on refactoring large code base.


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
        // see the following for details
		//System.Console.WriteLine(B.Value);  // 310  👈 correct!!
		//System.Console.WriteLine(B.Other);  // 620
		//System.Console.WriteLine(A.Value);  // 620
		//System.Console.WriteLine(A.Other);  // 310
	}
}
```


**C# Compiler Initialization Logic**

- `A.Value = B.Other;`
    - // 'B' initialization is started by member access
    - `B.Value = A.Other;`  // B.Value will be 0 because reading uninitialized `A.Other`
    - `B.Other = 620;`
    - // then, assign `B.Other` value (620) to `A.Value`
- `A.Other = 310;`  // initialized here!! this value is not assigned to B.Value





<!--
# Why not `const`?

## Effective C#

In Effective C#, it describes that runtime constant `readonly static` is better than compile-time constant `const`.

For example, when there are 2 libralies, MyLib.dll and External.dll
- External.dll has public constant `10.1f`
- MyLib.dll read that value and compiled to managed assembly
- Then, replacing External.dll which has updated constant value `20.2f`

In this case, MyLib.dll will continue to use it's compile-time constant value `10.1f` until it is recompiled.

> ie. constant values are "burned" into compiled assembly.


## `const string` can easily be listed up

When you store your api end point (costs each access) or api key or something secret as `const string`, those are easily retrieved by `strings YourApp.exe` command.

Of course using `readonly static string` won't solve the problem perfectly, but worth to consider use.
-->
