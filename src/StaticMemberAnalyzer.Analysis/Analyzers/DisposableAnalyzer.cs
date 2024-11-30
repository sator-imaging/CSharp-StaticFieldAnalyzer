﻿#define STMG_DEBUG_MESSAGE
#if DEBUG == false
#undef STMG_DEBUG_MESSAGE
#endif

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
            Core.Rule_DebugError,
            Core.Rule_DebugWarn,
#endif
            Rule_MissingUsing
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeCast, OperationKind.Conversion);
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            context.RegisterOperationAction(AnalyzeUsualCreation, OperationKind.ObjectCreation);
            context.RegisterOperationAction(AnalyzeAnonymousCreation, OperationKind.AnonymousObjectCreation);
            context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
            context.RegisterOperationAction(AnalyzeArrayElementReference, OperationKind.ArrayElementReference);
        }


        /*  entry  ================================================================ */

        private static void AnalyzeCast(OperationAnalysisContext context)
        {
            if (context.Operation is not IConversionOperation op)
                return;

            bool isResultDisposable = op.Type is INamedTypeSymbol resultSymbol && IsDisposable(context, resultSymbol);
            bool isSourceDisposable = op.Operand.Type is INamedTypeSymbol sourceSymbol && IsDisposable(context, sourceSymbol);

            // both are disposable OR both are not disposable
            if (isResultDisposable == isSourceDisposable)
            {
                return;
            }

            CheckAssignmentAndUsingStatementExistence(context, op, op.Type);
        }


        private static void AnalyzeInvocation(OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation op)
                return;

            var returnSymbol = op.TargetMethod.ReturnType;
            if (returnSymbol is not INamedTypeSymbol named || !IsDisposable(context, named))
                return;

            CheckAssignmentAndUsingStatementExistence(context, op, returnSymbol);
        }


        private static void AnalyzeUsualCreation(OperationAnalysisContext context)
        {
            if (context.Operation is not IObjectCreationOperation op)
                return;

            if (op.Type is not INamedTypeSymbol named || !IsDisposable(context, named))
                return;

            CheckAssignmentAndUsingStatementExistence(context, op, op.Type);
        }

        private static void AnalyzeAnonymousCreation(OperationAnalysisContext context)
        {
            if (context.Operation is not IAnonymousObjectCreationOperation op)
                return;

            if (op.Type is not INamedTypeSymbol named || !IsDisposable(context, named))
                return;

            CheckAssignmentAndUsingStatementExistence(context, op, op.Type);
        }


        private static void AnalyzePropertyReference(OperationAnalysisContext context)
        {
            if (context.Operation is not IPropertyReferenceOperation op)
                return;

            if (op.Type is not INamedTypeSymbol named || !IsDisposable(context, named))
                return;

            // ignore right hand
            if (op.Parent is IAssignmentOperation assignOp)
            {
                if (op == assignOp.Value)
                    return;
            }
            else
            {
                if (op.Syntax.Parent is EqualsValueClauseSyntax equalsStx
                 && op.Syntax == equalsStx.Value
                )
                {
                    return;
                }
            }

            CheckAssignmentAndUsingStatementExistence(context, op, op.Type);
        }

        private static void AnalyzeArrayElementReference(OperationAnalysisContext context)
        {
            if (context.Operation is not IArrayElementReferenceOperation op)
                return;

            if (op.Type is not INamedTypeSymbol named || !IsDisposable(context, named))
                return;

            // ignore right hand
            if (op.Parent is IAssignmentOperation assignOp)
            {
                if (op == assignOp.Value)
                    return;
            }
            else
            {
                if (op.Syntax.Parent is EqualsValueClauseSyntax equalsStx
                 && op.Syntax == equalsStx.Value
                )
                {
                    return;
                }
            }

            CheckAssignmentAndUsingStatementExistence(context, op, op.Type);
        }


        /*  internal  ================================================================ */

#pragma warning disable RS1008

        readonly static Func<INamedTypeSymbol, bool> HasDisposableImplemented = static x =>
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

