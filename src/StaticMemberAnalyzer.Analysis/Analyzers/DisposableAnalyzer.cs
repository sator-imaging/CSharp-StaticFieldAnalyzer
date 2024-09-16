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

            context.RegisterOperationAction(AnalyzeDisposableUsage,
                ImmutableArray.Create(OperationKind.ObjectCreation, OperationKind.AnonymousObjectCreation));


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

                case IAnonymousObjectCreationOperation anonyCreateOp:
                    {
                        disposableSymbol = anonyCreateOp.Type as INamedTypeSymbol;
                    }
                    break;
            }


            if (disposableSymbol == null)
                return;

            const Accessibility ACCESS_HIDDEN = Accessibility.Protected | Accessibility.Private | Accessibility.NotApplicable;

            var methods = disposableSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(static x => x.Parameters.Length == 0 && x.ReturnType.SpecialType == SpecialType.System_Void)
                .Where(static x => (x.DeclaredAccessibility & ~ACCESS_HIDDEN) != 0)
                .Where(static x => x.Name == nameof(IDisposable.Dispose))
                ;

            if (!methods.Any())
                return;

            // find using
            if (context.Operation.Syntax.Parent is EqualsValueClauseSyntax equalsStx
             && equalsStx.Parent is VariableDeclaratorSyntax declaratorStx
             && declaratorStx.Parent is VariableDeclarationSyntax varDeclStx
            )
            {
                var parentStx = varDeclStx.Parent;
                if (parentStx is UsingStatementSyntax or MemberDeclarationSyntax)
                {
                    return;
                }

                if (parentStx is LocalDeclarationStatementSyntax localVarStx && localVarStx.GetFirstToken().IsKind(SyntaxKind.UsingKeyword))
                {
                    return;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule_NotUsing, context.Operation.Syntax.GetLocation(), disposableSymbol.Name));
        }
    }
}
