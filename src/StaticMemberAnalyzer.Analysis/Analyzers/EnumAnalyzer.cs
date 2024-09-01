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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EnumAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      STATIC MEMBER DESCRIPTOR      = */

        public const string RuleId_CastToEnum = "SMA0020";
        private static readonly DiagnosticDescriptor Rule_CastToEnum = new(
            RuleId_CastToEnum,
            new LocalizableResourceString(nameof(Resources.SMA0020_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0020_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0020_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_CastFromEnum = "SMA0021";
        private static readonly DiagnosticDescriptor Rule_CastFromEnum = new(
            RuleId_CastFromEnum,
            new LocalizableResourceString(nameof(Resources.SMA0021_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0021_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0021_Description), Resources.ResourceManager, typeof(Resources)));


        public const string RuleId_CastToGenericEnum = "SMA0022";
        private static readonly DiagnosticDescriptor Rule_CastToGenericEnum = new(
            RuleId_CastToGenericEnum,
            new LocalizableResourceString(nameof(Resources.SMA0022_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0022_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0022_Description), Resources.ResourceManager, typeof(Resources)));

        public const string RuleId_CastFromGenericEnum = "SMA0023";
        private static readonly DiagnosticDescriptor Rule_CastFromGenericEnum = new(
            RuleId_CastFromGenericEnum,
            new LocalizableResourceString(nameof(Resources.SMA0023_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0023_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0023_Description), Resources.ResourceManager, typeof(Resources)));


        public const string RuleId_EnumToString = "SMA0024";
        private static readonly DiagnosticDescriptor Rule_EnumToString = new(
            RuleId_EnumToString,
            new LocalizableResourceString(nameof(Resources.SMA0024_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0024_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0024_Description), Resources.ResourceManager, typeof(Resources)));


        public const string RuleId_EnumMethod = "SMA0025";
        private static readonly DiagnosticDescriptor Rule_EnumMethod = new(
            RuleId_EnumMethod,
            new LocalizableResourceString(nameof(Resources.SMA0025_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0025_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0025_Description), Resources.ResourceManager, typeof(Resources)));


        public const string RuleId_EnumObfuscation = "SMA0026";
        private static readonly DiagnosticDescriptor Rule_EnumObfuscation = new(
            RuleId_EnumObfuscation,
            new LocalizableResourceString(nameof(Resources.SMA0026_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0026_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0026_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DEBUG,
#endif
            Rule_CastToEnum,
            Rule_CastFromEnum,
            Rule_CastToGenericEnum,
            Rule_CastFromGenericEnum,
            Rule_EnumToString,
            Rule_EnumMethod,
            Rule_EnumObfuscation
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeEnumOperations,
                ImmutableArray.Create(OperationKind.Conversion, OperationKind.Invocation));

            //context.RegisterSyntaxNodeAction(AnalyzeExplicitCast,
            //    ImmutableArray.Create(SyntaxKind.CastExpression));

            context.RegisterSymbolAction(AnalyzeEnumDeclaration,
                ImmutableArray.Create(SymbolKind.NamedType));


            //context.RegisterCompilationStartAction(InitializeAndRegisterCallbacks);
        }


        //private static void InitializeAndRegisterCallbacks(CompilationStartAnalysisContext context)
        //{
        //}


        /*  entry  ================================================================ */

        private static void AnalyzeEnumOperations(OperationAnalysisContext context)
        {
            switch (context.Operation)
            {
                case IConversionOperation castOp:// when castOp.IsImplicit:
                    {
                        // generic type parameter??
                        if (castOp.Type is ITypeParameterSymbol typeParamSymbol)
                        {
                            if (HasEnumConstraint(typeParamSymbol))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(
                                    Rule_CastToGenericEnum, castOp.Syntax.GetLocation(), typeParamSymbol.Name));
                            }
                            return;
                        }

                        var castToEnum = IsEnumDerivedType(castOp.Type);
                        if (castToEnum)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Rule_CastToEnum, castOp.Syntax.GetLocation(), castOp.Type.Name));
                        }
                        else
                        {
                            AnalyzeCastFromEnum(castOp.Syntax, castOp.Type, context.Compilation.GetSemanticModel(castOp.Syntax.SyntaxTree), context.ReportDiagnostic);
                        }
                    }
                    break;

                case IInvocationOperation methodOp:
                    {
                        // derived type methods!
                        if (methodOp.Instance != null)
                        {
                            if (methodOp.Instance.Type is ITypeParameterSymbol typeParamSymbol)
                            {
                                if (!HasEnumConstraint(typeParamSymbol))
                                    return;
                            }
                            else if (!IsEnumDerivedType(methodOp.Instance.Type))
                            {
                                return;
                            }

                            //string??
                            if (methodOp.Type.SpecialType == SpecialType.System_String)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(
                                    Rule_EnumToString, methodOp.Syntax.GetLocation(), methodOp.Instance.Type.Name));
                            }

                            return;
                        }

                        // Enum methods!!
                        else if (methodOp.TargetMethod.ReceiverType.SpecialType == SpecialType.System_Enum)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Rule_EnumMethod, methodOp.Syntax.GetLocation()));

                            return;
                        }
                    }
                    break;
            }
        }


        /*
        private static void AnalyzeExplicitCast(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not CastExpressionSyntax castStx)
                return;

            var model = context.SemanticModel;
            if (model.GetTypeInfo(castStx.Type).ConvertedType is not ITypeSymbol castToSymbol)
                return;

            // generic type parameter??
            if (castToSymbol is ITypeParameterSymbol typeParamSymbol)
            {
                if (HasEnumConstraint(typeParamSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_CastToGenericEnum, castStx.GetLocation(), typeParamSymbol.Name));
                }
                return;
            }

            var castToEnum = IsEnumDerivedType(castToSymbol);
            if (castToEnum)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule_CastToEnum, castStx.GetLocation(), castStx.Type.ToString()));
            }
            else
            {
                AnalyzeCastFromEnum(castStx, castToSymbol, model, context.ReportDiagnostic);
            }
        }
        */


        private static void AnalyzeEnumDeclaration(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } namedSymbol)
                return;

            const string attrFullName = nameof(System) + "." + nameof(System.Reflection) + "." + nameof(ObfuscationAttribute);
            foreach (var attr in namedSymbol.GetAttributes())
            {
                if (attr.AttributeClass.Name == nameof(System.Reflection.ObfuscationAttribute)
                 && attr.AttributeClass.ToString() == attrFullName
                )
                {
                    if (attr.NamedArguments.Any(static x => x.Key == nameof(ObfuscationAttribute.Exclude) && x.Value.Value is bool BOOL && BOOL)
                     && attr.NamedArguments.Any(static x => x.Key == nameof(ObfuscationAttribute.ApplyToMembers) && x.Value.Value is bool BOOL && BOOL)
                    )
                    {
                        return;
                    }
                }
            }

            foreach (var loc in namedSymbol.Locations)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule_EnumObfuscation, loc));
            }
        }


        /*  helper  ================================================================ */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEnumDerivedType(ITypeSymbol symbol)
        {
            return symbol.TypeKind == TypeKind.Enum && symbol.SpecialType != SpecialType.System_Enum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasEnumConstraint(ITypeParameterSymbol symbol)
        {
            return symbol.ConstraintTypes.Any(static x => x.SpecialType == SpecialType.System_Enum);
        }


        private static void AnalyzeCastFromEnum(SyntaxNode syntax, ITypeSymbol castToSymbol, SemanticModel model, Action<Diagnostic> reportAction)
        {
            ITypeSymbol? castFromSymbol = null;

            castFromSymbol = syntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                .Select(x => model.GetTypeInfo(x.Name).ConvertedType)
                .FirstOrDefault(static x => x != null);

            castFromSymbol ??= syntax.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Select(x => model.GetTypeInfo(x).ConvertedType)
                .FirstOrDefault(static x => x != null);

            if (castFromSymbol == null)
            {
                //Core.ReportDebugMessage(reportAction, "Type Not Found", null, syntax.GetLocation());
                return;
            }

            // generic type parameter??
            if (castFromSymbol is ITypeParameterSymbol typeParamSymbol)
            {
                if (HasEnumConstraint(typeParamSymbol))
                {
                    reportAction.Invoke(Diagnostic.Create(
                        Rule_CastFromGenericEnum, syntax.GetLocation(), typeParamSymbol.Name));
                }
                return;
            }

            //if (!IsEnumDerivedType(castFromSymbol))
            //{
            //    Core.ReportDebugMessage(reportAction, "[CastFromEnum] Non Enum", castFromSymbol?.ToString(), syntax.GetLocation());
            //    return;
            //}

            //// correct underlying type?
            //if (castToSymbol.IsValueType
            // && castFromSymbol is INamedTypeSymbol named && castToSymbol.SpecialType != named.EnumUnderlyingType.SpecialType)
            //{
            //    Core.ReportDebugMessage(reportAction, "Underlying Type Mismatch", null, syntax.GetLocation());
            //}
            //else
            {
                reportAction.Invoke(Diagnostic.Create(
                    Rule_CastFromEnum, syntax.GetLocation(), castFromSymbol.Name));
            }
        }

    }
}
