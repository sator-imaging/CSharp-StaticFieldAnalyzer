// Licensed under the MIT License
// https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposableMethodImplAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId_UndisposedMember = "SMA0043";
        public const string RuleId_MissingDisposeImpl = "SMA0044";
        private const string DisposeMethodName = "Dispose";

        private static readonly DiagnosticDescriptor Rule_UndisposedMember = new(
            RuleId_UndisposedMember,
            new LocalizableResourceString(nameof(Resources.SMA0043_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0043_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0043_Description), Resources.ResourceManager, typeof(Resources)));

        private static readonly DiagnosticDescriptor Rule_MissingDisposeImpl = new(
            RuleId_MissingDisposeImpl,
            new LocalizableResourceString(nameof(Resources.SMA0044_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0044_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0044_Description), Resources.ResourceManager, typeof(Resources)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Rule_UndisposedMember,
            Rule_MissingDisposeImpl
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            if (namedType.TypeKind != TypeKind.Class && namedType.TypeKind != TypeKind.Struct)
                return;

            var disposableMembers = new List<ISymbol>();
            foreach (var member in namedType.GetMembers())
            {
                if (member.IsStatic || member.IsImplicitlyDeclared) continue;

                if (member is IFieldSymbol field && IsDisposable(field.Type))
                {
                    disposableMembers.Add(field);
                }
                else if (member is IPropertySymbol property && IsDisposable(property.Type) && IsAutoProperty(property))
                {
                    disposableMembers.Add(property);
                }
            }

            if (disposableMembers.Count == 0)
                return;

            var dispose0Methods = new List<IMethodSymbol>();
            var dispose1BoolMethods = new List<IMethodSymbol>();
            var allDisposeMethods = new List<IMethodSymbol>();

            var iDisposable = context.Compilation.GetSpecialType(SpecialType.System_IDisposable);
            var iDisposableDispose = iDisposable?.GetMembers(DisposeMethodName).OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Length == 0);

            foreach (var member in namedType.GetMembers().OfType<IMethodSymbol>())
            {
                if (member.IsStatic) continue;

                bool isDispose = member.Name == DisposeMethodName;
                if (!isDispose && iDisposableDispose != null)
                {
                    // Check explicit implementation
                    if (member.ExplicitInterfaceImplementations.Any(e => SymbolEqualityComparer.Default.Equals(e, iDisposableDispose)))
                    {
                        isDispose = true;
                    }
                }

                if (isDispose)
                {
                    allDisposeMethods.Add(member);
                    if (member.Parameters.Length == 0 && (member.DeclaredAccessibility == Accessibility.Public || member.ExplicitInterfaceImplementations.Any()))
                    {
                        dispose0Methods.Add(member);
                    }
                    else if (member.Parameters.Length == 1 && member.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && member.DeclaredAccessibility == Accessibility.Protected)
                    {
                        dispose1BoolMethods.Add(member);
                    }
                }
            }

            var gcType = context.Compilation.GetTypeByMetadataName("System.GC");
            bool callsSuppressFinalize = false;
            foreach (var dispose0 in dispose0Methods)
            {
                foreach (var syntaxRef in dispose0.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxRef.GetSyntax();
                    var model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                    var operation = model.GetOperation(syntax, context.CancellationToken);
                    if (operation != null)
                    {
                        foreach (var op in operation.DescendantsAndSelf())
                        {
                            if (op is IInvocationOperation invocation &&
                                invocation.TargetMethod.Name == "SuppressFinalize" &&
                                SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, gcType))
                            {
                                callsSuppressFinalize = true;
                                break;
                            }
                        }
                    }
                    if (callsSuppressFinalize) break;
                }
                if (callsSuppressFinalize) break;
            }

            bool isFullDisposePattern = dispose0Methods.Count > 0 && dispose1BoolMethods.Count > 0 && callsSuppressFinalize;

            var targetMethods = isFullDisposePattern ? dispose1BoolMethods : allDisposeMethods;

            if (targetMethods.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule_MissingDisposeImpl, namedType.Locations[0], namedType.Name));
                return;
            }

            var disposedMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            foreach (var method in targetMethods)
            {
                foreach (var syntaxRef in method.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxRef.GetSyntax();
                    var model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                    var operation = model.GetOperation(syntax, context.CancellationToken);
                    if (operation == null) continue;

                    foreach (var op in operation.DescendantsAndSelf())
                    {
                        if (op is IInvocationOperation invocation && invocation.TargetMethod.Name == DisposeMethodName && invocation.TargetMethod.Parameters.Length == 0)
                        {
                            var receiver = invocation.Instance;
                            if (receiver is IConditionalAccessInstanceOperation)
                            {
                                var parent = invocation.Parent;
                                while (parent != null && parent is not IConditionalAccessOperation)
                                {
                                    parent = parent.Parent;
                                }
                                if (parent is IConditionalAccessOperation conditional)
                                {
                                    receiver = conditional.Operation;
                                }
                            }

                            var disposedMember = UnwrapMember(receiver);
                            if (disposedMember != null && SymbolEqualityComparer.Default.Equals(disposedMember.ContainingType, namedType))
                            {
                                disposedMembers.Add(disposedMember);
                            }
                        }
                    }
                }
            }

            var undisposedMembers = disposableMembers.Where(m => !disposedMembers.Contains(m)).ToList();
            if (undisposedMembers.Count == 0)
                return;

            foreach (var method in targetMethods)
            {
                foreach (var member in undisposedMembers)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule_UndisposedMember, method.Locations[0], member.Name));
                }
            }
        }

        private static bool IsAutoProperty(IPropertySymbol property)
        {
            foreach (var syntaxRef in property.DeclaringSyntaxReferences)
            {
                var syntax = syntaxRef.GetSyntax();
                if (syntax is PropertyDeclarationSyntax propSyntax)
                {
                    if (propSyntax.ExpressionBody != null) return false;
                    if (propSyntax.AccessorList == null) return false;
                    foreach (var accessor in propSyntax.AccessorList.Accessors)
                    {
                        if (accessor.Body != null || accessor.ExpressionBody != null)
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private static ISymbol? UnwrapMember(IOperation? op)
        {
            while (op != null)
            {
                if (op is IConversionOperation conversion)
                    op = conversion.Operand;
                else if (op is IParenthesizedOperation parenthesized)
                    op = parenthesized.Operand;
                else
                    break;
            }

            if (op is IFieldReferenceOperation fieldRef)
            {
                return fieldRef.Field;
            }
            if (op is IPropertyReferenceOperation propRef)
            {
                return propRef.Property;
            }

            return null;
        }

        private static bool IsDisposable(ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol namedType)
                return false;

            // Check IDisposable interface
            if (namedType.SpecialType == SpecialType.System_IDisposable ||
                namedType.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_IDisposable))
            {
                return true;
            }

            // Only check duck-typing for non-built-in types
            if (namedType.SpecialType != SpecialType.None)
                return false;

            // Check public void Dispose()
            return namedType.GetMembers(DisposeMethodName).OfType<IMethodSymbol>().Any(m =>
                m.Parameters.Length == 0 &&
                m.ReturnType.SpecialType == SpecialType.System_Void &&
                m.DeclaredAccessibility == Accessibility.Public);
        }
    }
}
