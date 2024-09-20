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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposableAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      DESCRIPTOR      = */

        public const string RuleId_NotUsing = "SMA0040";
        private static readonly DiagnosticDescriptor Rule_NotUsing = new(
            RuleId_NotUsing,
            new LocalizableResourceString(nameof(Resources.SMA0040_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0040_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0040_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DEBUG,
#endif
            Rule_NotUsing
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeDisposableUsage, ImmutableArray.Create(
                OperationKind.ObjectCreation,
                OperationKind.Invocation,
                OperationKind.Conversion
                ));


            //context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        //private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
        //{
        //}


        /*  entry  ================================================================ */

        private static void AnalyzeDisposableUsage(OperationAnalysisContext context)
        {
            INamedTypeSymbol? disposableSymbol = null;

            switch (context.Operation)
            {
                case IObjectCreationOperation createOp:
                    {
                        disposableSymbol = createOp.Type as INamedTypeSymbol;
                    }
                    break;

                case IInvocationOperation invokeOp:
                    {
                        disposableSymbol = invokeOp.TargetMethod.ReturnType as INamedTypeSymbol;
                    }
                    break;

                case IConversionOperation castOp:
                    {
                        disposableSymbol = castOp.Type as INamedTypeSymbol;
                    }
                    break;
            }


            if (disposableSymbol == null)
                return;

            if (!IsSymbolDisposable(context, disposableSymbol))
                return;


            // find `using` statement and return
            var syntax = context.Operation.Syntax;

            // > using(new Disposable()) { ... }
            if (syntax.Parent is UsingStatementSyntax)
            {
                return;
            }

            // > using var x = new Disposable();
            // > using(var x = new Disposable()) { ... }
            if (syntax.Parent is EqualsValueClauseSyntax equalsStx
             && equalsStx.Parent is VariableDeclaratorSyntax declaratorStx
             && declaratorStx.Parent is VariableDeclarationSyntax varDeclStx
            )
            {
                var parentStx = varDeclStx.Parent;
                if (parentStx is UsingStatementSyntax or MemberDeclarationSyntax)
                {
                    return;
                }

                if (parentStx is LocalDeclarationStatementSyntax localVarStx)
                {
                    var token = localVarStx.GetFirstToken();
                    if (token.IsKind(SyntaxKind.UsingKeyword))
                    {
                        return;
                    }
                    else if (token.IsKind(SyntaxKind.AwaitKeyword))
                    {
                        token = token.GetNextToken();
                        if (token.IsKind(SyntaxKind.UsingKeyword))
                        {
                            return;
                        }
                    }
                }
            }


            // NOT FOUND...!!
            context.ReportDiagnostic(Diagnostic.Create(
                Rule_NotUsing, syntax.GetLocation(), disposableSymbol.Name));
        }


        readonly static Func<INamedTypeSymbol, bool> IsDisposableFunc = static x =>
        {
            if (x.SpecialType is SpecialType.System_IDisposable)
            {
                return true;
            }
            // TODO: SpecialType enum item for 'IAsyncDisposable'
            else if (x.Name == "IAsyncDisposable")
            {
                var ns = x.ContainingNamespace;
                if (ns.Name == nameof(System) && ns.ContainingNamespace.IsGlobalNamespace)
                {
                    return true;
                }
            }
            return false;
        };

        private static bool IsSymbolDisposable(OperationAnalysisContext context,
                                               INamedTypeSymbol disposableSymbol
            )
        {
            if (!disposableSymbol.IsRefLikeType)
            {
                if (disposableSymbol.Interfaces.Any(IsDisposableFunc)
                 || disposableSymbol.AllInterfaces.Any(IsDisposableFunc)
                )
                {
                    return true;
                }

                return false;
            }


            const Accessibility ACCESS_HIDDEN = Accessibility.Protected | Accessibility.Private | Accessibility.NotApplicable;

            var candidateMethods = disposableSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(static x => x.Parameters.Length == 0 && (x.DeclaredAccessibility & ~ACCESS_HIDDEN) != 0)
                ;

            var isDisposable = candidateMethods
                .Where(static x => x.Name == nameof(IDisposable.Dispose))
                .Any(static x => x.ReturnType.SpecialType == SpecialType.System_Void)
                ;
            if (!isDisposable)
            {
                var isAsyncDisposable = candidateMethods
                    .Where(static x => x.Name == "DisposeAsync")
                    .Any(static x =>
                    {
                        // TODO: SpecialType enum item for 'ValueTask'
                        if (x.ReturnType.Name != nameof(ValueTask))
                            return false;

                        var ns = x.ReturnType.ContainingNamespace;
                        if (ns.Name != nameof(System.Threading.Tasks)
                         || ns.ContainingNamespace.Name != nameof(System.Threading)
                         || ns.ContainingNamespace.ContainingNamespace.Name != nameof(System)
                         || !ns.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
                        )
                        {
                            return false;
                        }

                        return true;
                    })
                    ;

                if (!isAsyncDisposable)
                    return false;
            }

            return true;
        }

    }
}
