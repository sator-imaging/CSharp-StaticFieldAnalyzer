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

        public const string RuleId_MissingUsing = "SMA0040";
        private static readonly DiagnosticDescriptor Rule_MissingUsing = new(
            RuleId_MissingUsing,
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
            Rule_MissingUsing
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeDisposableUsage, ImmutableArray.Create(
                OperationKind.ObjectCreation,
                OperationKind.Invocation,
                OperationKind.Conversion,
                OperationKind.PropertyReference
                ));


            //context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        //private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
        //{
        //}


        /*  entry  ================================================================ */

        private static void AnalyzeDisposableUsage(OperationAnalysisContext context)
        {
            INamedTypeSymbol? disposableSymbol;

            // NOTE: if cast happens between disposable types, do not report diagnostic
            //       > disposableLocalVar as IDisposable
            //       > (IDisposable)disposableLocalVar
            if (context.Operation is IConversionOperation castOp)
            {
                disposableSymbol = castOp.Type as INamedTypeSymbol;

                if (disposableSymbol == null || !IsSymbolDisposable(context, disposableSymbol))
                    return;

                // cast from disposable type?
                if (castOp.Operand.Type is INamedTypeSymbol sourceSymbol && IsSymbolDisposable(context, sourceSymbol))
                    return;
            }
            else
            {
                disposableSymbol = context.Operation switch
                {
                    IObjectCreationOperation x => x.Type as INamedTypeSymbol,
                    IInvocationOperation x => x.TargetMethod.ReturnType as INamedTypeSymbol,
                    IPropertyReferenceOperation x => x.Type as INamedTypeSymbol,
                    _ => null
                };

                if (disposableSymbol == null || !IsSymbolDisposable(context, disposableSymbol))
                    return;
            }


            // NOTE: if disposable type symbol is found, check the following
            //       - is it used as method call receiver?
            //       - does `using` statement exist?
            //       - on return statement?

            var syntax = context.Operation.Syntax;
            SemanticModel? model = null;

            // remove parenthesizes!!
            // > (((new Disposable()))) --> new Disposable()
            while (syntax.Parent is ParenthesizedExpressionSyntax || syntax.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            {
                syntax = syntax.Parent;
            }

            // > Method() => new Disposable();
            // > Method() { return new Disposable(); }
            if (syntax.Parent is ArrowExpressionClauseSyntax or ReturnStatementSyntax)
            {
                return;
            }

            // > new Disposable().Return();
            // > new Disposable().Property;
            if (syntax.Parent is InvocationExpressionSyntax or MemberAccessExpressionSyntax)
            {
                return;
            }
            // > disposable?.XXX...
            else if (syntax.Parent is ConditionalAccessExpressionSyntax conditionalStx)
            {
                if (conditionalStx.WhenNotNull.Kind() is SyntaxKind.InvocationExpression or SyntaxKind.SimpleMemberAccessExpression)
                {
                    return;
                }
            }


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
                else if (parentStx is LocalDeclarationStatementSyntax localVarStx)
                {
                    model ??= context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                    if (model.GetTypeInfo(localVarStx.Declaration.Type).ConvertedType is INamedTypeSymbol localVarSymbol
                    && !IsSymbolDisposable(context, localVarSymbol))
                    {
                        return;
                    }

                    if (localVarStx.UsingKeyword != default)
                    {
                        return;
                    }

                    /* var token = localVarStx.GetFirstToken();
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
                    */
                }
            }
            // NOTE: ignore field/property assignment even if field/property type is disposable
            //       > Field = new Disposable();
            //       > Property = new Disposable();
            else if (syntax.Parent is AssignmentExpressionSyntax assignStx)
            {
                model ??= context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                var leftSymbol = model.GetSymbolInfo(assignStx.Left).Symbol;

                //ummm.....
                (bool ignorable, INamedTypeSymbol? foundSymbol) = leftSymbol switch
                {
                    // NOTE: allow field/property assignment!!
                    IFieldSymbol fieldSymbol => (true, fieldSymbol.Type as INamedTypeSymbol),
                    IPropertySymbol propertySymbol => (true, propertySymbol.Type as INamedTypeSymbol),

                    ILocalSymbol localVarSymbol => (false, localVarSymbol.Type as INamedTypeSymbol),
                    ILocalReferenceOperation localRefSymbol => (false, localRefSymbol.Type as INamedTypeSymbol),
                    _ => (false, null)
                };

                if (ignorable || (foundSymbol != null && !IsSymbolDisposable(context, foundSymbol)))
                {
                    return;
                }
            }


            // NOT FOUND...!!
            context.ReportDiagnostic(Diagnostic.Create(
                Rule_MissingUsing, syntax.GetLocation(), (((disposableSymbol!))).Name));
        }


        /*  internal  ================================================================ */

        readonly static Func<INamedTypeSymbol, bool> cache_HasDisposableImplemented = static x =>
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
                if (cache_HasDisposableImplemented.Invoke(disposableSymbol)
                 || disposableSymbol.Interfaces.Any(cache_HasDisposableImplemented)
                 || disposableSymbol.AllInterfaces.Any(cache_HasDisposableImplemented)
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
