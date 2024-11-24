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
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EnumAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      DESCRIPTOR      = */

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


        public const string RuleId_UnusualEnum = "SMA0027";
        private static readonly DiagnosticDescriptor Rule_UnusualEnum = new(
            RuleId_UnusualEnum,
            new LocalizableResourceString(nameof(Resources.SMA0027_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0027_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0027_Description), Resources.ResourceManager, typeof(Resources)));


        public const string RuleId_EnumLike = "SMA0028";
        private static readonly DiagnosticDescriptor Rule_EnumLike = new(
            RuleId_EnumLike,
            new LocalizableResourceString(nameof(Resources.SMA0028_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0028_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0028_Description), Resources.ResourceManager, typeof(Resources)));

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
            Rule_EnumObfuscation,
            Rule_UnusualEnum,
            Rule_EnumLike
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterOperationAction(AnalyzeEnumOperations, ImmutableArray.Create(
                OperationKind.Conversion,
                OperationKind.Invocation
                ));

            context.RegisterSyntaxNodeAction(AnalyzeEnumLikePattern, ImmutableArray.Create(
                SyntaxKind.ClassDeclaration
                ));

            context.RegisterSymbolAction(AnalyzeEnumDeclaration, ImmutableArray.Create(
                SymbolKind.NamedType
                ));


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


        [ThreadStatic] static List<IFieldSymbol>? ts_enumLikePatternFieldSymbolList;
        [ThreadStatic] static List<IFieldSymbol>? ts_enumLikePatternEntriesSymbolList;

        private static void AnalyzeEnumLikePattern(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax clsDeclStx)
                return;

            var model = context.SemanticModel;
            if (model.GetDeclaredSymbol(clsDeclStx) is not INamedTypeSymbol fieldContainerSymbol)
                return;

            if (fieldContainerSymbol.IsAbstract
             || fieldContainerSymbol.IsStatic
             || fieldContainerSymbol.IsImplicitlyDeclared
             )
            {
                return;
            }

            const string NAME_ENTRIES = "Entries";
            const string NAME_EQUALS = "Equals";
            const int LIST_CAPACITY = 8;

            var enumFieldList = (ts_enumLikePatternFieldSymbolList ??= new(capacity: LIST_CAPACITY));
            enumFieldList.Clear();

            var enumEntriesList = (ts_enumLikePatternEntriesSymbolList ??= new(capacity: LIST_CAPACITY));
            enumEntriesList.Clear();

            bool hasPublicEntries = false;
            foreach (var memberSymbol in fieldContainerSymbol.GetMembers())
            {
                string? symbolName = null;
                bool isPublic = (memberSymbol.DeclaredAccessibility & Accessibility.Public) == Accessibility.Public;

                // public bool Equals?
                // public static RETVAL Entries?
                if (isPublic)
                {
                    symbolName ??= memberSymbol.Name;

                    if (memberSymbol.IsStatic)
                    {
                        if (symbolName == NAME_ENTRIES)
                        {
                            hasPublicEntries = true;
                        }
                    }

                    // check only name and return type. IEquatable<T> won't require 'override' keyword
                    if (memberSymbol is IMethodSymbol methodSymbol
                     && methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean
                     && symbolName == NAME_EQUALS
                    )
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Rule_EnumLike, memberSymbol.Locations[0], NAME_EQUALS,
                            "equality comparer should not be overridden"));
                    }
                }


                /* =====  collect field analysis targets  ===== */

                if (memberSymbol is not IFieldSymbol fieldSymbol)
                    continue;

                // check static AND readonly modifiers only here!!
                if (!fieldSymbol.IsStatic || !fieldSymbol.IsReadOnly)
                    continue;

                if (fieldSymbol.IsImplicitlyDeclared)
                    continue;

                // is type enum member?
                if (SymbolEqualityComparer.Default.Equals(fieldSymbol.Type, fieldContainerSymbol))
                {
                    //public?
                    if (isPublic)
                    {
                        enumFieldList.Add(fieldSymbol);
                    }

                    continue;
                }


                //entries??
                symbolName ??= fieldSymbol.Name;

                if (!symbolName.StartsWith(NAME_ENTRIES, StringComparison.Ordinal/*IgnoreCase*/)
                 && !symbolName.EndsWith(NAME_ENTRIES, StringComparison.OrdinalIgnoreCase)
                )
                {
                    continue;
                }

                //type??
                if (fieldSymbol.Type is IArrayTypeSymbol arr && SymbolEqualityComparer.Default.Equals(arr.ElementType, fieldContainerSymbol))
                {
                    goto ENTRIES_FOUND;
                }

                if (IsReadOnlyMemory(fieldSymbol.Type, fieldContainerSymbol))
                {
                    goto ENTRIES_FOUND;
                }

                // not found
                continue;

            //entries!!
            ENTRIES_FOUND:
                enumEntriesList.Add(fieldSymbol);
            }


            /* =      class      = */

            // only when 'Entries' found
            if (hasPublicEntries || enumEntriesList.Count > 0)
            {
                const Accessibility ACCESS_HIDDEN = Accessibility.Protected | Accessibility.Private | Accessibility.NotApplicable;

                string? fieldContainerSymbolName = null;

                var ctors = fieldContainerSymbol.InstanceConstructors;
                if ((ctors.Length == 1 && ctors[0].IsImplicitlyDeclared)
                  || ctors.Any(static x => (x.DeclaredAccessibility & ~ACCESS_HIDDEN) != 0)
                  || ctors.Length == 0
                )
                {
                    fieldContainerSymbolName ??= fieldContainerSymbol.Name;

                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_EnumLike, clsDeclStx.Identifier.GetLocation(), fieldContainerSymbolName,
                        "constructor is not 'private' or 'protected'"));
                }

                if (!fieldContainerSymbol.IsSealed)
                {
                    fieldContainerSymbolName ??= fieldContainerSymbol.Name;

                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_EnumLike, clsDeclStx.Identifier.GetLocation(), fieldContainerSymbolName,
                        "type should be 'sealed'"));
                }

                // no public 'Entries'
                if (!hasPublicEntries)
                {
                    fieldContainerSymbolName ??= fieldContainerSymbol.Name;

                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_EnumLike, clsDeclStx.Identifier.GetLocation(), fieldContainerSymbolName,
                        "'public static' member called '" + NAME_ENTRIES + "' is not found"));
                }
            }


            /* =      entries fields      = */

            for (int i = 0; i < enumEntriesList.Count; i++)
            {
                AnalyzeEnumLikeEntriesField(context, enumEntriesList[i], enumFieldList);
            }
        }

        private static void AnalyzeEnumLikeEntriesField(SyntaxNodeAnalysisContext context,
                                                        IFieldSymbol entriesSymbol,
                                                        List<IFieldSymbol> enumFieldList
            )
        {
            var entriesContainerSymbol = entriesSymbol.ContainingType;
            string? entriesContainerSymbolName = null;

            /* =      Entries initializer      = */

            var initializerStx = entriesSymbol.DeclaringSyntaxReferences[0]?.GetSyntax()
                .DescendantNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

            if (initializerStx == null)
            {
                foreach (var stxRef in entriesSymbol.DeclaringSyntaxReferences)
                {
                    entriesContainerSymbolName ??= entriesContainerSymbol.Name;
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_EnumLike, stxRef.GetSyntax().GetLocation(), entriesContainerSymbolName,
                        "'Entries' doesn't have field initializer"));
                }

                return;
            }


            InitializerExpressionSyntax? initExprStx =
                initializerStx.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().FirstOrDefault()?.Initializer
             ?? initializerStx.DescendantNodes().OfType<ImplicitArrayCreationExpressionSyntax>().FirstOrDefault()?.Initializer
            ;

            if (initExprStx == null)
            {
                goto REPORT_INITIALIZER;
            }
            else
            {
                var initRefSyntaxes = initExprStx.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .ToImmutableArray();

                if (initRefSyntaxes.Length != enumFieldList.Count)
                {
                    goto REPORT_INITIALIZER;
                }
                else
                {
                    var model = context.Compilation.GetSemanticModel(initExprStx.SyntaxTree);

                    var initExprSymbols = initRefSyntaxes
                        .Select(x =>
                        {
                            if (model.GetOperation(x) is IFieldReferenceOperation fieldRefOp)
                            {
                                return fieldRefOp;
                            }
                            return null;
                        })
                        .Where(static x => x != null)
                        .ToImmutableArray();

                    for (int i = 0; i < enumFieldList.Count; i++)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(initExprSymbols[i]/*checked*/!.Member, enumFieldList[i]))
                        {
                            goto REPORT_INITIALIZER;
                        }
                    }

                    return;
                }
            }


        REPORT_INITIALIZER:
            entriesContainerSymbolName ??= entriesContainerSymbol.Name;
            context.ReportDiagnostic(Diagnostic.Create(
                Rule_EnumLike, initializerStx.GetLocation(), entriesContainerSymbolName,
                "'Entries' doesn't have all of 'public static readonly' field of type '" + entriesContainerSymbolName + "' in declared order"));
        }

        private static bool IsReadOnlyMemory(ITypeSymbol memoryCandidateType,
                                             ITypeSymbol elementType
            )
        {
            if (memoryCandidateType is not INamedTypeSymbol targetType)
                return false;

            if (targetType.TypeArguments.Length != 1)
                return false;

            if (!SymbolEqualityComparer.Default.Equals(targetType.TypeArguments[0], elementType))
                return false;

            //Core.ReportDebugMessage(reportAction,
            //    "TARGET TYPE NAME: ", targetType.ContainingNamespace.ContainingNamespace.IsGlobalNamespace.ToString(), syntax.GetLocation());

            if (targetType.Name != nameof(System.ReadOnlyMemory<int>))
                return false;

            var targetNS = targetType.ContainingNamespace;
            if (targetNS.Name != nameof(System) || !targetNS.ContainingNamespace.IsGlobalNamespace)
            {
                return false;
            }

            return true;
        }


        private static void AnalyzeEnumDeclaration(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } namedSymbol)
                return;

            var attrs = namedSymbol.GetAttributes();

            const string ATTR_OBFUSCATION_NAME_FULL = nameof(System) + "." + nameof(System.Reflection) + "." + nameof(System.Reflection.ObfuscationAttribute);
            const string ATTR_FLAGS_NAME_FULL = nameof(System) + "." + nameof(System.FlagsAttribute);

            bool hasObfuscationAttr = false;
            bool hasFlagsAttr = false;
            foreach (var attr in attrs)
            {
                var attrName = attr.AttributeClass.Name;
                switch (attrName)
                {
                    case nameof(System.Reflection.ObfuscationAttribute):
                        {
                            if (attr.AttributeClass.ToString() == ATTR_OBFUSCATION_NAME_FULL)
                            {
                                if (attr.NamedArguments.Any(static x => x.Key == nameof(ObfuscationAttribute.Exclude) && x.Value.Value is bool BOOL && BOOL)
                                 && attr.NamedArguments.Any(static x => x.Key == nameof(ObfuscationAttribute.ApplyToMembers) && x.Value.Value is bool BOOL && BOOL)
                                )
                                {
                                    hasObfuscationAttr = true;
                                }
                            }
                        }
                        break;

                    case nameof(System.FlagsAttribute):
                        {
                            if (attr.AttributeClass.ToString() == ATTR_FLAGS_NAME_FULL)
                            {
                                hasFlagsAttr = true;
                            }
                        }
                        break;
                }
            }

            if (!hasObfuscationAttr)
            {
                foreach (var loc in namedSymbol.Locations)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_EnumObfuscation, loc));
                }
            }

            if (!hasFlagsAttr)
            {
                AnalyzeUnusualEnum(context, namedSymbol);
            }
        }

        private static void AnalyzeUnusualEnum(SymbolAnalysisContext context,
                                               INamedTypeSymbol namedSymbol
            )
        {
            // partial enum is not allowed, take first one
            var declareStx = namedSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (declareStx == null)
                return;

            //underlyingType
            if (namedSymbol.EnumUnderlyingType.SpecialType != SpecialType.System_Int32)
            {
                var baseStx = declareStx.DescendantNodes().OfType<BaseTypeSyntax>().FirstOrDefault();
                if (baseStx != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule_UnusualEnum, baseStx.GetLocation()));
                }
            }

            //initializer
            foreach (var memberDeclare in declareStx.DescendantNodes().OfType<EnumMemberDeclarationSyntax>())
            {
                foreach (var childNode in memberDeclare.ChildNodes())
                {
                    //var grandNodes = childNode.ChildNodes();
                    //if (grandNodes.Any())
                    //{
                    //    foreach (var grand in grandNodes)
                    //    {
                    //        context.ReportDiagnostic(Diagnostic.Create(
                    //            Rule_UnusualEnum, grand.GetLocation()));
                    //    }
                    //}
                    //else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Rule_UnusualEnum, childNode.GetLocation()));
                    }
                }
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


        private static void AnalyzeCastFromEnum(SyntaxNode syntax,
                                                ITypeSymbol castToSymbol,
                                                SemanticModel model,
                                                Action<Diagnostic> reportAction
            )
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

            if (IsEnumDerivedType(castFromSymbol))
            {
                reportAction.Invoke(Diagnostic.Create(
                    Rule_CastFromEnum, syntax.GetLocation(), castFromSymbol.Name));
            }

            //Core.ReportDebugMessage(reportAction,
            //    nameof(Rule_CastFromEnum) + ": " + syntax.Kind(), syntax.ToString(), syntax.GetLocation());
        }

    }
}
