/*  Core  ================================================================ */
#define STMG_DEBUG_MESSAGE    // some try-catch will be enabled
#if DEBUG == false
#undef STMG_DEBUG_MESSAGE
#endif

#if STMG_DEBUG_MESSAGE
//#define STMG_DEBUG_MESSAGE_VERBOSE    // for debugging. many of additional debug diagnostics will be emitted
#endif
/*  /Core  ================================================================ */

#define STMG_USE_ATTRIBUTE_CACHE
#define STMG_USE_DESCRIPTION_CACHE
//#define STMG_ENABLE_LINE_FILL

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StaticMemberAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      DESCRIPTOR      = */

        public const string RuleId_WrongInit = "SMA0001";
        private static readonly DiagnosticDescriptor Rule_WrongInit = new(
            RuleId_WrongInit,
            new LocalizableResourceString(nameof(Resources.SMA0001_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0001_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0001_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_CrossRef = "SMA0002";
        private static readonly DiagnosticDescriptor Rule_CrossRef = new(
            RuleId_CrossRef,
            new LocalizableResourceString(nameof(Resources.SMA0002_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0002_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0002_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_AnotherFile = "SMA0003";
        private static readonly DiagnosticDescriptor Rule_AnotherFile = new(
            RuleId_AnotherFile,
            new LocalizableResourceString(nameof(Resources.SMA0003_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0003_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0003_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_LateDeclare = "SMA0004";
        private static readonly DiagnosticDescriptor Rule_LateDeclare = new(
            RuleId_LateDeclare,
            new LocalizableResourceString(nameof(Resources.SMA0004_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0004_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0004_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DebugError,
            Core.Rule_DebugWarn,
#endif
            Rule_WrongInit,
            Rule_CrossRef,
            Rule_AnotherFile,
            Rule_LateDeclare
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            /* =      semantic model      = */

            // called per source document
            context.RegisterSemanticModelAction(AnalyzeStaticFields);


            //context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        //private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
        //{
        //}


        /*  impl  ================================================================ */

        const int DEFAULT_LIST_CAPACITY = 4;

#pragma warning disable RS1008  // OK: always clear on method startup
        [ThreadStatic, DescriptionAttribute] static Dictionary<string, SemanticModel>? ts_filePathToModel;
        [ThreadStatic, DescriptionAttribute] static HashSet<string>? ts_declaredMemberSet;
        [ThreadStatic, DescriptionAttribute] static HashSet<IMemberReferenceOperation>? ts_crossRefReportedSet;
        [ThreadStatic, DescriptionAttribute] static List<FieldDeclarationSyntax>? ts_crossFDSyntaxList;
        [ThreadStatic, DescriptionAttribute] static List<ISymbol>? ts_foundSymbolList;
        [ThreadStatic, DescriptionAttribute] static List<IMemberReferenceOperation>? ts_refOperatorList;
        [ThreadStatic, DescriptionAttribute] static List<IMemberReferenceOperation>? ts_crossRefOperatorList;
#pragma warning restore RS1008


        // NOTE: async method causes error on complex source code
        private static /*async*/ void AnalyzeStaticFields(SemanticModelAnalysisContext context)
        {
            // make local var to avoid static field access
            var declaredMemberSet = (ts_declaredMemberSet ??= new());
            var crossRefReportedSet = (ts_crossRefReportedSet ??= new());
            var crossFDSyntaxList = (ts_crossFDSyntaxList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            var declaredMemberSymbolList = (ts_foundSymbolList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            var refOperatorList = (ts_refOperatorList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            var crossRefOperatorList = (ts_crossRefOperatorList ??= new(capacity: DEFAULT_LIST_CAPACITY));

            declaredMemberSet.Clear();
            crossRefReportedSet.Clear();
            crossFDSyntaxList.Clear();
            declaredMemberSymbolList.Clear();
            refOperatorList.Clear();
            crossRefOperatorList.Clear();

            // it seems that model cannot be reusable. all reports are gone after opening other file in VisualStudio
            var filePathToModel = (ts_filePathToModel ??= new());
            filePathToModel.Clear();
            filePathToModel[context.SemanticModel.SyntaxTree.FilePath] = context.SemanticModel;

            var token = context.CancellationToken;

            //var root = await context.SemanticModel.SyntaxTree.GetRootAsync(context.CancellationToken).ConfigureAwait(false);
            var root = context.SemanticModel.SyntaxTree.GetRoot(token);
            foreach (var memberSyntax in root.DescendantNodes().OfType<MemberDeclarationSyntax>()) //FieldDeclarationSyntax
            {
                var fieldSyntax = memberSyntax as FieldDeclarationSyntax;
                if (fieldSyntax == null && memberSyntax is not PropertyDeclarationSyntax /*propSyntax*/)
                    continue;

                ClearAndCollectFieldInfo(memberSyntax, context.SemanticModel, declaredMemberSymbolList, refOperatorList, token);

                for (int i = 0; i < refOperatorList.Count; i++)
                {
                    var refOp = refOperatorList[i];

                    /*  declaration order  ================================================================ */

                    var refOpMemberContainingTypeDeclares = refOp.Member.ContainingType.DeclaringSyntaxReferences;

                    // reading field in same type
                    if (SymbolEqualityComparer.Default.Equals(refOp.Member.ContainingType, declaredMemberSymbolList[i].ContainingType))
                    {
                        bool isPartial = false;
                        if (refOpMemberContainingTypeDeclares.Length > 1)
                        {
                            var a = refOp.Member.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.FilePath;
                            var b = declaredMemberSymbolList[i].DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.FilePath;

                            if (a != null && b != null && a != b)
                            {
                                isPartial = true;

                                context.ReportDiagnostic(
                                    Diagnostic.Create(Rule_AnotherFile, refOp.Syntax.GetLocation(),
                                    refOp.Member.Name));
                            }
                        }

                        if (!isPartial)
                        {
                            var prefix = Core.GetMemberNamePrefix(refOp.Member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(token));
                            if (!declaredMemberSet.Contains(Core.SpanConcat(prefix.AsSpan(), refOp.Member.Name.AsSpan())))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(Rule_WrongInit, refOp.Syntax.GetLocation(),
                                    refOp.Member.Name));

                                foreach (var loc in refOp.Member.Locations)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(Rule_LateDeclare, loc, declaredMemberSymbolList[i].Name));
                                }
                            }
                        }
                    }

                    /*  cross referencing  ================================================================ */

                    else
                    {
                        if (crossRefReportedSet.Contains(refOp))
                            continue;

                        crossFDSyntaxList.Clear();
                        foreach (var dsr in refOpMemberContainingTypeDeclares)
                        {
                            //var s = await dsr.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(false);
                            var s = dsr.GetSyntax(token);
                            crossFDSyntaxList.AddRange(s.DescendantNodes().OfType<FieldDeclarationSyntax>());
                        }

                        foreach (var crossField in crossFDSyntaxList)
                        {
                            if (!filePathToModel.TryGetValue(crossField.SyntaxTree.FilePath, out var crossModel))
                            {
                                crossModel = context.SemanticModel.Compilation.GetSemanticModel(crossField.SyntaxTree);
                                filePathToModel[crossField.SyntaxTree.FilePath] = crossModel;
                            }

                            ClearAndCollectFieldInfo(crossField, crossModel, /*crossFoundSymbolList*/null, crossRefOperatorList, token);

                            for (int c = 0; c < crossRefOperatorList.Count; c++)
                            {
                                if (!SymbolEqualityComparer.Default.Equals(crossRefOperatorList[c].Member.ContainingType, declaredMemberSymbolList[i].ContainingType))
                                    continue;

                                context.ReportDiagnostic(
                                    Diagnostic.Create(Rule_CrossRef, refOp.Syntax.GetLocation(),
                                    refOp.Member.ContainingType.Name, declaredMemberSymbolList[i].ContainingType.Name));

                                crossRefReportedSet.Add(refOp);

                                break;
                            }
                        }
                    }
                }

                if (fieldSyntax != null)
                {
                    var prefix = Core.GetMemberNamePrefix(fieldSyntax);

                    declaredMemberSet.UnionWith(fieldSyntax.Declaration.Variables.Select(x =>
                    {
                        return Core.SpanConcat(prefix.AsSpan(), x.Identifier.Text.AsSpan());
                    }));
                }
            }
        }


        private static void ClearAndCollectFieldInfo(MemberDeclarationSyntax memberSyntax,
                                                     SemanticModel semanticModel,
                                                     List<ISymbol>? foundSymbolList,
                                                     List<IMemberReferenceOperation> refOperatorList,
                                                     CancellationToken token
            )
        {
            foundSymbolList?.Clear();
            refOperatorList.Clear();

            // GetOperation must run on EqualsValueClauseSyntax, otherwise returns null
            foreach (var eq in memberSyntax.DescendantNodes().OfType<EqualsValueClauseSyntax>())
            {
                var initOp = semanticModel.GetOperation(eq, token) as ISymbolInitializerOperation;// IFieldInitializerOperation;
                if (initOp == null || initOp.IsImplicit)
                    continue;

                //lambda??
                if (initOp.Children.FirstOrDefault() is IDelegateCreationOperation)
                    continue;

                ISymbol? foundSymbol = null;
                if (initOp is IFieldInitializerOperation fieldInitOp)
                {
                    var fieldSymbol = fieldInitOp.InitializedFields.FirstOrDefault();  // check first one --> static int FIRST, SECOND = 10;
                    if (fieldSymbol == null || fieldSymbol.IsConst || !fieldSymbol.IsStatic || fieldSymbol.IsImplicitlyDeclared)
                        continue;

                    foundSymbol = fieldSymbol;
                }
                else if (initOp is IPropertyInitializerOperation propInitOp)
                {
                    var propSymbol = propInitOp.InitializedProperties.FirstOrDefault();
                    if (propSymbol == null || !propSymbol.IsStatic || propSymbol.IsImplicitlyDeclared)
                        continue;

                    foundSymbol = propSymbol;
                }

                if (foundSymbol == null)
                    continue;

                foreach (var refOp in initOp.Descendants().OfType<IFieldReferenceOperation>()) //IMemberReferenceOperation
                {
                    if (!refOp.Member.IsStatic || refOp.Member.IsImplicitlyDeclared)
                        continue;

                    //const??
                    if ((refOp.Member as IFieldSymbol)?.IsConst == true)
                        continue;

                    ////method??
                    //if (refOp.Member is IMethodSymbol)
                    //    continue;

                    //nameof/typeof??
                    if (refOp.Parent is INameOfOperation)// or ITypeOfOperation)
                        continue;

                    foundSymbolList?.Add(foundSymbol);  // allow duplicate entry to simplify logic
                    refOperatorList.Add(refOp);
                }
            }
        }

    }
}
