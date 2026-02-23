[![NuGet](https://img.shields.io/nuget/vpre/SatorImaging.StaticMemberAnalyzer)](https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer)
&nbsp;
[![🇯🇵](https://img.shields.io/badge/🇯🇵-日本語-789)](./README.ja.md)
[![🇨🇳](https://img.shields.io/badge/🇨🇳-简体中文-789)](./README.zh-CN.md)
[![🇺🇸](https://img.shields.io/badge/🇺🇸-English-789)](./README.md)





Roslyn ベースのアナライザーです。静的フィールド/プロパティ初期化やその他の問題を診断します。

- [初期化の不安定性解析](#初期化の不安定性解析) で不安定な初期化を検出
    - 静的フィールド/プロパティ宣言順の誤り
    - partial 型でのファイル跨ぎ参照
    - 型を跨ぐ静的フィールドの [相互参照問題](#相互参照問題)
- [読み取り専用変数解析](#読み取り専用変数解析) でローカル/引数への代入と可変な引数受け渡しを検出
- [`Enum` アナライザーとコード修正プロバイダー](#enum-アナライザーとコード修正プロバイダー) でユーザー側の値変換を禁止し、[Kotlin 風 Enum パターン](#kotlin-風-enum-パターン) も検査
- [Disposable アナライザー](#disposable-アナライザー) で `using` の欠落を検出
- `struct` の引数なしコンストラクター誤用解析
- `TSelf` ジェネリック型引数と型制約の解析
- ファイルヘッダーコメントの強制
- ~~カスタムメッセージでのフィールド/プロパティ等の注釈と下線表示~~

> [!TIP]
> 診断ルール一覧: [**RULES.md**](RULES.md)



## 初期化の不安定性解析

![Analyzer in Action](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/InAction.gif)

## `Enum` 型解析

整数との相互キャストを制限します。ユーザーコードでの enum 値変換を全面的に禁止できます。

![Enum Analyzer](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumAnalyzer.png)

## `TSelf` 型引数解析

CRTP (Curiously Recurring Template Pattern) 向けに `TSelf` 型引数の不一致を解析します。

![TSelf Type Argument](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/GenericTypeArgTSelf.png)



## 型・フィールド・プロパティへの注釈 💯

> [!IMPORTANT]
> Underlining analyzer は廃止扱いです。再度有効化するには、プリプロセッサシンボル `STMG_ENABLE_UNDERLINING_ANALYZER` を設定して再ビルドしてください。


Visual Studio でのコーディング時に注意を引く追加機能です。型/メソッド/フィールド/プロパティへの注釈に `Obsolete` 属性を使う必要がなくなります。

[以下のセクション](#注釈--下線表示) で詳細を確認できます。


![Draw Underline](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/DrawUnderline.png)





&nbsp;

# インストール

- NuGet
	- https://www.nuget.org/packages/SatorImaging.StaticMemberAnalyzer
    - ```
      PM> Install-Package SatorImaging.StaticMemberAnalyzer
      ```


## Visual Studio 2019 以前

このアナライザーは Visual Studio 2022 でテストされています。

旧バージョンの Visual Studio でも利用可能です。その場合は `Vsix` プロジェクトのメモに従って設定を更新し、ビルドしてください。





&nbsp;

# Unity 連携

このアナライザーは Unity 2020.2 以降で利用できます。詳細は次のページを参照してください。

[Unity/README.md](Unity/README.md)





&nbsp;

# 相互参照問題

これは設計上の問題で、複雑さを増やすだけでなく特定条件下でのみ初期化エラーを引き起こします。

一見動いていても、手作業では発見しづらい潜在バグの原因になるため修正が必要です。静的フィールドは初期化失敗を例外として報告しない点にも注意が必要です。


```cs
class A {
    public static int Value = B.Other;
    public static int Other = 310;
}

class B {
    public static int Other = 620;
    public static int Value = A.Other;  // '310' ではなく '0' になる
}

public static class Test
{
    public static void Main()
    {
        System.Console.WriteLine(A.Value);  // 620
        System.Console.WriteLine(A.Other);  // 310
        System.Console.WriteLine(B.Value);  // 0   👈👈👈
        System.Console.WriteLine(B.Other);  // 620

        // メンバーのアクセス順を変えると正しく動作する 🤣
        // 詳細は次項を参照
        //System.Console.WriteLine(B.Value);  // 310  👈 正しい!!
        //System.Console.WriteLine(B.Other);  // 620
        //System.Console.WriteLine(A.Value);  // 620
        //System.Console.WriteLine(A.Other);  // 310
    }
}
```


**C# Compiler Initialization Sequence**

- `A.Value = B.Other;`
    - // `B` の初期化がメンバーアクセスで開始
    - `B.Other = 620;`
    - `B.Value = A.Other;`  // BUG: 未初期化 `A.Other` を読むため 0
    - // その後 `B.Other` の値 620 を `A.Value` に代入
- `A.Other = 310;`  // ここで初期化。B.Value には反映されない


先に B 側を読むと初期化順が変わり、結果も変わります。

- `B.Other = 620;`
- `B.Value = A.Other;`
    - // `A` の初期化がメンバーアクセスで開始
    - `A.Value = B.Other;`  // 正常: `B.Other` は先に初期化済み
    - `A.Other = 310;`





&nbsp;

# `Enum` アナライザーとコード修正プロバイダー

enum の扱いは複雑になりがちです。整数/文字列への変換や文字列からの解析などをユーザーコードで直接行わないようにすると、運用を一元化しやすくなります。

このアナライザーは、アプリ中央の enum ユーティリティへ処理を集約するのに役立ちます。

![Enum Analyzer](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumAnalyzer.png)


## 難読化から `Enum` 型を除外

難読化ツールによる文字列表現の変更を防ぐための注釈とコード修正を提供します。

![Enum Code Fix](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/EnumCodeFix.png)

> [!NOTE]
> `Obfuscation` 属性は C# 標準ライブラリの属性であり、単体で難読化機能を提供するものではありません。対応ツールに設定を伝えるためのものです。


## Kotlin 風 Enum パターン

Kotlin 風 enum class の実装を支援する解析です。

Enum ライク型の要件:
- `MyEnumLike[]` または `ReadOnlyMemory<MyEnumLike>` フィールドが存在
    - フィールド名が `Entries` で始まる (大文字小文字区別) か `entries` で終わる (大文字小文字非区別) 場合、初期化子の妥当性を検査
- 型に `sealed` 修飾子
- コンストラクターは `private` のみ
- `Entries` という名前の `public static` メンバーが存在
- `public bool Equals` を宣言/オーバーライドしない


```cs
public class EnumLike
//           ~~~~~~~~ 警告: sealed 修飾子がなく、公開コンストラクターが存在する
//                          * この警告は 'Entries' メンバーを持つ場合にのみ表示される
{
    public static readonly EnumLike A = new("A");
    public static readonly EnumLike B = new("B");

    public static ReadOnlySpan<EnumLike> Entries => EntriesAsMemory.Span;

    // 'Entries' はすべての 'public static readonly' フィールドを宣言順に保持する必要がある
    static readonly EnumLike[] _entries = new[] { B, A };
    //                                    ~~~~~~~~~~~~~~ 順序が正しくない!!

    // 配列の代わりに 'ReadOnlyMemory<T>' も使用可能
    public static readonly ReadOnlyMemory<EnumLike> EntriesAsMemory = new(new[] { A, B });


    /* ===  Kotlin 風 Enum テンプレート  === */

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


### Enum ライク型の利点

<p><details --open><summary>利点</summary>

Kotlin 風 enum (代数的データ型) は無効値の生成を防ぎやすくします。

```cs
var invalid = Activator.CreateInstance(typeof(EnumLike));

if (EnumLike.A == invalid || EnumLike.B == invalid)
{
    // このコードパスには到達しない
    // 各 Enum ライクエントリはクラスインスタンスであり、ReferenceEquals での一致が必要
}
```


ただし `switch` での利用は少し独特になります。

```cs
var val = EnumLike.A;

switch (val)
{
    // ケースガード付きのパターンマッチング...!!
    case EnumLike when val == EnumLike.A:
        System.Console.WriteLine(val);
        break;

    case EnumLike when val == EnumLike.B:
        System.Console.WriteLine(val);
        break;
}

// このパターンは同じ AOT コンパイル済みコードを生成する
switch (val)
{
    // 型指定なしのケースガード
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

# Disposable アナライザー

```cs
var d = new Disposable();
//      ~~~~~~~~~~~~~~~~ using 文が見つからない

d = (new object()) as IDisposable;
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Disposable 型への/からのキャスト
```


次の条件では警告を出しません:
- `return` 文でインスタンスを生成
    - `return new Disposable();`
- フィールド/プロパティへの代入
    - `m_field = new Disposable();`
- `IDisposable` 型同士のキャスト
    - `var x = myDisposable as IDisposable;`



## `Disposable` 解析の抑制

特定型の解析を抑制するには、`DisposableAnalyzerSuppressor` という属性を定義し、アセンブリに付与します。

```cs
[assembly: DisposableAnalyzerSuppressor(typeof(Task), typeof(Task<>))]  // Task と Task<T> はデフォルトで無視される

[Conditional("DEBUG"), AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
sealed class DisposableAnalyzerSuppressor : Attribute
{
    public DisposableAnalyzerSuppressor(params Type[] _) { }
}
```





&nbsp;

# 読み取り専用変数解析

このアナライザーは、書き込み操作を検出してローカル値/引数の不変性維持を支援します。

- 代入
    - `=`
    - `??=`
    - `= ref`
    - 分解代入: `(x, y) = ...` / `(x, var y) = ...`
        - 分解「宣言」代入は許可: `var (x, y) = ...`
    - *注*: メソッド `out` 引数への代入は常に許可
- インクリメント/デクリメント
    - `++x`, `x++`, `--x`, `x--`
- ループヘッダーの特殊な扱い
    - 許可: `for` ループのヘッダー内での代入とインクリメント/デクリメント
    - 許可: `while` ループの条件式内での単純代入
- 複合代入
    - `+=`, `-=`, `*=`, `/=`, `%=`
    - `&=`, `|=`, `^=`, `<<=`, `>>=`
- 引数処理
    - 許可: メソッド呼び出し/オブジェクト生成 (例: `Use(Create())`, `Use(new C())`)
    - 許可: 匿名オブジェクト/配列生成 (例: `Use(new { X = 1 })`, `Use(new[] { 1, 2 })`)
    - 許可: 呼び出し側 `out var x` / `out T x` 宣言
    - 許可: ルートローカル/引数名が `mut_` で始まる
    - 型チェック (`string` は読み取り専用 struct 相当として扱う)
        - 参照型引数 (`string` 以外) は常に報告
        - struct 引数:
            - 許可: 呼び出し先引数が `in`
            - 許可: 呼び出し先引数に修飾子なし かつ struct が `readonly`
            - それ以外は報告


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
        result = 0;  // 許可: out 引数への代入

        param += 1;      // 報告: 引数への代入
        mut_param += 1;  // 許可: 引数名が mut_ で始まっている

        int foo = 0;
        foo = 1;     // 報告: ローカル変数への代入
        foo++;       // 報告: ローカル変数のインクリメント

        var (x, y) = (42, 310);  // 許可: var (...) は許可される
        (x, y) = (42, 310);      // 報告: 分解代入
        (x, var z) = (42, 310);  // 報告: 混在した分解はエラーを引き起こす
                                    //           Unity との互換性のため、var z もエラーになる

        // 許可: for ヘッダー内での代入
        int i;
        for (i = 0; i < 10; i++)
        {
            i += 0;  // 報告: for ヘッダー外
        }

        // 許可: while ヘッダー内での代入
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            read = 0;  // 報告: while ヘッダー外
        }

        int.TryParse("1", out var parsed);  // 許可: 呼び出し先での out 宣言
        int.TryParse("1", out parsed);      // 報告: out が変数を上書きしている

        int.TryParse("1", out var mut_parsed);
        int.TryParse("1", out mut_parsed);  // 許可: mut_ プレフィックス

        int mut_counter = 0;
        mut_counter = 1;  // 許可: mut_ プレフィックス

        string key = "A";
        object keyObj = new object();
        var indexer = new Demo();
        _ = indexer[key];     // 許可: string は読み取り専用構造体として扱われる
        _ = indexer[keyObj];  // 報告: 参照型のインデクサーキー
        indexer = new();      // 報告: ローカル変数への代入（参照型）

        UseIn(s);                  // 許可: 呼び出し先の引数が in 修飾子付き
        UseReadOnly(rs);           // 許可: 修飾子なしの読み取り専用構造体
        UseRefType(Create());      // 許可: 引数がメソッド呼び出し
        UseRefType(new object());  // 許可: 引数がオブジェクト生成
    }
}
```

> [!NOTE]
> ローカル/引数をルートにしたメンバー代入 (例: `foo.Bar.Value = 1` の `foo`) は報告対象です。フィールドをルートにした場合は報告しません。


## カテゴリによる解析の抑制

カテゴリ全体で解析を抑制するには、`.editorconfig` ファイルに以下を追加します。

```
[*.cs]
dotnet_analyzer_diagnostic.category-ImmutableVariable.severity = silent
```





&nbsp;

# 注釈 / 下線表示

> [!IMPORTANT]
> Underlining analyzer は廃止扱いです。再度有効化するには、プリプロセッサシンボル `STMG_ENABLE_UNDERLINING_ANALYZER` を設定して再ビルドしてください。


型、フィールド、プロパティ、ジェネリック型/メソッド引数、メソッド/デリゲート/ラムダ引数に下線を描画するオプション機能です。

Visual Studio の仕様上、`Info` 重要度の下線は先頭数文字にしか描画されない場合があります。その回避として、キーワード上の下線は破線で描画されます。


![Draw Underline](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/DrawUnderline.png)

> [!TIP]
> メッセージを `!` で始めると、info ではなく warning 注釈としてキーワードに表示します。


## 使い方

このアナライザーへの依存を避けるため、下線用属性には組み込みの `System.ComponentModel` を利用します。そのため記法はやや独特です。

解析は C# の実型ではなく、ソース上の識別子キーワードを見ます。下線描画対象として認識されるのは C# 属性構文での `DescriptionAttribute` だけです。`Attribute` の省略や名前空間付き指定は認識されません。


> [!TIP]
> `CategoryAttribute` can be used instead of `DescriptionAttribute`.
>
> Description と異なり、`CategoryAttribute` は厳密な型参照とコンストラクター (`base()`) のみに下線を描画します。継承型・変数・フィールド・プロパティには適用されません。


```cs
using System.ComponentModel;

[DescriptionAttribute("Draw underline for IDE environment and show this message")]
//          ^^^^^^^^^ 下線を描画するには Attribute サフィックスが必要
public class WithUnderline
{
    [DescriptionAttribute]  // 引数なしはデフォルトメッセージで下線を描画する
    public static void Method() { }
}

// C# 仕様では Attribute サフィックスを省略できるが、省略すると下線は描画されない
// VS フォームデザイナー向けの元々の用途との競合を避けるため
[Description("No Underline")]
public class NoUnderline { }

// 名前空間が指定されている場合、下線は描画されない
[System.ComponentModel.DescriptionAttribute("...")]
public static int Underline_Not_Drawn = 0;

// このコードは下線を描画する。属性構文に 'Trivia' を含めることが可能
[ /**/  DescriptionAttribute   (   "Underline will be drawn" )   /* hello, world. */   ]
public static int Underline_Drawn = 310;
```



## 詳細度の制御

下線には 4 種類あります: line head, line leading, line end, keyword。

デフォルトでは静的フィールドアナライザーが最も詳細な下線を描画します。
`#pragma` プリプロセッサディレクティブや `SuppressMessage` 属性などで特定種類の下線を抑制できます。


![Verbosity Control](https://raw.githubusercontent.com/sator-imaging/CSharp-StaticFieldAnalyzer/main/assets/VerbosityControl.png)



## Unity 向けヒント

下線表示は、Visual Studio のビジュアルデザイナー (旧 Form Designer) 向けの [Description](https://learn.microsoft.com/dotnet/api/system.componentmodel.descriptionattribute) 属性を使って実現しています。

Unity ビルドから不要属性を除去するには、Unity プロジェクトの `Assets` フォルダーに次の `link.xml` を追加してください。

```xml
<linker>
    <assembly fullname="System.ComponentModel">
        <type fullname="System.ComponentModel.DescriptionAttribute" preserve="nothing"/>
    </assembly>
</linker>
```





&nbsp;

# TODO

## Disposable アナライザー

### 既知の誤検出

- ラムダの `return` 文
    - `MethodArg(() => DisposableProperty);`
    - `MethodArg(() => { return DisposableProperty; });`
- `?:` 演算子
    - `DisposableProperty = condition ? null : disposableList[index];` 


## Enum アナライザー機能
- 暗黙的キャスト抑制属性
    - `[assembly: EnumAnalyzer(SuppressImplicitCast = true)]`
        - `object` `Enum` `string` `int` や他の blittable 型へのキャストは***抑制しないこと***
        - （暗黙的キャスト演算子は多くの場合で設計意図があるため、既定で抑制すべき？）
- Enum ライク型で internal 専用エントリを許可
  ```cs
  sealed class MyEnumLike
  {
      public static readonly MyEnumLike PublicEntry = new();
      internal static readonly MyEnumLike ForDebuggingPurpose = new();
  }
  ```