#pragma warning restore RS1008


        private static bool IsDisposable(OperationAnalysisContext context,
                                         INamedTypeSymbol disposableSymbol
            )
        {
            if (IsTypeIgnoredByAssemblyAttribute(context, disposableSymbol))
            {
                return false;
            }

            if (!disposableSymbol.IsRefLikeType)
            {
                if (disposableSymbol.Name.StartsWith(nameof(Task), StringComparison.Ordinal)
                 && disposableSymbol.ContainingNamespace.Name == nameof(System.Threading.Tasks)
                 && disposableSymbol.ContainingNamespace.ContainingNamespace.Name == nameof(System.Threading)
                 && disposableSymbol.ContainingNamespace.ContainingNamespace.ContainingNamespace.Name == nameof(System)
                 && disposableSymbol.ContainingNamespace.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
                )
                {
                    return false;
                }

                if (HasDisposableImplemented.Invoke(disposableSymbol)
                 || disposableSymbol.Interfaces.Any(HasDisposableImplemented)
                 || disposableSymbol.AllInterfaces.Any(HasDisposableImplemented)
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


        private static bool IsTypeIgnoredByAssemblyAttribute(OperationAnalysisContext context, INamedTypeSymbol disposableSymbol)
        {
            const string ATTR_NAME = nameof(DisposableAnalyzer);

            foreach (var attr in context.Compilation.Assembly.GetAttributes())
            {
                if (attr.AttributeClass.Name.StartsWith(ATTR_NAME, StringComparison.Ordinal))
                {
                    foreach (var ctorArg in attr.ConstructorArguments)
                    {
                        if (ctorArg.Kind != TypedConstantKind.Array)
                        {
                            if (ctorArg.Value is ITypeSymbol typeSymbol && SymbolEqualityComparer.Default.Equals(disposableSymbol, typeSymbol))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            foreach (var argument in ctorArg.Values)
                            {
                                //Core.ReportDebugMessage(context.ReportDiagnostic, context.Operation);

                                if (argument.Value is ITypeSymbol typeSymbol && SymbolEqualityComparer.Default.Equals(disposableSymbol, typeSymbol))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }


        /*  check  ================================================================ */

        private static void CheckAssignmentAndUsingStatementExistence(OperationAnalysisContext context,
                                                                      IOperation op,
                                                                      ITypeSymbol? disposableSymbol
            )
        {
            if (disposableSymbol == null)
                return;

            // MUST check before unpacking implicit cast
            bool isCreationOp = op is IObjectCreationOperation
                                   or IAnonymousObjectCreationOperation
                                   or ITypeParameterObjectCreationOperation
                                   or IDefaultValueOperation
                                   ;

            // NOTE: unpack implicit cast operation
            //       --> Method(new Disposable())
            //                  ^^^^^^^^^^^^^^^^ implicit cast may happen
            {
                IConversionOperation? castOp = op.Parent as IConversionOperation;
                while (castOp != null)
                {
                    if (castOp.IsImplicit && castOp.Type is INamedTypeSymbol named && IsDisposable(context, named))
                    {
                        op = castOp;
                        disposableSymbol = op.Type;
                    }
                    castOp = castOp.Parent as IConversionOperation;
                }
            }


            // NOTE: this code can check using 'block' statement only
            //       --> using (new Disposable()) { ... }
            // NOTE: block-less using statement cannot be checked! it checked later!!
            //       --> using var x = new...
            {
                if (op.Parent is IUsingOperation)
                {
                    goto NO_WARN;
                }
            }


            // method argument?
            {
                if (op.Parent is IArgumentOperation)
                {
                    if (!isCreationOp)
                    {
                        goto NO_WARN;
                    }
                }
            }


            var memberRefOrInvokeOp = Core.UnpackNullCoalesceOperation(op);

            // member reference!!
            // --> disposable.Property;
            // --> disposable?.Property;
            {
                if (memberRefOrInvokeOp is IMemberReferenceOperation memberRefOp)
                {
                    if (!isCreationOp)
                    {
                        if (memberRefOp.Type is INamedTypeSymbol named && !IsDisposable(context, named))
                        {
                            goto NO_WARN;
                        }
                        else
                        {
                            var parentOp = Core.UnpackNullCoalesceOperation(memberRefOrInvokeOp.Parent);
                            if (parentOp != null)
                            {
                                if (parentOp is IMemberReferenceOperation or ILocalReferenceOperation)
                                {
                                    goto NO_WARN;
                                }
                                else
                                {
                                    // NOTE: need to check subsequent method chain
                                    //       --> ...Prop.ToString();
                                    //                   ^^^^^^^^^^
                                    memberRefOrInvokeOp = parentOp;
                                }
                            }
                        }
                    }
                }
            }

            // method receiver!!
            // --> disposable.Return();
            // --> disposable?.Return();
            {
                if (memberRefOrInvokeOp is IInvocationOperation invokeOp)
                {
                    if (!isCreationOp)
                    {
                        if (invokeOp.TargetMethod.ReturnType is INamedTypeSymbol named && !IsDisposable(context, named))
                        {
                            goto NO_WARN;
                        }
                        else
                        {
                            //Core.ReportDebugMessage(context.ReportDiagnostic, op);

                            // check original operation type to determine code path is redirect from member ref or not
                            if (op is IMemberReferenceOperation)
                            {
                                goto NO_WARN;
                            }
                        }
                    }
                }
            }


            // NOTE: in the following, cannot use I***Operation to analyze usage
            //       because unity doesn't allow using latest roslyn analyzer
            var syntax = op.Syntax;


            // NOTE: if switch arm expression found, move focus to parent expression
            //       > var x = value switch { ... };
            //                              ~~~~~~~ current focus
            //       > var x = value switch { ... };
            //                 ~~~~~~~~~~~~~~~~~~~~ moving to here
            {
                if (op.Parent is ISwitchExpressionArmOperation switchArmOp
                 && switchArmOp.Parent is ISwitchExpressionOperation switchOp
                )
                {
                    syntax = switchOp.Syntax;
                }
            }


            // NOTE: remove parenthesizes and null warning suppressor!!
            //       --> (((new Disposable()))) --> new Disposable()
            //       --> (new Disposable())! --> new Disposable()
            syntax = Core.UnpackParenthesizeAndNullCoalesceNodes(syntax);


            // return statement?
            // --> Method() => new Disposable();
            // --> Method() { return new Disposable(); }
            {
                if (syntax.Parent is ArrowExpressionClauseSyntax or ReturnStatementSyntax)
                {
                    goto NO_WARN;
                }
            }


            // NOTE: IUsingOperation is not pointing to block-less using syntax --> using var x = ...
            if (syntax.Parent is EqualsValueClauseSyntax equalsStx)
            {
                // using statement w/o block scope?
                // --> using var x = new Disposable();
                // --> using(var x = new Disposable()) { ... }
                if (equalsStx.Parent is VariableDeclaratorSyntax declaratorStx
                 && declaratorStx.Parent is VariableDeclarationSyntax varDeclStx
                )
                {
                    var parStx = varDeclStx.Parent;
                    if (parStx is UsingStatementSyntax or MemberDeclarationSyntax)
                    {
                        goto NO_WARN;
                    }
                    else if (parStx is LocalDeclarationStatementSyntax localVarStx)
                    {
                        // DON'T check localVarStx variable type is disposable or not
                        // just check using keyword existence
                        if (localVarStx.UsingKeyword != default)
                        {
                            goto NO_WARN;
                        }
                    }
                }
            }
            else
            {
                // NOTE: ignore field/property assignment even if field/property type is disposable
                //       --> Field = new Disposable();
                //       --> Property = new Disposable();
                if (syntax.Parent is AssignmentExpressionSyntax assignStx)
                {
                    var leftStx = assignStx.Left;

                    var model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                    var leftSymbol = model.GetSymbolInfo(leftStx).Symbol;

                    // left hand is indexer?
                    if (leftStx is ElementAccessExpressionSyntax elementAccessStx)
                    {
                        // array[0] returns null, list[0] returns non-null
                        var nonArraySymbol = model.GetSymbolInfo(elementAccessStx.Expression).Symbol;
                        if (nonArraySymbol != null)
                        {
                            leftSymbol = nonArraySymbol;
                        }
                    }

                    // ignore field/property
                    if (leftSymbol != null && (leftSymbol.Kind is SymbolKind.Field or SymbolKind.Property))
                    {
                        // don't allow cast and forget
                        // NG --> m_objectField = (new Disposable()) as object;
                        if (!isCreationOp
                        || op.Parent is not IConversionOperation castOp
                        || (castOp.Type is INamedTypeSymbol named && IsDisposable(context, named))
                        )
                        {
                            goto NO_WARN;
                        }
                    }
                }
                // --> if (disposable == ...)
                // --> switch (disposable) ...
                else if (op.Parent is IBinaryOperation)
                {
                    // don't allow creation operation pass the warning
                    if (!isCreationOp)
                    {
                        goto NO_WARN;
                    }
                }
            }


            // !! REPORT !!
            context.ReportDiagnostic(Diagnostic.Create(
                Rule_MissingUsing, syntax.GetLocation(), disposableSymbol.Name));


            //Core.ReportDebugMessage(context.ReportDiagnostic,
            //    "WARN",
            //    op.Syntax.GetLocation(),
            //    "target: " + op.Kind,
            //    "parent: " + op.Parent.Kind,
            //    "symbol: " + disposableSymbol.Name
            //    );

            return;

        NO_WARN:
            //Core.ReportDebugMessage(context.ReportDiagnostic,
            //    "NO WARN",
            //    op.Syntax.GetLocation(),
            //    "target: " + op.Kind,
            //    "parent: " + op.Parent.Kind,
            //    "symbol: " + disposableSymbol.Name
            //    );

            return;
        }

    }
}
