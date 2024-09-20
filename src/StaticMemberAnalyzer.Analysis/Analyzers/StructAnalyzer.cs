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
            Core.Rule_DEBUG,
#endif
            Rule_InvalidStructCtor
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeStructConstructor, ImmutableArray.Create(
                OperationKind.ObjectCreation
                ));


            //context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        //private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
        //{
        //}


        /*  entry  ================================================================ */

        private static void AnalyzeStructConstructor(OperationAnalysisContext context)
        {
            INamedTypeSymbol? structSymbol = null;

            switch (context.Operation)
            {
                case IObjectCreationOperation createOp when createOp.Type.IsValueType:
                    {
                        if (createOp.Arguments.Length == 0)
                            structSymbol = createOp.Type as INamedTypeSymbol;
                    }
                    break;

                case IAnonymousObjectCreationOperation anonyCreateOp when anonyCreateOp.Type.IsValueType:
                    {
                        if (!anonyCreateOp.Syntax.DescendantNodes().OfType<ArgumentSyntax>().Any())
                            structSymbol = anonyCreateOp.Type as INamedTypeSymbol;
                    }
                    break;
            }


            if (structSymbol == null)
                return;

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
