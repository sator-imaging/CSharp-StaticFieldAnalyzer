#define STMG_DEBUG_MESSAGE
#if DEBUG == false
#undef STMG_DEBUG_MESSAGE
#endif

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StructAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      DESCRIPTOR      = */

        public const string RuleId_InvalidStructCtor = "SMA0030";
        private static readonly DiagnosticDescriptor Rule_InvalidStructCtor = new(
            RuleId_InvalidStructCtor,
            new LocalizableResourceString(nameof(Resources.SMA0030_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0030_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0030_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DebugError,
            Core.Rule_DebugWarn,
#endif
            Rule_InvalidStructCtor
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeUsualConstructor, OperationKind.ObjectCreation);
            context.RegisterOperationAction(AnalyzeAnonymousConstructor, OperationKind.AnonymousObjectCreation);
        }


        /*  entry  ================================================================ */

        private static void AnalyzeUsualConstructor(OperationAnalysisContext context)
        {
            if (context.Operation is not IObjectCreationOperation op || !op.Type.IsValueType)
                return;

            if (op.Arguments.Length == 0 && op.Type is INamedTypeSymbol namedSymbol)
            {
                AnalyzeConstructor_Impl(context, namedSymbol);
            }
        }

        private static void AnalyzeAnonymousConstructor(OperationAnalysisContext context)
        {
            if (context.Operation is not IAnonymousObjectCreationOperation op || !op.Type.IsValueType)
                return;

            if (!op.Children.OfType<IArgumentOperation>().Any() && op.Type is INamedTypeSymbol namedSymbol)
            {
                AnalyzeConstructor_Impl(context, namedSymbol);
            }
        }


        private static void AnalyzeConstructor_Impl(OperationAnalysisContext context,
                                                    INamedTypeSymbol structSymbol
            )
        {
            var ctors = structSymbol.InstanceConstructors
                .Where(static x => x.Parameters.Length > 0)
                //.Where(static x => (x.DeclaredAccessibility & ~(Accessibility.Private | Accessibility.NotApplicable)) != 0)
                ;

            if (!ctors.Any())
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Rule_InvalidStructCtor, context.Operation.Syntax.GetLocation(), structSymbol.Name));
        }

    }
}
