﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SatorImaging.StaticMemberAnalyzer.Analysis {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SatorImaging.StaticMemberAnalyzer.Analysis.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Static Field Analysis に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0001__MD_TITLE__ {
            get {
                return ResourceManager.GetString("SMA0001__MD_TITLE__", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Static field declaration order is wrong. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0001_Description {
            get {
                return ResourceManager.GetString("SMA0001_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Reading &apos;{0}&apos; before initialize (WrongInit) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0001_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0001_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Reading Uninitialized Value に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0001_Title {
            get {
                return ResourceManager.GetString("SMA0001_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Trying to read value from static field declared in cross-referencing type. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0002_Description {
            get {
                return ResourceManager.GetString("SMA0002_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Type &apos;{0}&apos; is referencing static field in type &apos;{1}&apos; (CrossRef) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0002_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0002_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cross Referencing across Type に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0002_Title {
            get {
                return ResourceManager.GetString("SMA0002_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Initialization order of partial type files is undefined in language spec. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0003_Description {
            get {
                return ResourceManager.GetString("SMA0003_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Partial type member &apos;{0}&apos; is declared in another .cs file (AnotherFile) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0003_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0003_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Static Member Declared in Another File に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0003_Title {
            get {
                return ResourceManager.GetString("SMA0003_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Static field is read before initialize. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0004_Description {
            get {
                return ResourceManager.GetString("SMA0004_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &apos;{0}&apos; is reading this member before declaration (LateDeclare) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0004_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0004_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Late Declaration に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0004_Title {
            get {
                return ResourceManager.GetString("SMA0004_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   `TSelf` Type Arg Analysis に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0010__MD_TITLE__ {
            get {
                return ResourceManager.GetString("SMA0010__MD_TITLE__", resourceCulture);
            }
        }
        
        /// <summary>
        ///   `TSelf` type arg should be pointing itself. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0010_Description {
            get {
                return ResourceManager.GetString("SMA0010_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Did you mean &apos;{0}&apos;? (TSelf) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0010_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0010_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   TSelf is Not Self に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0010_Title {
            get {
                return ResourceManager.GetString("SMA0010_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   `TSelf` type arg should be pointing itself or its base type. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0011_Description {
            get {
                return ResourceManager.GetString("SMA0011_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Did you mean &apos;{0}&apos; or base type? (TSelfCovariant) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0011_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0011_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   TSelf is Not Self or Base に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0011_Title {
            get {
                return ResourceManager.GetString("SMA0011_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   `TSelf` type arg should be pointing itself or its derived type. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0012_Description {
            get {
                return ResourceManager.GetString("SMA0012_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Did you mean &apos;{0}&apos; or derived type? (TSelfContravariant) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0012_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0012_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   TSelf is Not Self or Derived に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0012_Title {
            get {
                return ResourceManager.GetString("SMA0012_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   `TSelf` type constraint is not pointing itself. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0015_Description {
            get {
                return ResourceManager.GetString("SMA0015_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Did you mean &apos;{0}&apos;? (TSelfPointingOther) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0015_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0015_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   TSelf Constraint is Not Self に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0015_Title {
            get {
                return ResourceManager.GetString("SMA0015_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum Type Analysis に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0020__MD_TITLE__ {
            get {
                return ResourceManager.GetString("SMA0020__MD_TITLE__", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unchecked value conversion to enum type. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0020_Description {
            get {
                return ResourceManager.GetString("SMA0020_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unchecked value of &apos;{0}&apos; (CastToEnum) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0020_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0020_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unchecked Cast to Enum Type に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0020_Title {
            get {
                return ResourceManager.GetString("SMA0020_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Casting enum type to other. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0021_Description {
            get {
                return ResourceManager.GetString("SMA0021_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Conversion of &apos;{0}&apos; (CastFromEnum) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0021_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0021_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cast from Enum Type to Other に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0021_Title {
            get {
                return ResourceManager.GetString("SMA0021_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unchecked value conversion to generic enum type. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0022_Description {
            get {
                return ResourceManager.GetString("SMA0022_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unchecked value of &apos;{0}&apos; (CastToGenericEnum) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0022_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0022_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unchecked Cast to Generic Enum Type に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0022_Title {
            get {
                return ResourceManager.GetString("SMA0022_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Casting generic enum type to other. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0023_Description {
            get {
                return ResourceManager.GetString("SMA0023_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Conversion of &apos;{0}&apos; (CastFromGenericEnum) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0023_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0023_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cast from Generic Enum Type to Other に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0023_Title {
            get {
                return ResourceManager.GetString("SMA0023_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Trying to convert enum value to string. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0024_Description {
            get {
                return ResourceManager.GetString("SMA0024_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   String representation of &apos;{0}&apos; could be modified by obfuscation tool (EnumToString) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0024_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0024_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum to String に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0024_Title {
            get {
                return ResourceManager.GetString("SMA0024_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Calling enum system method. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0025_Description {
            get {
                return ResourceManager.GetString("SMA0025_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum handling should be encapsulated in utility class (EnumMethod) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0025_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0025_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum System Method に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0025_Title {
            get {
                return ResourceManager.GetString("SMA0025_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum obfuscation should have controlled. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0026_Description {
            get {
                return ResourceManager.GetString("SMA0026_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &apos;Obfuscation&apos; attribute should be added to prevent name changes (EnumObfuscation) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0026_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0026_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum Obfuscation に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0026_Title {
            get {
                return ResourceManager.GetString("SMA0026_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum w/o `Flags` attribute should be defined as usual. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0027_Description {
            get {
                return ResourceManager.GetString("SMA0027_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Non-Flags enum type should have int-typed and no index modifier (UnusualEnum) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0027_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0027_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Unusual Enum Definition に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0027_Title {
            get {
                return ResourceManager.GetString("SMA0027_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum-like pattern implementation is not complete. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0028_Description {
            get {
                return ResourceManager.GetString("SMA0028_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Enum-like type &apos;{0}&apos; (EnumLike)
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0028_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0028_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Invalid Enum-like Pattern に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0028_Title {
            get {
                return ResourceManager.GetString("SMA0028_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Struct Analysis に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0030__MD_TITLE__ {
            get {
                return ResourceManager.GetString("SMA0030__MD_TITLE__", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Constructor has declared explicitly so should not use parameter-less one. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0030_Description {
            get {
                return ResourceManager.GetString("SMA0030_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &apos;{0}&apos; has applicable constructor (InvalidStructCtor) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0030_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0030_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Invalid Struct Constructor に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0030_Title {
            get {
                return ResourceManager.GetString("SMA0030_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Disposable Analysis に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0040__MD_TITLE__ {
            get {
                return ResourceManager.GetString("SMA0040__MD_TITLE__", resourceCulture);
            }
        }
        
        /// <summary>
        ///   `using` statement should be used for instance that has public `void Dispose()` or `ValueTask DisposeAsync()` method. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0040_Description {
            get {
                return ResourceManager.GetString("SMA0040_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   &apos;{0}&apos; has &apos;IDisposable&apos; pattern implemented (MissingUsing) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0040_MessageFormat {
            get {
                return ResourceManager.GetString("SMA0040_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Missing Using Statement に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA0040_Title {
            get {
                return ResourceManager.GetString("SMA0040_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Annotating and Underling に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9000__MD_TITLE__ {
            get {
                return ResourceManager.GetString("SMA9000__MD_TITLE__", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9000_Description {
            get {
                return ResourceManager.GetString("SMA9000_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [identifier] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9000_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9000_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining on Identifier Symbols に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9000_Title {
            get {
                return ResourceManager.GetString("SMA9000_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9001_Description {
            get {
                return ResourceManager.GetString("SMA9001_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [localvar] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9001_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9001_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining on Local Variables に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9001_Title {
            get {
                return ResourceManager.GetString("SMA9001_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9002_Description {
            get {
                return ResourceManager.GetString("SMA9002_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [param] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9002_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9002_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining on Method or Lambda Parameters に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9002_Title {
            get {
                return ResourceManager.GetString("SMA9002_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9010_Description {
            get {
                return ResourceManager.GetString("SMA9010_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [declaration] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9010_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9010_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining on Declarations に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9010_Title {
            get {
                return ResourceManager.GetString("SMA9010_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9015_Description {
            get {
                return ResourceManager.GetString("SMA9015_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [designated] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9015_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9015_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining on Designated Type only に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9015_Title {
            get {
                return ResourceManager.GetString("SMA9015_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9020_Description {
            get {
                return ResourceManager.GetString("SMA9020_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [linehead] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9020_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9020_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining at Line Head に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9020_Title {
            get {
                return ResourceManager.GetString("SMA9020_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9021_Description {
            get {
                return ResourceManager.GetString("SMA9021_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [linelead] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9021_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9021_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining at Line Leading に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9021_Title {
            get {
                return ResourceManager.GetString("SMA9021_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9022_Description {
            get {
                return ResourceManager.GetString("SMA9022_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [linefill] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9022_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9022_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining on Identifier に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9022_Title {
            get {
                return ResourceManager.GetString("SMA9022_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9023_Description {
            get {
                return ResourceManager.GetString("SMA9023_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   [lineend] {0}
        ///&gt; {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9023_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9023_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining at Line End に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9023_Title {
            get {
                return ResourceManager.GetString("SMA9023_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Draw underline in IDE. に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9100_Description {
            get {
                return ResourceManager.GetString("SMA9100_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {1} ({0}) に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9100_MessageFormat {
            get {
                return ResourceManager.GetString("SMA9100_MessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Underlining as Warning に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SMA9100_Title {
            get {
                return ResourceManager.GetString("SMA9100_Title", resourceCulture);
            }
        }
    }
}
