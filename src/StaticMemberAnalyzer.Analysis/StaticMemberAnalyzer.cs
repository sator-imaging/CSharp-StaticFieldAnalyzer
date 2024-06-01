using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace SatorImaging.StaticMemberAnalyzer.Analysis
{
    //https://github.com/dotnet/roslyn/blob/main/docs/wiki/Roslyn-Overview.md#solutions-projects-documents
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticMemberAnalyzer : DiagnosticAnalyzer
    {
        //https://github.com/dotnet/roslyn-analyzers/blob/main/src/Utilities/Compiler/DiagnosticCategoryAndIdRanges.txt
        private const string Category = "Usage";


#if DEBUG
        public const string RuleId_DEBUG = "SMAxDBG";  // no hyphens!
        private static readonly DiagnosticDescriptor Rule_DEBUG = new(
            RuleId_DEBUG,
            "DEBUG",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "DEBUG");
#endif

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

        public const string RuleId_WrongInit = "SMA0001";
        private static readonly DiagnosticDescriptor Rule_WrongInit = new(
            RuleId_WrongInit,
            new LocalizableResourceString(nameof(Resources.SMA0001_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0001_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0001_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_CrossRef = "SMA0002";
        private static readonly DiagnosticDescriptor Rule_CrossRef = new(
            RuleId_CrossRef,
            new LocalizableResourceString(nameof(Resources.SMA0002_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0002_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0002_Description), Resources.ResourceManager, typeof(Resources)));


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if DEBUG
            Rule_DEBUG,
#endif
            Rule_WrongInit,
            Rule_CrossRef
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        /*  init  ================================================================ */

        private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
        {
            // it seems that model cannot be reusable. all reports are gone after opening other file in VisualStudio
            cache_filePathToModel.Clear();

            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md
            // called per source document
            context.RegisterSemanticModelAction(AnalyzeStaticFields);

            //context.RegisterCompilationEndAction(_ =>
            //{
            //});
        }


        /*  impl  ================================================================ */

        const int DEFAULT_LIST_CAPACITY = 4;

#pragma warning disable RS1008
        // Obsolete is for attention. make local var and use it to avoid static field access
        [ThreadStatic, Obsolete] static HashSet<string>? ts_declaredMemberSet;
        [ThreadStatic, Obsolete] static HashSet<IMemberReferenceOperation>? ts_crossRefReportedSet;
        [ThreadStatic, Obsolete] static List<FieldDeclarationSyntax>? ts_crossFDSyntaxList;
        [ThreadStatic, Obsolete] static List<IFieldSymbol>? ts_fieldSymbolList;
        [ThreadStatic, Obsolete] static List<IMemberReferenceOperation>? ts_refOperatorList;
        //[ThreadStatic, Obsolete] static List<IFieldSymbol>? ts_crossFieldSymbolList;
        [ThreadStatic, Obsolete] static List<IMemberReferenceOperation>? ts_crossRefOperatorList;
#pragma warning restore RS1008

        readonly static ConcurrentDictionary<string, SemanticModel> cache_filePathToModel = new();


        // NOTE: async method causes error on complex source code
        private static /*async*/ void AnalyzeStaticFields(SemanticModelAnalysisContext context)
        {
            // make local var to avoid static field access
            var declaredMemberSet = (ts_declaredMemberSet ??= new());
            var crossRefReportedSet = (ts_crossRefReportedSet ??= new());
            var crossFDSyntaxList = (ts_crossFDSyntaxList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            declaredMemberSet.Clear();
            crossRefReportedSet.Clear();
            crossFDSyntaxList.Clear();

            var fieldSymbolList = (ts_fieldSymbolList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            var refOperatorList = (ts_refOperatorList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            //var crossFieldSymbolList = (ts_crossFieldSymbolList ??= new(capacity: DEFAULT_LIST_CAPACITY));
            var crossRefOperatorList = (ts_crossRefOperatorList ??= new(capacity: DEFAULT_LIST_CAPACITY));

            cache_filePathToModel.TryAdd(context.SemanticModel.SyntaxTree.FilePath, context.SemanticModel);

            //var root = await context.SemanticModel.SyntaxTree.GetRootAsync(context.CancellationToken).ConfigureAwait(false);
            var root = context.SemanticModel.SyntaxTree.GetRoot();
            foreach (var fieldSyntax in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                ClearAndCollectFieldInfo(fieldSyntax, context.SemanticModel, fieldSymbolList, refOperatorList);

                for (int i = 0; i < refOperatorList.Count; i++)
                {
                    var refOp = refOperatorList[i];

                    /*  declaration order  ================================================================ */

                    if (SymbolEqualityComparer.Default.Equals(refOp.Member.ContainingType, fieldSymbolList[i].ContainingType))
                    {
                        if (!declaredMemberSet.Contains(refOp.Member.Name))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(Rule_WrongInit, refOp.Syntax.GetLocation(),
                                refOp.Member.Name));
                        }
                    }

                    /*  cross referencing  ================================================================ */

                    else
                    {
                        if (crossRefReportedSet.Contains(refOp))
                            continue;

                        crossFDSyntaxList.Clear();
                        foreach (var dsr in refOp.Member.ContainingType.DeclaringSyntaxReferences)
                        {
                            //var s = await dsr.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(false);
                            var s = dsr.GetSyntax();
                            crossFDSyntaxList.AddRange(s.DescendantNodes().OfType<FieldDeclarationSyntax>());
                        }

                        foreach (var crossField in crossFDSyntaxList)
                        {
                            if (!cache_filePathToModel.TryGetValue(crossField.SyntaxTree.FilePath, out var crossModel))
                            {
                                crossModel = cache_filePathToModel.GetOrAdd(crossField.SyntaxTree.FilePath,
                                                                            context.SemanticModel.Compilation.GetSemanticModel(crossField.SyntaxTree));
                            }

                            ClearAndCollectFieldInfo(crossField, crossModel, /*crossFieldSymbolList*/null, crossRefOperatorList);

                            for (int c = 0; c < crossRefOperatorList.Count; c++)
                            {
                                if (!SymbolEqualityComparer.Default.Equals(crossRefOperatorList[c].Member.ContainingType, fieldSymbolList[i].ContainingType))
                                    continue;

                                context.ReportDiagnostic(
                                    Diagnostic.Create(Rule_CrossRef, refOp.Syntax.GetLocation(),
                                    refOp.Member.ContainingType.Name, fieldSymbolList[i].ContainingType.Name));

                                crossRefReportedSet.Add(refOp);

                                break;
                            }
                        }
                    }
                }

                declaredMemberSet.UnionWith(fieldSyntax.Declaration.Variables.Select(x => x.Identifier.Text));
            }
        }


        private static void ClearAndCollectFieldInfo(FieldDeclarationSyntax fieldSyntax,
                                                     SemanticModel semanticModel,
                                                     List<IFieldSymbol>? fieldSymbolList,
                                                     List<IMemberReferenceOperation> refOperatorList)
        {
            fieldSymbolList?.Clear();
            refOperatorList.Clear();

            // GetOperation must run on EqualsValueClauseSyntax, otherwise returns null
            foreach (var eq in fieldSyntax.DescendantNodes().OfType<EqualsValueClauseSyntax>())
            {
                var initOp = semanticModel.GetOperation(eq) as IFieldInitializerOperation;
                if (initOp == null || initOp.IsImplicit)
                    continue;

                var fieldSymbol = initOp.InitializedFields.FirstOrDefault();  // check first one --> static int FIRST, SECOND = 10;
                if (fieldSymbol == null || fieldSymbol.IsConst || !fieldSymbol.IsStatic || fieldSymbol.IsImplicitlyDeclared)
                    continue;

                foreach (var refOp in initOp.Descendants().OfType<IMemberReferenceOperation>())
                {
                    if (!refOp.Member.IsStatic || refOp.Member.IsImplicitlyDeclared)
                        continue;

                    //const??
                    if ((refOp.Member as IFieldSymbol)?.IsConst == true)
                        continue;

                    fieldSymbolList?.Add(fieldSymbol);  // allow duplicate entry to simplify logic
                    refOperatorList.Add(refOp);
                }
            }
        }


        /*  property  ================================================================ */

        //TODO

    }
}
