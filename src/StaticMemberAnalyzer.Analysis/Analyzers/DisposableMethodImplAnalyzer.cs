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
        public const string RuleId = "SMA0043";

        private static readonly DiagnosticDescriptor Rule = new(
            RuleId,
            new LocalizableResourceString(nameof(Resources.SMA0043_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0043_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0043_Description), Resources.ResourceManager, typeof(Resources)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

            var disposedMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

            foreach (var member in namedType.GetMembers().OfType<IMethodSymbol>())
            {
                if (member.IsStatic) continue;

                foreach (var syntaxRef in member.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxRef.GetSyntax();
                    var model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                    var operation = model.GetOperation(syntax, context.CancellationToken);
                    if (operation == null) continue;

                    foreach (var op in operation.DescendantsAndSelf())
                    {
                        if (op is IInvocationOperation invocation && invocation.TargetMethod.Name == "Dispose" && invocation.TargetMethod.Parameters.Length == 0)
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

            foreach (var member in disposableMembers)
            {
                if (!disposedMembers.Contains(member))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0], member.Name));
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
            return namedType.GetMembers("Dispose").OfType<IMethodSymbol>().Any(m =>
                m.Parameters.Length == 0 &&
                m.ReturnType.SpecialType == SpecialType.System_Void &&
                m.DeclaredAccessibility == Accessibility.Public);
        }
    }
}
