[![NuGet](https://img.shields.io/nuget/vpre/SatorImaging.StaticMemberAnalyzer)](https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer)
&nbsp;
[![🇯🇵](https://img.shields.io/badge/🇯🇵-日本語-789)](./README.ja.md)
[![🇨🇳](https://img.shields.io/badge/🇨🇳-简体中文-789)](./README.zh-CN.md)
[![🇺🇸](https://img.shields.io/badge/🇺🇸-English-789)](./README.md)





基于 Roslyn 的分析器，用于诊断静态字段/属性初始化以及其他问题。

- [不稳定初始化分析](#不稳定初始化分析) 检测不稳定初始化
    - 静态字段与属性声明顺序错误
    - partial 类型跨文件成员引用
    - 跨类型静态字段的 [交叉引用问题](#交叉引用问题)
- [只读变量分析](#只读变量分析) 检测对局部变量/参数赋值，以及可变参数传递
- [`Enum` 分析器与代码修复提供程序](#enum-分析器与代码修复提供程序) 防止用户层面的值转换，并支持 [Kotlin 风格 Enum 模式](#kotlin-风格-enum-模式)
- [Disposable 分析器](#disposable-分析器) 检测缺少 `using` 语句
- `struct` 无参构造函数误用分析
- `TSelf` 泛型类型参数与类型约束分析
- 文件头注释强制规则
- ~~对字段/属性等进行自定义消息标注与下划线~~

> [!TIP]
> 查看全部诊断规则: [**RULES.md**](RULES.md)



## 不稳定初始化分析

![Analyzer in Action](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/InAction.gif)

## `Enum` 类型分析

限制与整数之间的双向转换，彻底禁止用户代码直接进行 enum 值转换。

![Enum Analyzer](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumAnalyzer.png)

## `TSelf` 类型参数分析

用于分析 CRTP（Curiously Recurring Template Pattern）中 `TSelf` 类型参数不匹配问题。

![TSelf Type Argument](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/GenericTypeArgTSelf.png)



## 类型、字段与属性标注 💯

> [!IMPORTANT]
> Underlining analyzer 已废弃。如需重新启用，请设置预处理符号 `STMG_ENABLE_UNDERLINING_ANALYZER` 并重新构建。


这是一个在 Visual Studio 编码时用于增强提示的附加功能。你不再需要通过 `Obsolete` 属性来标注类型、方法、字段和属性。

详见 [该章节](#标注--下划线)。


![Draw Underline](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/DrawUnderline.png)





&nbsp;

# 安装

- NuGet
	- https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer
    - ```
      PM> Install-Package SatorImaging.StaticMemberAnalyzer
      ```


## Visual Studio 2019 或更早版本

该分析器在 Visual Studio 2022 上已测试。

你也可以在更早版本的 Visual Studio 中使用。请按项目中的说明更新 `Vsix` 项目文件后再构建。





&nbsp;

# Unity 集成

该分析器可用于 Unity 2020.2 及以上版本，详见：

[Unity/README.md](Unity/README.md)





&nbsp;

# 交叉引用问题

这是一个设计层面的问题，会让初始化逻辑变得复杂，并且只在特定条件下触发初始化错误。

即使当前看起来运行正常，也必须修复，以避免在大型代码库中难以手工发现的潜在问题。静态字段初始化失败通常不会直接抛出易见错误。


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


**C# 编译器初始化顺序**

- `A.Value = B.Other;`
    - // 访问成员触发 `B` 初始化
    - `B.Other = 620;`
    - `B.Value = A.Other;`  // BUG: 读取未初始化 `A.Other`，结果为 0
    - // 然后把 `B.Other` 的值 620 赋给 `A.Value`
- `A.Other = 310;`  // 在这里才初始化，这个值不会回填到 B.Value


如果先读取 B 侧，初始化顺序会改变，结果也会随之变化。

- `B.Other = 620;`
- `B.Value = A.Other;`
    - // 访问成员触发 `A` 初始化
    - `A.Value = B.Other;`  // 正确: `B.Other` 已先初始化
    - `A.Other = 310;`





&nbsp;

# `Enum` 分析器与代码修复提供程序

enum 的处理很容易变得混乱。通常应避免在业务代码中直接做与整数/字符串之间的转换与解析。

该分析器可帮助你将 enum 处理集中并封装到统一的工具层中。

![Enum Analyzer](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumAnalyzer.png)


## 从混淆中排除 `Enum` 类型

提供注解与代码修复，避免混淆工具修改 enum 的字符串表示。

![Enum Code Fix](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumCodeFix.png)

> [!NOTE]
> `Obfuscation` 属性来自 C# 基础库，本身不提供混淆功能。它只是向识别该属性的混淆工具传递配置。


## Kotlin 风格 Enum 模式

用于辅助实现 Kotlin 风格的 enum class 模式。

类 enum 类型要求：
- 存在 `MyEnumLike[]` 或 `ReadOnlyMemory<MyEnumLike>` 字段
    - 字段名以 `Entries` 开头（区分大小写）或以 `entries` 结尾（不区分大小写）时，会检查初始化器正确性
- 类型带 `sealed` 修饰符
- 仅允许 `private` 构造函数
- 存在名为 `Entries` 的 `public static` 成员
- 不应声明/重写 `public bool Equals`


```cs
public class EnumLike
//           ~~~~~~~~ WARN: no `sealed` modifier on type and public constructor exists
//                          * this warning appears only if type has member called 'Entries'
{
    public static readonly EnumLike A = new("A");
    public static readonly EnumLike B = new("B");

    public static ReadOnlySpan<EnumLike> Entries => EntriesAsMemory.Span;

    // 'Entries' must have all of 'public static readonly' fields in declared order
    static readonly EnumLike[] _entries = new[] { B, A };
    //                                    ~~~~~~~~~~~~~~ wrong order!!

    // 'ReadOnlyMemory<T>' can be used instead of array
    public static readonly ReadOnlyMemory<EnumLike> EntriesAsMemory = new(new[] { A, B });


    /* ===  Kotlin style enum template  === */

    static int AUTO_INCREMENT = 0;  // iota

    public readonly int Ordinal;
    public readonly string Name;

    private EnumLike(string name) { Ordinal = AUTO_INCREMENT++; Name = name; }

    public override string ToString()
    {
        const string SEP = ": ";
        Span<char> span = stackalloc char[Name.Length + 11 + SEP.Length];  // 11 for int.MinValue.ToString().Length

        Ordinal.TryFormat(span, out var written);
        SEP.AsSpan().CopyTo(span.Slice(written));
        written += SEP.Length;
        Name.AsSpan().CopyTo(span.Slice(written));
        written += Name.Length;

        return span.Slice(0, written).ToString();
    }
}
```


### 类 Enum 类型的优势

<p><details --open><summary>优势</summary>

Kotlin 风格 enum（代数数据类型）可以防止无效值被创建。

```cs
var invalid = Activator.CreateInstance(typeof(EnumLike));

if (EnumLike.A == invalid || EnumLike.B == invalid)
{
    // this code path won't be reached
    // each enum like entry is a class instance and ReferenceEquals match required
}
```


不过在 `switch` 中使用会稍显别扭。

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

<!------- End of Details Tag -------></details></p>





&nbsp;

# Disposable 分析器

```cs
var d = new Disposable();
//      ~~~~~~~~~~~~~~~~ no `using` statement found

d = (new object()) as IDisposable;
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ cast from/to disposable
```


以下情况不会报警：
- 在 `return` 语句中创建实例
    - `return new Disposable();`
- 赋值给字段或属性
    - `m_field = new Disposable();`
- 在可释放类型之间转换
    - `var x = myDisposable as IDisposable;`



## 抑制 `Disposable` 分析

若需对指定类型抑制分析，声明名为 `DisposableAnalyzerSuppressor` 的特性并加到程序集上。

```cs
[assembly: DisposableAnalyzerSuppressor(typeof(Task), typeof(Task<>))]  // Task and Task<T> are ignored by default

[Conditional("DEBUG"), AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
sealed class DisposableAnalyzerSuppressor : Attribute
{
    public DisposableAnalyzerSuppressor(params Type[] _) { }
}
```





&nbsp;

# 只读变量分析

该分析器通过标记写操作，帮助保持局部变量和参数的不可变性。

- 赋值
    - `=`
    - `??=`
    - `= ref`
    - 解构赋值: `(x, y) = ...` / `(x, var y) = ...`
        - 允许解构声明赋值: `var (x, y) = ...`
    - *注*: 对 `out` 参数赋值始终允许
- 自增/自减
    - `++x`, `x++`, `--x`, `x--`
- 复合赋值
    - `+=`, `-=`, `*=`, `/=`, `%=`
    - `&=`, `|=`, `^=`, `<<=`, `>>=`
- 参数处理
    - 允许: 方法调用和对象创建（如 `Use(Create())`, `Use(new C())`）
    - 允许: 匿名对象和数组创建（如 `Use(new { X = 1 })`, `Use(new[] { 1, 2 })`）
    - 允许: 调用点 `out var x` / `out T x` 声明
    - 允许: 根局部变量/参数名以 `mut_` 开头
    - 类型检查（`string` 按只读 struct 处理）
        - 引用类型参数（除 `string` 外）总是报告
        - struct 参数:
            - 允许: 被调用参数带 `in`
            - 允许: 被调用参数无修饰符且 struct 为 `readonly`
            - 否则报告


```cs
class Demo
{
    readonly struct ReadOnlyS { }
    struct MutableS { }

    static object Create() => new object();
    static void UseRefType(object value) { }
    static void UseIn(in MutableS value) { }
    static void UseReadOnly(ReadOnlyS value) { }
    public int this[string key] => 0;
    public int this[object key] => 0;

    void Test(
        int param,
        int mut_param,
        MutableS s,
        ReadOnlyS rs,
        ref int refValue,
        out int result
    )
    {
        result = 0;  // Allowed: assignment to `out` parameter

        param += 1;      // Reported: parameter assignment
        mut_param += 1;  // Allowed: `mut_` prefix on parameter

        int foo = 0;
        foo = 1;     // Reported: local assignment
        foo++;       // Reported: local increment

        var (x, y) = (42, 310);  // Allowed: var (...) is allowed
        (x, y) = (42, 310);      // Reported: deconstruction assignment
        (x, var z) = (42, 310);  // Reported: mixed deconstruction causes error
                                    //           For Unity compatibility, `var z` also get error

        // Allowed: assignment in for-header
        int i;
        for (i = 0; i < 10; i++)
        {
            i += 0;  // Reported: not in for-header
        }

        // Allowed: assignment in while-header
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            // read = 0;  // Reported: not in while-header
        }

        int.TryParse("1", out var parsed);  // Allowed: out declaration at call site
        int.TryParse("1", out parsed);      // Reported: out overwrites variable

        int.TryParse("1", out var mut_parsed);
        int.TryParse("1", out mut_parsed);  // Allowed: `mut_` prefix

        int mut_counter = 0;
        mut_counter = 1;  // Allowed: `mut_` prefix

        string key = "A";
        object keyObj = new object();
        var indexer = new Demo();
        _ = indexer[key];     // Allowed: string is treated readonly-struct
        _ = indexer[keyObj];  // Reported: reference type indexer key
        indexer = new();      // Reported: local assignment (reference type)

        UseIn(s);                  // Allowed: callee parameter is `in`
        UseReadOnly(rs);           // Allowed: readonly struct with no modifier
        UseRefType(Create());      // Allowed: argument value is invocation
        UseRefType(new object());  // Allowed: argument value is object creation
    }
}
```

> [!NOTE]
> 当赋值根节点是局部变量/参数时会被报告（例如 `foo.Bar.Value = 1` 中的 `foo`）。根节点是字段时不会报告。





&nbsp;

# 标注 / 下划线

> [!IMPORTANT]
> Underlining analyzer 已废弃。如需重新启用，请设置预处理符号 `STMG_ENABLE_UNDERLINING_ANALYZER` 并重新构建。


这是一个可选功能，可在类型、字段、属性、泛型类型/方法参数，以及方法/委托/Lambda 参数上绘制下划线。

由于 Visual Studio 的 UX 设计，`Info` 级别诊断下划线通常只显示在前几个字符上，而不是整个标记区域。为规避此问题，关键字处会绘制虚线下划线。


![Draw Underline](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/DrawUnderline.png)

> [!TIP]
> 消息以 `!` 开头时，会在关键字上添加 warning 标注，而不是 info 标注。


## 使用方法

为避免对该分析器产生依赖，下划线功能所需特性选用了内置的 `System.ComponentModel`，因此语法看起来会有些特殊。

分析器检查的是 C# 源码中的关键字标识，而非真实类型。只有在 C# 特性语法中使用 `DescriptionAttribute` 才会触发下划线。省略 `Attribute` 后缀或添加命名空间都不会被识别。


> [!TIP]
> `CategoryAttribute` can be used instead of `DescriptionAttribute`.
>
> 与 Description 不同，`CategoryAttribute` 只会在精确类型引用和构造函数（含 `base()`）上绘制下划线。继承类型、变量、字段和属性不会绘制。


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



## 详细级别控制

下划线共有 4 类：line head、line leading、line end 和 keyword。

默认情况下，静态字段分析器会绘制最详细的下划线。
你可以通过 `#pragma` 预处理指令、`SuppressMessage` 特性等方式屏蔽指定类型的下划线。


![Verbosity Control](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/VerbosityControl.png)



## Unity 提示

下划线功能基于 [Description](https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute) 特性实现，该特性原本用于 Visual Studio 的可视化设计器（旧称 Form Designer）。

若要从 Unity 构建中移除不必要特性，请在 Unity 项目的 `Assets` 目录添加如下 `link.xml`：

```xml
<linker>
    <assembly fullname="System.ComponentModel">
        <type fullname="System.ComponentModel.DescriptionAttribute" preserve="nothing"/>
    </assembly>
</linker>
```





&nbsp;

# TODO

## Disposable 分析器

### 已知误检

- Lambda `return` 语句
    - `MethodArg(() => DisposableProperty);`
    - `MethodArg(() => { return DisposableProperty; });`
- `?:` 运算符
    - `DisposableProperty = condition ? null : disposableList[index];` 


## Enum 分析器功能
- 隐式转换抑制特性
    - `[assembly: EnumAnalyzer(SuppressImplicitCast = true)]`
        - ***不要*** 抑制转换到 `object` `Enum` `string` `int` 或其他 blittable 类型
        - （隐式转换运算符在大多数场景是有设计意图的，是否应默认抑制？）
- 允许类 Enum 类型存在仅 internal 的条目
  ```cs
  sealed class MyEnumLike
  {
      public static readonly MyEnumLike PublicEntry = new();
      internal static readonly MyEnumLike ForDebuggingPurpose = new();
  }
  ```
