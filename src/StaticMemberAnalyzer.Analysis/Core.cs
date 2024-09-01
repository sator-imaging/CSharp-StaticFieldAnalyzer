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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SatorImaging.StaticMemberAnalyzer.Analysis
{
    //https://github.com/dotnet/roslyn/blob/main/docs/wiki/Roslyn-Overview.md#solutions-projects-documents
    public sealed class Core
    {
        //https://github.com/dotnet/roslyn-analyzers/blob/main/src/Utilities/Compiler/DiagnosticCategoryAndIdRanges.txt
        internal const string Category = "Usage";


        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

        // NOTE: define in any case to avoid error.
        //       but this is not registered to analyzer when debug message flag is not set.
        public const string RuleId_DEBUG = "SMAxDEBUG";  // no hyphens!
        [DescriptionAttribute("use instead --> " + nameof(ReportDebugMessage))]
        internal static readonly DiagnosticDescriptor Rule_DEBUG = new(
            RuleId_DEBUG,
            "SMAxDEBUG",
            "{0}",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "SMAxDEBUG");


        /*  DEBUG  ================================================================ */

        [Conditional("STMG_DEBUG_MESSAGE")]
        internal static void ReportDebugMessage(Action<Diagnostic> reportMethod, string title, string? message, Location location)
        {
            ReportDebugMessage(reportMethod, title, message, new Location[] { location });
        }

        [Conditional("STMG_DEBUG_MESSAGE")]
        internal static void ReportDebugMessage<T>(Action<Diagnostic> reportMethod, string title, string? message, T locations)
            where T : IEnumerable<Location>
        {
            if (locations == null)
                return;

            message = message != null ? title + "\n" + message : title;
            foreach (var loc in locations)
            {
                reportMethod(Diagnostic.Create(Rule_DEBUG, loc, message));
            }
        }


        /*  string op  ================================================================ */

        [ThreadStatic, DescriptionAttribute] static StringBuilder? ts_sb;

        internal static string GetMemberNamePrefix(SyntaxNode? node)
        {
            var sb = (ts_sb ??= new());
            sb.Length = 0;  // don't clear! it will set capacity = 0 and allocated memory is gone!!

            var parent = node?.Parent;
            while (parent != null)
            {
                switch (parent)
                {
                    case TypeDeclarationSyntax type:
                        sb.Insert(0, type.Identifier.Text);
                        break;
                    case NamespaceDeclarationSyntax ns:
                        sb.Insert(0, ns.Name.ToString());
                        break;
                }
                parent = parent.Parent;
            }

            return sb.ToString();
        }


        // string.Create and Concat(ReadOnlySpan) cannot be used in .net standard 2.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string SpanConcat(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
        {
            Span<char> buffer = stackalloc char[left.Length + right.Length];
            left.CopyTo(buffer);
            right.CopyTo(buffer.Slice(left.Length));

            return buffer.ToString();
        }

    }
}
