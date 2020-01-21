using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DisableDateTimeNow_Updated
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisableDateTimeNow_UpdatedCodeFixProvider)), Shared]
    public class DisableDateTimeNow_UpdatedCodeFixProvider : CodeFixProvider
    {
        private const string title = "Call DateTime.UtcNow rather than DateTime.Now";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DisableDateTimeNow_UpdatedAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            return Task.Run(() => context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithUtcNowAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: title),
                diagnostic));
        }

        private async Task<Document> ReplaceWithUtcNowAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync();
            var repl = "DateTime.UtcNow";
            if (Regex.Replace(text.GetSubText(span).ToString(), @"\s+", string.Empty) == "System.DateTime.Now")
                repl = "System.DateTime.UtcNow";
            var newtext = text.Replace(span, repl);
            return document.WithText(newtext);
        }
    }
}
