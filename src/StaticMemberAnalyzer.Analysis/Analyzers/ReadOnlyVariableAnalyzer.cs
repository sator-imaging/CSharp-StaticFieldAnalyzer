// Licensed under the MIT License
// https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer

#define STMG_DEBUG_MESSAGE
#if DEBUG == false
#undef STMG_DEBUG_MESSAGE
#endif

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReadOnlyLocalsAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId_ReadOnlyLocal = "SMA0060";
        public const string RuleId_ReadOnlyParameter = "SMA0061";

        private static readonly DiagnosticDescriptor Rule_ReadOnlyLocal = new(
            RuleId_ReadOnlyLocal,
            new LocalizableResourceString("SMA0060_Title", Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString("SMA0060_MessageFormat", Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString("SMA0060_Description", Resources.ResourceManager, typeof(Resources)));

        private static readonly DiagnosticDescriptor Rule_ReadOnlyParameter = new(
            RuleId_ReadOnlyParameter,
            new LocalizableResourceString("SMA0061_Title", Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString("SMA0061_MessageFormat", Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString("SMA0061_Description", Resources.ResourceManager, typeof(Resources)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DebugError,
            Core.Rule_DebugWarn,
#endif
            Rule_ReadOnlyLocal,
            Rule_ReadOnlyParameter
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeSimpleAssignment, OperationKind.SimpleAssignment);
            context.RegisterOperationAction(AnalyzeCoalesceAssignment, OperationKind.CoalesceAssignment);
            context.RegisterOperationAction(AnalyzeCompoundAssignment, OperationKind.CompoundAssignment);
            context.RegisterOperationAction(AnalyzeIncrementOrDecrement, OperationKind.Increment, OperationKind.Decrement);
            context.RegisterOperationAction(AnalyzeDeconstructionAssignment, OperationKind.DeconstructionAssignment);
        }

        private static void AnalyzeSimpleAssignment(OperationAnalysisContext context)
        {
            if (context.Operation is not ISimpleAssignmentOperation op)
            {
                return;
            }

            ReportIfDisallowedLocal(context, op.Target);
        }

        private static void AnalyzeCompoundAssignment(OperationAnalysisContext context)
        {
            if (context.Operation is not ICompoundAssignmentOperation op)
            {
                return;
            }

            ReportIfDisallowedLocal(context, op.Target);
        }

        private static void AnalyzeCoalesceAssignment(OperationAnalysisContext context)
        {
            if (context.Operation is not ICoalesceAssignmentOperation op)
            {
                return;
            }

            ReportIfDisallowedLocal(context, op.Target);
        }

        private static void AnalyzeIncrementOrDecrement(OperationAnalysisContext context)
        {
            if (context.Operation is not IIncrementOrDecrementOperation op)
            {
                return;
            }

            ReportIfDisallowedLocal(context, op.Target);
        }

        private static void AnalyzeDeconstructionAssignment(OperationAnalysisContext context)
        {
            if (context.Operation is not IDeconstructionAssignmentOperation op)
            {
                return;
            }

            ReportIfDisallowedLocal(context, op.Target);
        }

        private static void ReportIfDisallowedLocal(OperationAnalysisContext context, IOperation target)
        {
            var reported = new HashSet<string>();
            foreach (var (name, isParameter, location, syntax) in EnumerateAssignedLocalsAndParameters(target))
            {
                if (IsAllowed(name))
                {
                    continue;
                }

                if (IsAllowedForStatementHeaderAssignment(syntax))
                {
                    continue;
                }

                var key = name + "@" + location.SourceSpan.Start;
                if (!reported.Add(key))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(GetDescriptor(isParameter), location, name));
            }
        }

        private static IEnumerable<(string name, bool isParameter, Location location, SyntaxNode syntax)> EnumerateAssignedLocalsAndParameters(IOperation op)
        {
            if (op is ILocalReferenceOperation localReference)
            {
                yield return (localReference.Local.Name, false, op.Syntax.GetLocation(), op.Syntax);
            }
            else if (op is IParameterReferenceOperation parameterReference)
            {
                yield return (parameterReference.Parameter.Name, true, op.Syntax.GetLocation(), op.Syntax);
            }
            else if (op is IPropertyReferenceOperation or IFieldReferenceOperation)
            {
                if (TryGetRootLocalOrParameter(op, out var name, out var isParameter))
                {
                    yield return (name, isParameter, op.Syntax.GetLocation(), op.Syntax);
                }
            }
            else if (op is ITupleOperation tupleOperation)
            {
                foreach (var element in tupleOperation.Elements)
                {
                    foreach (var nested in EnumerateAssignedLocalsAndParameters(element))
                    {
                        yield return nested;
                    }
                }
            }
            else if (op is IVariableDeclaratorOperation variableDeclarator && variableDeclarator.Symbol is ILocalSymbol localSymbol)
            {
                yield return (localSymbol.Name, false, op.Syntax.GetLocation(), op.Syntax);
            }
            else if (op is IDeclarationExpressionOperation declarationExpression)
            {
                foreach (var nested in EnumerateAssignedLocalsAndParameters(declarationExpression.Expression))
                {
                    yield return nested;
                }
            }
        }

        private static bool IsAllowed(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.StartsWith("mut_");
        }

        private static bool IsAllowedForStatementHeaderAssignment(SyntaxNode syntax)
        {
            var forSyntax = syntax.FirstAncestorOrSelf<ForStatementSyntax>();
            if (forSyntax == null)
            {
                return false;
            }

            if (forSyntax.Declaration != null && forSyntax.Declaration.Span.Contains(syntax.Span))
            {
                return true;
            }

            if (forSyntax.Condition != null && forSyntax.Condition.Span.Contains(syntax.Span))
            {
                return true;
            }

            foreach (var initializer in forSyntax.Initializers)
            {
                if (initializer.Span.Contains(syntax.Span))
                {
                    return true;
                }
            }

            foreach (var incrementor in forSyntax.Incrementors)
            {
                if (incrementor.Span.Contains(syntax.Span))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetRootLocalOrParameter(IOperation operation, out string name, out bool isParameter)
        {
            var current = operation;
            while (current != null)
            {
                if (current is IConversionOperation conversion)
                {
                    current = conversion.Operand;
                    continue;
                }

                if (current is ILocalReferenceOperation localReference)
                {
                    name = localReference.Local.Name;
                    isParameter = false;
                    return true;
                }

                if (current is IParameterReferenceOperation parameterReference)
                {
                    name = parameterReference.Parameter.Name;
                    isParameter = true;
                    return true;
                }

                if (current is IPropertyReferenceOperation propertyReference)
                {
                    current = propertyReference.Instance;
                    continue;
                }

                if (current is IFieldReferenceOperation fieldReference)
                {
                    current = fieldReference.Instance;
                    continue;
                }

                if (current is IArrayElementReferenceOperation arrayElementReference)
                {
                    current = arrayElementReference.ArrayReference;
                    continue;
                }

                break;
            }

            name = string.Empty;
            isParameter = false;
            return false;
        }

        private static DiagnosticDescriptor GetDescriptor(bool isParameter)
        {
            return isParameter ? Rule_ReadOnlyParameter : Rule_ReadOnlyLocal;
        }
    }
}
