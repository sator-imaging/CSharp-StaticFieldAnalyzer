#define STMG_DEBUG_MESSAGE    // some try-catch will be enabled

#if STMG_DEBUG_MESSAGE
//#define STMG_DEBUG_MESSAGE_VERBOSE    // for debugging. many of additional debug diagnostics will be emitted
#endif

#define STMG_USE_ATTRIBUTE_CACHE
#define STMG_USE_DESCRIPTION_CACHE
//#define STMG_ENABLE_LINE_FILL

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace SatorImaging.StaticMemberAnalyzer.Analysis
{
    //https://github.com/dotnet/roslyn/blob/main/docs/wiki/Roslyn-Overview.md#solutions-projects-documents
    public sealed class StaticMemberAnalyzer
    {
        //https://github.com/dotnet/roslyn-analyzers/blob/main/src/Utilities/Compiler/DiagnosticCategoryAndIdRanges.txt
        private const string Category = "Usage";


        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

        #region  /* =      STATIC MEMBER DESCRIPTOR      = */

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

        public const string RuleId_AnotherFile = "SMA0003";
        private static readonly DiagnosticDescriptor Rule_AnotherFile = new(
            RuleId_AnotherFile,
            new LocalizableResourceString(nameof(Resources.SMA0003_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0003_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0003_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_LateDeclare = "SMA0004";
        private static readonly DiagnosticDescriptor Rule_LateDeclare = new(
            RuleId_LateDeclare,
            new LocalizableResourceString(nameof(Resources.SMA0004_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0004_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0004_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        #region  /* =      DESCRIPTION DESCRIPTOR      = */

        public const string RuleId_SymbolDesc = "SMA9000";
        private static readonly DiagnosticDescriptor Rule_SymbolDesc = new(
            RuleId_SymbolDesc,
            new LocalizableResourceString(nameof(Resources.SMA9000_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9000_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9000_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_LocalVarDesc = "SMA9001";
        private static readonly DiagnosticDescriptor Rule_LocalVarDesc = new(
            RuleId_LocalVarDesc,
            new LocalizableResourceString(nameof(Resources.SMA9001_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9001_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9001_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_ParameterDesc = "SMA9002";
        private static readonly DiagnosticDescriptor Rule_ParameterDesc = new(
            RuleId_ParameterDesc,
            new LocalizableResourceString(nameof(Resources.SMA9002_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9002_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9002_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_DeclarationDesc = "SMA9010";
        private static readonly DiagnosticDescriptor Rule_DeclarationDesc = new(
            RuleId_DeclarationDesc,
            new LocalizableResourceString(nameof(Resources.SMA9010_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9010_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9010_Description), Resources.ResourceManager, typeof(Resources)));


        // line annotators
        public const string RuleId_LineHeadDesc = "SMA9020";
        private static readonly DiagnosticDescriptor Rule_LineHeadDesc = new(
            RuleId_LineHeadDesc,
            new LocalizableResourceString(nameof(Resources.SMA9020_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9020_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9020_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_LineLeadingDesc = "SMA9021";
        private static readonly DiagnosticDescriptor Rule_LineLeadingDesc = new(
            RuleId_LineLeadingDesc,
            new LocalizableResourceString(nameof(Resources.SMA9021_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9021_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9021_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_LineFillDesc = "SMA9022";
        private static readonly DiagnosticDescriptor Rule_LineFillDesc = new(
            RuleId_LineFillDesc,
            new LocalizableResourceString(nameof(Resources.SMA9022_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9022_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9022_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_LineEndDesc = "SMA9023";
        private static readonly DiagnosticDescriptor Rule_LineEndDesc = new(
            RuleId_LineEndDesc,
            new LocalizableResourceString(nameof(Resources.SMA9023_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9023_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9023_Description), Resources.ResourceManager, typeof(Resources)));


        //warning!!
        public const string RuleId_WarningDesc = "SMA9100";
        private static readonly DiagnosticDescriptor Rule_WarningDesc = new(
            RuleId_WarningDesc,
            new LocalizableResourceString(nameof(Resources.SMA9100_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA9100_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA9100_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        #region  /* =      DEBUG DESCRIPTOR      = */

        // NOTE: need to define in any case to avoid error but don't register to
        //       analyzer when debug message flag is not set.
        public const string RuleId_DEBUG = "SMAxDEBUG";  // no hyphens!
        private static readonly DiagnosticDescriptor Rule_DEBUG = new(
            RuleId_DEBUG,
            "SMAxDEBUG",
            "{0}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "SMAxDEBUG");

        #endregion


        /*  DEBUG  ================================================================ */

        [Conditional("STMG_DEBUG_MESSAGE")]
        [DescriptionAttribute]
        private static void ReportDebugMessage(Action<Diagnostic> reportAction, string title, string? message, Location location)
        {
            ReportDebugMessage(reportAction, title, message, new Location[] { location });
        }

        [Conditional("STMG_DEBUG_MESSAGE")]
        [DescriptionAttribute]
        private static void ReportDebugMessage<T>(Action<Diagnostic> reportAction, string title, string? message, T locations)
            where T : IEnumerable<Location>
        {
            if (locations == null)
                return;

            message = message == null ? message : title + "\n" + message;
            foreach (var loc in locations)
            {
                reportAction(Diagnostic.Create(Rule_DEBUG, loc, message));
            }
        }


        /*  string op  ================================================================ */

        [ThreadStatic, DescriptionAttribute] static StringBuilder? ts_sb;

        private static string GetMemberNamePrefix(SyntaxNode? node)
        {
            var sb = (ts_sb ??= new());
            sb.Length = 0;  // don't clear! it will set capacity = 0 and allocated memory is gone!!

            var parent = node?.Parent;
            while (parent != null)
            {
                switch (parent)
                {
                    case TypeDeclarationSyntax type:
                        sb.Insert(0, type.Identifier.Text);
                        break;
                    case NamespaceDeclarationSyntax ns:
                        sb.Insert(0, ns.Name.ToString());
                        break;
                }
                parent = parent.Parent;
            }

            return sb.ToString();
        }


        // string.Create and Concat(ReadOnlySpan) cannot be used in .net standard 2.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string SpanConcat(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
        {
            Span<char> buffer = stackalloc char[left.Length + right.Length];
            left.CopyTo(buffer);
            right.CopyTo(buffer.Slice(left.Length));

            return buffer.ToString();
        }


        /*  underlining analyzer  ================================================================ */

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public sealed class UnderliningImpl : DiagnosticAnalyzer
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
                Rule_DEBUG,
#endif
                Rule_SymbolDesc,
                Rule_LocalVarDesc,
                Rule_ParameterDesc,
                Rule_DeclarationDesc,

                Rule_LineHeadDesc,
                Rule_LineLeadingDesc,
                Rule_LineFillDesc,
                Rule_LineEndDesc,

                Rule_WarningDesc
                );


            static SyntaxKind[]? _descriptionTargetSyntaxes;
            static SymbolKind[]? _descriptionTargetSymbols;
            static OperationKind[]? _descriptionTargetOperations;

            public override void Initialize(AnalysisContext context)
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.EnableConcurrentExecution();


                //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

                /* =      symbol      = */

                _descriptionTargetSymbols ??= new SymbolKind[]
                {
                    SymbolKind.NamedType,
                    SymbolKind.Method,
                    SymbolKind.ArrayType,
                    SymbolKind.Event,
                    SymbolKind.Field,
                    SymbolKind.Property,
                    SymbolKind.Parameter,
                    SymbolKind.TypeParameter,
                };

                context.RegisterSymbolAction(DrawUnderlineOnSymbols, _descriptionTargetSymbols);


                /* =      syntax      = */

                _descriptionTargetSyntaxes ??= new SyntaxKind[]
                {
                    SyntaxKind.IdentifierName,

                    //// NOTE: ImplicitObjectCreationExpression (`new()`) is not supported. use Operation instead.
                    //SyntaxKind.ObjectCreationExpression,

                    // ctor(...) : base(...) or this(...)
                    SyntaxKind.BaseConstructorInitializer,
                    SyntaxKind.ThisConstructorInitializer,

                    // required for lambda parameter
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression,
                };

                context.RegisterSyntaxNodeAction(DrawUnderlineOnSyntaxNodes, _descriptionTargetSyntaxes);


                /* =      operation      = */

                _descriptionTargetOperations ??= new OperationKind[]
                {
                    //constructor
                    OperationKind.ObjectCreation,
                };

                context.RegisterOperationAction(DrawUnderlineOnOperators, _descriptionTargetOperations);


                /* =      cache      = */

                context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
            }


            /*  register  ================================================================ */

            [ThreadStatic, DescriptionAttribute] static Location[]? ts_singleLocationArray;
            [ThreadStatic, DescriptionAttribute] static Dictionary<ISymbol, AttributeSyntax?>? ts_symbolToDescription;
            [ThreadStatic, DescriptionAttribute] static Dictionary</*AttributeSyntax*/SyntaxNode, string?>? ts_descAttrToMessage;

            private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
            {
                /* =      clear cache      = */

                context.RegisterCodeBlockStartAction<SyntaxKind>(ctx =>
                {
                    var descAttrToMessage = ts_descAttrToMessage;
                    if (descAttrToMessage == null)
                        return;

                    // NOTE: assume that contained attribute syntax is about to update
                    foreach (var attr in ctx.CodeBlock.DescendantNodesAndSelf().OfType<AttributeSyntax>())
                    {
                        if (descAttrToMessage.ContainsKey(attr))
                            descAttrToMessage.Remove(attr);
                    }
                });

                // clear every 16 iterations
                int iter = 0;
                context.RegisterCompilationEndAction(ctx =>
                {
                    if (iter++ < 16)
                        return;

                    iter = 0;

                    var symbolToDescription = ts_symbolToDescription;
                    if (symbolToDescription == null)
                        return;

                    symbolToDescription.Clear();
                });
            }


            /*  entry  ================================================================ */

            //symbol
            private static void DrawUnderlineOnSymbols(SymbolAnalysisContext context)
            {
                var symbol = context.Symbol;
                if (symbol.IsImplicitlyDeclared)
                    return;


                var compilation = context.Compilation;
                var token = context.CancellationToken;
                Action<Diagnostic> reportAction = context.ReportDiagnostic;


                var symbolToDescription = (ts_symbolToDescription ??= new());
                var descAttrToMessage = (ts_descAttrToMessage ??= new());

#if STMG_DEBUG_MESSAGE_VERBOSE
            ReportDebugMessage(context.ReportDiagnostic,
                "DEBUG SYMBOL",
                GetAndUpdateDescriptionCache(symbol, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token)
                    ?? "=== NO DESCRIPTION === " + symbol.Name,
                symbol.Locations);
#endif

                Underlining(symbol.Locations, symbol, Rule_DeclarationDesc,
                    reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token, 0);


                // generic type/method
                switch (symbol)
                {
                    case INamedTypeSymbol type:
                        foreach (var tp in type.TypeParameters)
                        {
                            Underlining(tp.Locations, tp, Rule_DeclarationDesc,
                                reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token, 0);
                        }
                        break;
                    case IMethodSymbol method:
                        foreach (var tp in method.TypeParameters)
                        {
                            Underlining(tp.Locations, tp, Rule_DeclarationDesc,
                                reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token, 0);
                        }
                        break;
                }
            }


            //syntax
            private static void DrawUnderlineOnSyntaxNodes(SyntaxNodeAnalysisContext context)
            {
                var syntax = context.Node;
                var tree = syntax.SyntaxTree;
                var compilation = context.Compilation;

                // NOTE: don't use cached one!
                //var filePathToModel = (ts_filePathToModel ??= new());
                //if (!filePathToModel.TryGetValue(tree.FilePath, out var model))
                //{
                var model = compilation.GetSemanticModel(tree);
                //    filePathToModel[tree.FilePath] = model;
                //}


                var symbolToDescription = (ts_symbolToDescription ??= new());
                Action<Diagnostic> reportAction = context.ReportDiagnostic;
                var token = context.CancellationToken;

                var descAttrToMessage = (ts_descAttrToMessage ??= new());

                /* =      lambda!!      = */
                if (syntax is SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax)
                {
                    ReportLambda(model, syntax,
                        reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token);

                    return;
                }


                var rule = Rule_SymbolDesc;

                var singleLocation = (ts_singleLocationArray ??= new Location[1]);
                singleLocation[0] = syntax.GetLocation();

                var symbolCandidate = model.GetSymbolInfo(syntax, token);

                var symbol = symbolCandidate.Symbol;
                if (symbol == null)
                {
#if STMG_DEBUG_MESSAGE_VERBOSE
                ReportDebugMessage(context.ReportDiagnostic,
                    "DEBUG NODE (NOT FOUND)",
                    "=== SYMBOL NOT FOUND ===",
                    singleLocation);
#endif

                    //symbol = symbolCandidate.CandidateSymbols.FirstOrDefault();
                    //if (symbol == null)
                    return;
                }
                else if (symbol is ILocalSymbol localSymbol)
                {
                    symbol = localSymbol.Type;
                    rule = Rule_LocalVarDesc;
                }
                else if (symbol is IParameterSymbol paramSymbol)
                {
#if STMG_DEBUG_MESSAGE_VERBOSE
                ReportDebugMessage(context.ReportDiagnostic,
                    "DEBUG NODE (PARAM)",
                    GetAndUpdateDescriptionCache(symbol, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token)
                        ?? "=== NO DESCRIPTION === " + symbol.Name,
                    singleLocation);
#endif

                    // NOTE: draw underline on methodParameter --> void Method([DescriptionAttribute] T methodParameter);
                    Underlining(singleLocation, symbol, rule,
                        reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token,
                        0);

                    // then, draw underline based on variable type
                    symbol = paramSymbol.Type;
                    rule = Rule_ParameterDesc;
                }


#if STMG_DEBUG_MESSAGE_VERBOSE
            ReportDebugMessage(context.ReportDiagnostic,
                "DEBUG NODE: " + syntax.Kind(),
                GetAndUpdateDescriptionCache(symbol, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token)
                    ?? "=== NO DESCRIPTION === " + symbol.Name,
                singleLocation);
#endif

                if (symbol.IsImplicitlyDeclared)
                    return;


                Underlining(singleLocation, symbol, rule,
                    reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token,
                    0);
            }


            private static void ReportLambda(SemanticModel model,
                                             SyntaxNode syntax,
                                             Action<Diagnostic> reportAction,
                                             IDictionary<ISymbol, AttributeSyntax?> symbolToDescription,
                                             // works only on small source code file --> IDictionary<string, SemanticModel> filePathToModel,
                                             IDictionary<SyntaxNode, string?> descAttrToMessage,
                                             Compilation compilation,
                                             CancellationToken token
                )
            {
                INamedTypeSymbol? lambdaType = null;

                // lambda parameter type is type-less. take actual type from variable declaration
                var par = syntax.Parent;
                if (par is EqualsValueClauseSyntax)
                {
                    par = par.Parent;
                    if (par is VariableDeclaratorSyntax && par.Parent is VariableDeclarationSyntax varDecl)
                    {
                        lambdaType = model.GetSymbolInfo(varDecl.Type, token).Symbol as INamedTypeSymbol;
                    }
                }
                else if (par is AssignmentExpressionSyntax assign)
                {
                    // TODO: support delegate parameter types --> delgate ReturnType MyDelegate(ParamType param);
                    if (model.GetSymbolInfo(assign.Left, token).Symbol is ILocalSymbol localvar)
                    {
                        lambdaType = (localvar.Type as INamedTypeSymbol);//?.TypeArguments.FirstOrDefault();
                    }
                }

                if (lambdaType == null)
                {
#if STMG_DEBUG_MESSAGE_VERBOSE
                ReportDebugMessage(reportAction, "LAMBDA PARAM TYPE NOT FOUND", syntax.Kind().ToString(), syntax.GetLocation());
#endif

                    return;
                }


                var typeArgs = lambdaType.TypeArguments;
                var typeArgsCount = typeArgs.Length;
                if (typeArgsCount == 0)
                    return;

                var singleLocation = (ts_singleLocationArray ??= new Location[1]);

                switch (syntax)
                {
                    case SimpleLambdaExpressionSyntax lambda:
                        if (typeArgsCount == 1)
                        {
                            singleLocation[0] = lambda.Parameter.GetLocation();
                            Underlining(singleLocation, typeArgs[0], Rule_ParameterDesc,
                                reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token,
                                0);
                        }
                        return;

                    case ParenthesizedLambdaExpressionSyntax multi:
                        var parameters = multi.ParameterList.Parameters;
                        if (typeArgsCount == parameters.Count)
                        {
                            int pos = -1;
                            foreach (var param in parameters)
                            {
                                pos++;
                                singleLocation[0] = param.GetLocation();
                                Underlining(singleLocation, typeArgs[pos], Rule_ParameterDesc,
                                    reportAction, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token,
                                    0);
                            }
                        }
                        return;
                }


                reportAction.Invoke(Diagnostic.Create(Rule_ParameterDesc, syntax.GetLocation(),
                    SpanConcat("[SMA UNSUPPORTED]: ".AsSpan(), syntax.Kind().ToString().AsSpan()),
                    "TypeArguments.Count: " + typeArgsCount
                    ));
            }


            //operator
            private static void DrawUnderlineOnOperators(OperationAnalysisContext context)
            {
                var op = context.Operation;
                if (op.IsImplicit)
                    return;

                Action<Diagnostic> reportAction = context.ReportDiagnostic;
                var compilation = context.Compilation;
                var token = context.CancellationToken;

                var symbolToDescription = (ts_symbolToDescription ??= new());
                var descAttrToMessage = (ts_descAttrToMessage ??= new());

                var singleLocation = (ts_singleLocationArray ??= new Location[1]);

                if (op is IObjectCreationOperation ctorOp)
                {
                    if (ctorOp.Type is INamedTypeSymbol type && !type.IsImplicitlyDeclared)
                    {
                        var ctorOpSyntax = ctorOp.Syntax;
                        singleLocation[0] = ctorOpSyntax.GetLocation();

                        // NOTE: when optional parameter is omitted, Operation will return count INCLUDING omitted ones.
                        //       ex) ctor(int value, int other = 0)
                        //           _ = new(310);  // <-- this operation will return Length == 2, not 1.
                        var args = ctorOp.Arguments;
                        var argsCount = args.Length;
                        foreach (var ctor in type.Constructors)
                        {
                            if (!ctor.IsStatic)
                            {
                                var ctorParams = ctor.Parameters;

                                // count will match even if optional parameters are omitted
                                if (ctorParams.Length != argsCount)
                                    continue;

                                for (var i = 0; i < argsCount; i++)
                                {
                                    if (!SymbolEqualityComparer.Default.Equals(args[i].Parameter.Type, ctorParams[i].Type))
                                        goto NEXT;
                                }
                            }

                            Underlining(singleLocation, ctor, Rule_SymbolDesc,
                                reportAction, symbolToDescription, descAttrToMessage, compilation, token, 0);

                        NEXT:
                            ;
                        }
                    }
                }
                //else if (op is IMemberReferenceOperation memberOp)
                //{
                //    // NOTE: Class.Member consists of FieldReference and ClassReference
                //    var par = memberOp.Parent;
                //    for (; ; )
                //    {
                //        if (par is not IMemberReferenceOperation parOp)
                //            break;
                //        memberOp = parOp;
                //        par = par.Parent;
                //    }
                //    member = memberOp.Member;
                //}
                //else if (op is ITypeOfOperation typeofOp)
                //{
                //    member = typeofOp.TypeOperand;
                //}
            }


            /*  helper  ================================================================ */

            private static void Underlining<T>(T locations,
                                               ISymbol targetSymbol,
                                               DiagnosticDescriptor rule,
                                               Action<Diagnostic> reportAction,
                                               IDictionary<ISymbol, AttributeSyntax?> symbolToDescription,
                                               // works only on small source code file --> IDictionary<string, SemanticModel> filePathToModel,
                                               IDictionary<SyntaxNode, string?> descAttrToMessage,
                                               Compilation compilation,
                                               CancellationToken token,
                                               int dotTokenCount
                )
                where T : IEnumerable<Location>
            {
#if STMG_DEBUG_MESSAGE
                try
#endif
                {
                    int depth = 0;
                    string? description;
                    AttributeSyntax? attr;

                    do
                    {
                        description = null;

                        if (symbolToDescription.TryGetValue(targetSymbol, out attr))
                        {
                            if (attr != null && !descAttrToMessage.TryGetValue(attr, out description))
                            {
                                description = GetDescriptionMessage(attr, targetSymbol, /*filePathToModel,*/ descAttrToMessage, compilation, token);
                            }
                        }
                        else
                        {
                            description = GetAndUpdateDescriptionCache(targetSymbol, symbolToDescription, /*filePathToModel,*/ descAttrToMessage, compilation, token);
                        }

                        if (description == null)
                        {
#if STMG_DEBUG_MESSAGE_VERBOSE
                        ReportDebugMessage(reportAction,
                            targetSymbol.GetType().Name,
                            "=== NO DESCRIPTION === " + targetSymbol,
                            locations);
#endif

                            goto NEXT;
                        }


#if STMG_DEBUG_MESSAGE_VERBOSE
                    description += $" cache:{new Random().Next()}";
#endif

                        var assem = targetSymbol.ContainingAssembly?.Name;
                        if (assem == null)
                        {
                            if (targetSymbol.ContainingNamespace != null)
                            {
                                assem = string.Join(".", targetSymbol.ContainingNamespace.ConstituentNamespaces);
                            }
                            else
                            {
                                assem = "UNKNOWN ASSEMBLY";
                            }
                        }
                        //TODO
                        const string PREFIX_SYMBOL = " in ";
                        Span<char> span = stackalloc char[assem.Length + PREFIX_SYMBOL.Length];
                        PREFIX_SYMBOL.AsSpan().CopyTo(span);
                        assem.AsSpan().CopyTo(span.Slice(PREFIX_SYMBOL.Length));

                        var targetName = targetSymbol.Name;
                        Span<char> quoted = stackalloc char[targetName.Length + 2];
                        quoted[0] = '\'';
                        targetName.AsSpan().CopyTo(quoted.Slice(1));
                        quoted[quoted.Length - 1] = '\'';

                        var args = new object[] { SpanConcat(quoted, span), description };
                        foreach (var location in locations)
                        {
                            ReportDiagnosticPerChar(reportAction, location, rule, args);
                        }

                    NEXT:
                        if (depth++ >= dotTokenCount)
                            break;

                        targetSymbol = targetSymbol.ContainingType;
                    }
                    while (targetSymbol != null);
                }

#if STMG_DEBUG_MESSAGE
                catch (Exception ex)
                {
                    ReportDebugMessage(reportAction,
                        "=== ERROR OCCURED ===",
                        ex.ToString(),
                        locations);
                }
#endif
            }


            // NOTE: don't collect "[Description]". collect only "[DescriptionAttribute]" to prevent
            //       underlining by original Description attribute usage.
            //       - [Description("Description for VS Visual Designer")] int NoUnderlineOnMe;
            //       - [DescriptionAttribute("Draw underline in VS source code editor")] int GetUnderline;
            readonly static string _descriptionAttributeName = nameof(DescriptionAttribute);

            // NOTE: to apply attribute message update, don't cache description string directly.
            //       cache symbol to attribute syntax mapping instead.
            private static string? GetAndUpdateDescriptionCache(ISymbol symbol,
                                                                IDictionary<ISymbol, AttributeSyntax?> symbolToDescription,
                                                                // works only on small source code file --> IDictionary<string, SemanticModel> filePathToModel,
                                                                IDictionary<SyntaxNode, string?> descAttrToMessage,
                                                                Compilation compilation,
                                                                CancellationToken token
                )
            {
                // NOTE: cache existence check is done by caller
                AttributeSyntax? attr = null;
                string? description;

                SyntaxNode declareSyntax;
                var declareSyntaxRefs = symbol.DeclaringSyntaxReferences;

                // NOTE: don't reuse symbol var, dictionary key must be supplied symbol, not base/overridden symbol
                var recursiveTarget = symbol;
                uint recursiveCount = 0;
                //TODO
                const uint MAX_TRAVERSE_DEPTH = 3;

            RESTART_FOREACH:

                //// .ctor cannot be referenced!!!
                //if (!recursiveTarget.CanBeReferencedByName)
                //{
                //    goto SKIP_FOREACH;
                //}

                if (recursiveTarget.ContainingNamespace?.ConstituentNamespaces.FirstOrDefault()?.Name == nameof(System))
                {
                    goto NOT_FOUND;
                }

                // get attributes
                foreach (var declare in declareSyntaxRefs)
                {
                    declareSyntax = declare.GetSyntax(token);

                RESTART_SWITCH:
                    switch (declareSyntax)
                    {
                        case MemberDeclarationSyntax memberDecl:
                            attr = memberDecl.AttributeLists.SelectMany(static x => x.Attributes).FirstOrDefault(static x =>
                            {
                                return x.Name.Span.Length == _descriptionAttributeName.Length
                                    && x.Name.ToString() == _descriptionAttributeName;
                            });
                            break;

                        case AccessorDeclarationSyntax accessorDecl:
                            attr = accessorDecl.AttributeLists.SelectMany(static x => x.Attributes).FirstOrDefault(static x =>
                            {
                                return x.Name.Span.Length == _descriptionAttributeName.Length
                                    && x.Name.ToString() == _descriptionAttributeName;
                            });
                            break;

                        case ParameterSyntax paramDecl:
                            attr = paramDecl.AttributeLists.SelectMany(static x => x.Attributes).FirstOrDefault(static x =>
                            {
                                return x.Name.Span.Length == _descriptionAttributeName.Length
                                    && x.Name.ToString() == _descriptionAttributeName;
                            });
                            break;

                        case TypeParameterSyntax typeParam:
                            attr = typeParam.AttributeLists.SelectMany(static x => x.Attributes).FirstOrDefault(static x =>
                            {
                                return x.Name.Span.Length == _descriptionAttributeName.Length
                                    && x.Name.ToString() == _descriptionAttributeName;
                            });
                            break;

                        case LocalFunctionStatementSyntax localFunc:
                            attr = localFunc.ChildNodes().OfType<AttributeListSyntax>().FirstOrDefault()?.Attributes.FirstOrDefault(static x =>
                            {
                                return x.Name.Span.Length == _descriptionAttributeName.Length
                                    && x.Name.ToString() == _descriptionAttributeName;
                            });
                            break;


                        // label doesn't require underline
                        case LabeledStatementSyntax:
                            break;

                        // public int Property => 100;
                        //                        ^^^
                        // 100 get underline but not required
                        case ArrowExpressionClauseSyntax:
                            break;

                        // tuple should not get underline
                        case TupleTypeSyntax:
                        case TupleElementSyntax:
                            break;

                        // syntax used when code has error
                        // ex) root level method declaration --> void Test() { }
                        case CompilationUnitSyntax:
                            break;

                        // xml doc comment `cref="GenericType{T1, T2}"` can have this syntax
                        //                                    ^^  ^^
                        case IdentifierNameSyntax:
                            {
                                var par = declareSyntax.Parent;

                                //cref??
                                if (par is TypeArgumentListSyntax)
                                {
                                    par = par.Parent;
                                    if (par is not GenericNameSyntax)
                                        goto default;

                                    par = par.Parent;
                                    if (par is not CrefSyntax)
                                        goto default;
                                }
                                else
                                {
                                    goto default;
                                }
                            }
                            break;

                        // var anonymous = new { member = something };
                        // ^^^                   ^^^^^^
                        // creationExpr          memberDeclarator
                        case AnonymousObjectCreationExpressionSyntax:
                        case AnonymousObjectMemberDeclaratorSyntax:
                            break;


                        // NOTE: check at last!!!!
                        case VariableDeclaratorSyntax:
                            {
                                var par = declareSyntax.Parent;
                                if (par is VariableDeclarationSyntax)
                                {
                                    if (par.Parent is MemberDeclarationSyntax)//FieldDeclarationSyntax or EventFieldDeclarationSyntax)
                                    {
                                        declareSyntax = par.Parent;
                                        goto RESTART_SWITCH;  //tricky!!
                                    }
                                }
                            }
                            break;


                        //unsupported
                        default:
                            return SpanConcat("[SMA UNSUPPORTED]: ".AsSpan(), (declareSyntax?.Kind().ToString() ?? string.Empty).AsSpan());
                    }


                    if (attr == null)
                    {
                        continue;
                    }


#if STMG_USE_ATTRIBUTE_CACHE
                    // NOTE: existence check is done by caller
                    symbolToDescription[symbol] = attr;
#endif

                    if (descAttrToMessage.TryGetValue(attr, out description))
                        return description;

                    return GetDescriptionMessage(attr, symbol, /*filePathToModel,*/ descAttrToMessage, compilation, token);
                }


            SKIP_FOREACH:

                // NOTE: when searching base/overridden symbol perfectly, it makes analyzing EXTREMELY slow!!
                if (recursiveCount++ < MAX_TRAVERSE_DEPTH)
                {
                    if (symbol.IsOverride)
                    {
                        switch (symbol)
                        {
                            case IMethodSymbol M when !M.OverriddenMethod.IsImplicitlyDeclared:
                                {
                                    declareSyntaxRefs = M.OverriddenMethod.DeclaringSyntaxReferences;
                                }
                                goto RESTART_FOREACH;

                            case IPropertySymbol P when !P.OverriddenProperty.IsImplicitlyDeclared:
                                {
                                    declareSyntaxRefs = P.OverriddenProperty.DeclaringSyntaxReferences;
                                }
                                goto RESTART_FOREACH;

                            case IEventSymbol E when !E.OverriddenEvent.IsImplicitlyDeclared:
                                {
                                    declareSyntaxRefs = E.OverriddenEvent.DeclaringSyntaxReferences;
                                }
                                goto RESTART_FOREACH;
                        }
                    }

                    // base type
                    else
                    {
                        if (recursiveTarget is INamedTypeSymbol T && T.BaseType is INamedTypeSymbol B && !B.IsImplicitlyDeclared)
                        {
                            recursiveTarget = B;
                            declareSyntaxRefs = B.DeclaringSyntaxReferences;
                            goto RESTART_FOREACH;
                        }
                    }
                }


            NOT_FOUND:

#if STMG_USE_ATTRIBUTE_CACHE
                // NOTE: existence check is done by caller
                symbolToDescription[symbol] = null;
#endif

                return null;
            }


            private static string? GetDescriptionMessage(AttributeSyntax attr,
                                                         ISymbol symbol,
                                                         // works only on small source code file --> IDictionary<string, SemanticModel> filePathToModel,
                                                         IDictionary<SyntaxNode, string?> descAttrToMessage,
                                                         Compilation compilation,
                                                         CancellationToken token
                )
            {
                // NOTE: cache existence check is done by caller
                string? description = null;

                SeparatedSyntaxList<AttributeArgumentSyntax> attrArgs;
                SyntaxTree attrTree;
                Optional<object> attrValue;

                // NOTE: parameter-less attribute constructor has null argument list
                if (attr.ArgumentList != null
                && (attrArgs = attr.ArgumentList.Arguments).Count == 1)
                {
                    attrTree = attrArgs[0].SyntaxTree;
                    // NOTE: DON'T USE CACHE!!!!
                    //       THROWS EXCEPTION!!!!!!
                    /*
                                    if (!filePathToModel.TryGetValue(attrTree.FilePath, out var model))
                                    {
                                        model = compilation.GetSemanticModel(attrTree);
                                        filePathToModel.Add(attrTree.FilePath, model);
                                    }
                    */
                    var model = compilation.GetSemanticModel(attrTree);

                    attrValue = model.GetConstantValue((attrArgs[0].Expression as ExpressionSyntax), token);
                    if (attrValue.HasValue)
                    {
                        description = attrValue.ToString();
                    }
                    else
                    {
                        // NOTE: when constant value cannot be retrieved, assume that user is typing source code in IDE
                        //       so that don't try to update cache at this moment.
                        return null;
                    }
                }


                if (string.IsNullOrWhiteSpace(description))
                {
                    var name = symbol.Name;

                    Span<char> span = stackalloc char[name.Length + 2];
                    span[0] = '\'';
                    name.AsSpan().CopyTo(span.Slice(1));
                    span[span.Length - 1] = '\'';

                    description = SpanConcat(span, " has Description attribute w/o args".AsSpan());
                }

#if STMG_DEBUG_MESSAGE_VERBOSE
            description += $" (from:{symbol.Kind})";
#endif


#if STMG_USE_DESCRIPTION_CACHE
                // NOTE: existence checked is done in caller!!
                descAttrToMessage[attr] = description;
#endif

                return description;
            }


            private static void ReportDiagnosticPerChar(Action<Diagnostic> reportAction,
                                                        Location location,
                                                        DiagnosticDescriptor rule,
                                                        object[] messageArgs
                )
            {
                //warning??
                if (messageArgs[1] is string s && s.StartsWith("!", StringComparison.Ordinal))
                {
                    reportAction.Invoke(Diagnostic.Create(Rule_WarningDesc, location, messageArgs));
                    return;
                }


                int locStart = location.SourceSpan.Start;
                int locEnd = location.SourceSpan.End;
                var tree = location.SourceTree;

                __DrawUnderlinePerChar(reportAction, tree, locStart, locEnd, rule, messageArgs);


                // add line annotation
                //TODO
                const int length = 2;
                const int offset = length + 1;

                var spanInLine = location.GetLineSpan();
                if (spanInLine.IsValid)
                {
                    var treeText = tree.GetText();

                    int startInLine = spanInLine.StartLinePosition.Character;
                    if (startInLine > offset)
                    {
                        int start = locStart - startInLine;

                        //head!!
                        int linehead = start + 1;
                        reportAction.Invoke(Diagnostic.Create(
                            Rule_LineHeadDesc, tree.GetLocation(new(linehead, length)), messageArgs));

                        // find non-whitespace
                        var lineheadText = treeText.GetSubText(start);
                        for (int i = 0; i < startInLine; i++)
                        {
                            if (!char.IsWhiteSpace(lineheadText[i]))
                                break;
                            start++;
                        }

                        //lead!!
                        start -= offset;
                        if (start > linehead)
                        {
                            if (start < locStart)
                            {
                                reportAction.Invoke(Diagnostic.Create(
                                    Rule_LineLeadingDesc, tree.GetLocation(new(start, length)), messageArgs));
                            }
                        }

#if STMG_ENABLE_LINE_FILL
                        start += offset;
                        int linefillEnd = locStart - 1;

                        // add line fill
                        if (start < linefillEnd)
                        {
                            __DrawUnderlinePerChar(reportAction, tree, start, linefillEnd, Rule_LineFillDesc, messageArgs);
                        }
#endif
                    }

                    //lineEnd!!
                    int lineEnd = locEnd;
                    var lineEndText = treeText.GetSubText(lineEnd);
                    for (int i = 0; i < lineEndText.Length; i++)
                    {
                        if (lineEndText[i] is '\n' or '\r')
                            break;
                        lineEnd++;
                    }


                    reportAction.Invoke(Diagnostic.Create(
                        Rule_LineEndDesc, tree.GetLocation(new(lineEnd, 0)), messageArgs));  // TextSpan end is inclusive. 0 is allowed
                }
            }


            /// <summary>
            /// When report `Info` severity diagnostic on full span, VisualStudio draws underline only on first a few chars, not on whole span.
            /// To workaround, split report into multiple reports for no overlapping/connected span.
            /// </summary>
            private static void __DrawUnderlinePerChar(Action<Diagnostic> reportAction,
                                                       SyntaxTree tree,
                                                       int start,
                                                       int end,
                                                       DiagnosticDescriptor rule,
                                                       object[] messageArgs
                )
            {
                int lineLength = 2;
                int increment = lineLength + 1;

                Location loc;
                int remain;
                for (int i = start; i < end; i += increment)  // NOTE: connected underline cannot be drawn... need a gap
                {
                    remain = end - i;
                    if (remain == increment)
                    {
                        if (increment > 2)
                        {
                            lineLength -= 1;
                            increment -= 1;
                        }
                        else
                        {
                            lineLength = 2;
                        }
                    }

                    loc = tree.GetLocation(new(i, Math.Min(lineLength, remain)));
                    reportAction.Invoke(Diagnostic.Create(rule, loc, messageArgs));
                }
            }

        }


        /*  static member analysis  ================================================================ */

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public sealed class StaticMemberImpl : DiagnosticAnalyzer
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
                Rule_DEBUG,
#endif
                Rule_WrongInit,
                Rule_CrossRef,
                Rule_AnotherFile,
                Rule_LateDeclare
                );


            public override void Initialize(AnalysisContext context)
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
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


            /*  register  ================================================================ */

            const int DEFAULT_LIST_CAPACITY = 4;

            [ThreadStatic, DescriptionAttribute] static Dictionary<string, SemanticModel>? ts_filePathToModel;
            [ThreadStatic, DescriptionAttribute] static HashSet<string>? ts_declaredMemberSet;
            [ThreadStatic, DescriptionAttribute] static HashSet<IMemberReferenceOperation>? ts_crossRefReportedSet;
            [ThreadStatic, DescriptionAttribute] static List<FieldDeclarationSyntax>? ts_crossFDSyntaxList;
            [ThreadStatic, DescriptionAttribute] static List<ISymbol>? ts_foundSymbolList;
            [ThreadStatic, DescriptionAttribute] static List<IMemberReferenceOperation>? ts_refOperatorList;
            [ThreadStatic, DescriptionAttribute] static List<IMemberReferenceOperation>? ts_crossRefOperatorList;


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
                //if (!filePathToModel.ContainsKey(context.SemanticModel.SyntaxTree.FilePath))
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
                                var a = refOp.Member.DeclaringSyntaxReferences.SingleOrDefault()?.SyntaxTree.FilePath;
                                var b = declaredMemberSymbolList[i].DeclaringSyntaxReferences.SingleOrDefault()?.SyntaxTree.FilePath;

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
                                var prefix = GetMemberNamePrefix(refOp.Member.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax(token));
                                if (!declaredMemberSet.Contains(SpanConcat(prefix.AsSpan(), refOp.Member.Name.AsSpan())))
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
                        var prefix = GetMemberNamePrefix(fieldSyntax);

                        declaredMemberSet.UnionWith(fieldSyntax.Declaration.Variables.Select(x =>
                        {
                            return SpanConcat(prefix.AsSpan(), x.Identifier.Text.AsSpan());
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
                    if (initOp.Children.SingleOrDefault() is IDelegateCreationOperation)
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
}
