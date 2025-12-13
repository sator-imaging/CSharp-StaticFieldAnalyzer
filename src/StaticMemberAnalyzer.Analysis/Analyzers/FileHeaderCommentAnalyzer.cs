// Licensed under the MIT License
// https://github.com/sator-imaging/CSharp-StaticFieldAnalyzer

#define STMG_DEBUG_MESSAGE
#if DEBUG == false
#undef STMG_DEBUG_MESSAGE
#endif

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace SatorImaging.StaticMemberAnalyzer.Analysis.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileHeaderCommentAnalyzer : DiagnosticAnalyzer
    {
        #region     /* =      DESCRIPTOR      = */

        public const string RuleId_MissingFileHeaderComment = "SMA0050";
        private static readonly DiagnosticDescriptor Rule_MissingFileHeaderComment = new(
            RuleId_MissingFileHeaderComment,
            new LocalizableResourceString(nameof(Resources.SMA0050_Title), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.SMA0050_MessageFormat), Resources.ResourceManager, typeof(Resources)),
            Core.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.SMA0050_Description), Resources.ResourceManager, typeof(Resources)));

        #endregion


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
#if STMG_DEBUG_MESSAGE
            Core.Rule_DebugError,
            Core.Rule_DebugWarn,
#endif
            Rule_MissingFileHeaderComment
            );


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();


            //https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md

            context.RegisterSyntaxTreeAction(Analyze);
        }


        private static void Analyze(SyntaxTreeAnalysisContext context)
        {
            var text = context.Tree.GetText(context.CancellationToken);
            if (text.Length == 0)
                return;

            var position = SkipLeadingWhitespace(text);
            if (position >= text.Length)
                return;  // file contains only whitespace

            if (HasHeaderComment(text, position))
                return;

            var firstLine = text.Lines[0];
            var span = firstLine.Span;
            if (span.Length == 0 && firstLine.EndIncludingLineBreak > firstLine.Start)
            {
                // Cover the whole first line even if it's currently empty.
                span = TextSpan.FromBounds(firstLine.Start, firstLine.EndIncludingLineBreak);
            }

            var location = Location.Create(context.Tree, span);
            context.ReportDiagnostic(Diagnostic.Create(Rule_MissingFileHeaderComment, location));
        }


        /* =====  helper  ===== */

        private static int SkipLeadingWhitespace(SourceText text)
        {
            var position = 0;

            while (position < text.Length && char.IsWhiteSpace(text[position]))
            {
                position++;
            }

            return position;
        }

        private static bool HasHeaderComment(SourceText text, int position)
        {
            if (text[position] != '/')
            {
                return false;
            }

            var nextPosition = position + 1;
            if (nextPosition >= text.Length)
            {
                return false;
            }

            var nextChar = text[nextPosition];
            return nextChar is '/' or '*';
        }
    }
}
