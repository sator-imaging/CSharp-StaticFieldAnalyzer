﻿/*  Core  ================================================================ */
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
using System.Collections.Immutable;
using System.Linq;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TSelfTypeParameterAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      DESCRIPTOR      = */

        public const string RuleId_TSelfInvariant = "SMA0010";
        private static readonly DiagnosticDescriptor Rule_TSelfInvariant = new(
            RuleId_TSelfInvariant,
            new LocalizableResourceString(nameof(Resources.SMA0010_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0010_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0010_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_TSelfCovariant = "SMA0011";
        private static readonly DiagnosticDescriptor Rule_TSelfCovariant = new(
            RuleId_TSelfCovariant,
            new LocalizableResourceString(nameof(Resources.SMA0011_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0011_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0011_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_TSelfContravariant = "SMA0012";
        private static readonly DiagnosticDescriptor Rule_TSelfContravariant = new(
            RuleId_TSelfContravariant,
            new LocalizableResourceString(nameof(Resources.SMA0012_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0012_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0012_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DEBUG,
#endif
            Rule_TSelfInvariant,
            Rule_TSelfCovariant,
            Rule_TSelfContravariant
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterSyntaxNodeAction(AnalyzeTSelf, ImmutableArray.Create(
                SyntaxKind.ClassDeclaration,
                // TODO --> SyntaxKind.RecordDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration
                ));


            //context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        /*  impl  ================================================================ */

        private void AnalyzeTSelf(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not TypeDeclarationSyntax targetTypeDeclare)
                return;

            var baseTypeList = context.Node.DescendantNodes().OfType<BaseListSyntax>().FirstOrDefault();
            if (baseTypeList == null)
                return;

            ITypeSymbol? targetTypeSymbol = null;
            ITypeSymbol? targetBaseSymbol = null;
            var compilation = context.Compilation;
            SemanticModel? baseTypeModel;
            bool isCovariant = false;
            bool isContravariant = false;
            foreach (var baseType in baseTypeList.Types)
            {
                var genName = baseType.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
                if (genName == null)
                    continue;

                var typeArgList = genName.TypeArgumentList;
                if (typeArgList == null)
                    continue;

                // find base declaration
                baseTypeModel = compilation.GetSemanticModel(baseType.Type.SyntaxTree);
                var baseSymbol = baseTypeModel.GetTypeInfo(baseType.Type).ConvertedType;
                if (baseSymbol == null)
                {
                    Core.ReportDebugMessage(context.ReportDiagnostic,
                        "cannot get baseType symbol", null, targetTypeDeclare.GetLocation());

                    continue;
                }

                int TSelfTypeArgPosition = -1;
                foreach (var baseDeclareRef in baseSymbol.DeclaringSyntaxReferences)
                {
                    if (baseDeclareRef.GetSyntax() is not TypeDeclarationSyntax baseDeclare)
                        continue;

                    var baseTypeParamList = baseDeclare.DescendantNodes().OfType<TypeParameterListSyntax>().FirstOrDefault();
                    if (baseTypeParamList == null)
                        continue;

                    for (int i = 0; i < baseTypeParamList.Parameters.Count; i++)
                    {
                        var param = baseTypeParamList.Parameters[i];
                        if (param.Identifier.Text == "TSelf")
                        {
                            TSelfTypeArgPosition = i;

                            isCovariant = param.VarianceKeyword.IsKind(SyntaxKind.OutKeyword);
                            isContravariant = param.VarianceKeyword.IsKind(SyntaxKind.InKeyword);

                            goto EXIT_FOREACH;
                        }
                    }
                }

            EXIT_FOREACH:
                ;

                if (TSelfTypeArgPosition < 0)
                    continue;

                var foundTypeArgNode = typeArgList.DescendantNodes().ElementAtOrDefault(TSelfTypeArgPosition);
                if (foundTypeArgNode == null)
                    continue;


                var foundTypeArgSymbol = baseTypeModel.GetTypeInfo(foundTypeArgNode).ConvertedType;
                if (foundTypeArgSymbol == null)
                {
                    Core.ReportDebugMessage(context.ReportDiagnostic, "[NOT FOUND] TypeArg Symbol", null, foundTypeArgNode.GetLocation());
                    continue;
                }

                if (targetTypeSymbol == null)
                {
                    var model = compilation.GetSemanticModel(targetTypeDeclare.SyntaxTree);
                    targetTypeSymbol = model.GetDeclaredSymbol(targetTypeDeclare);

                    if (targetTypeSymbol == null)
                    {
                        Core.ReportDebugMessage(context.ReportDiagnostic, "[NOT FOUND] Target Symbol", null, targetTypeDeclare.Identifier.GetLocation());
                        continue;
                    }
                }

                //go!!
                if (!SymbolEqualityComparer.Default.Equals(foundTypeArgSymbol, targetTypeSymbol))
                {
                    if (isCovariant)
                    {
                        bool isOmittableBaseType = foundTypeArgSymbol.SpecialType == SpecialType.System_Object;
                        if (!isOmittableBaseType)
                        {
                            if (targetBaseSymbol == null)
                            {
                                var targetBaseType = baseTypeList.Types.FirstOrDefault()?.Type;
                                if (targetBaseType != null)
                                {
                                    targetBaseSymbol = baseTypeModel.GetTypeInfo(targetBaseType).ConvertedType;
                                }
                            }

                            var candidateSymbol = targetBaseSymbol;
                            while (candidateSymbol != null)
                            {
                                if (SymbolEqualityComparer.Default.Equals(candidateSymbol, foundTypeArgSymbol))
                                {
                                    isOmittableBaseType = true;
                                    break;
                                }

                                candidateSymbol = candidateSymbol.BaseType;
                            }
                        }

                        if (!isOmittableBaseType)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Rule_TSelfCovariant, foundTypeArgNode.GetLocation(), targetTypeSymbol.ToString()));
                        }
                    }

                    //contravariant!!
                    else if (isContravariant)
                    {
                        bool isFound = false;

                        var contraSymbol = compilation.GetSemanticModel(foundTypeArgNode.SyntaxTree).GetTypeInfo(foundTypeArgNode).ConvertedType;
                        while (contraSymbol != null)
                        {
                            if (SymbolEqualityComparer.Default.Equals(contraSymbol, targetTypeSymbol))
                            {
                                isFound = true;
                                break;
                            }

                            contraSymbol = contraSymbol.BaseType;
                        }

                        if (!isFound)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Rule_TSelfContravariant, foundTypeArgNode.GetLocation(), targetTypeSymbol.ToString()));
                        }
                    }

                    //invariant!!
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Rule_TSelfInvariant, foundTypeArgNode.GetLocation(), targetTypeSymbol.ToString()));
                    }
                }
            }
        }

    }
}
