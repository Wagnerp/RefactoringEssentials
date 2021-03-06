using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Add a Contract to specify the paramter must not be null")]
    /// <summary>
    /// Creates a 'Contract.Requires(param != null);' contruct for a parameter.
    /// </summary>
    public class ContractRequiresNotNullCodeRefactoringProvider : SpecializedCodeRefactoringProvider<ParameterSyntax>
    {
        protected override IEnumerable<CodeAction> GetActions(Document document, SemanticModel semanticModel, SyntaxNode root, TextSpan span, ParameterSyntax node, CancellationToken cancellationToken)
        {
            if (!node.Identifier.Span.Contains(span))
                return Enumerable.Empty<CodeAction>();
            var parameter = node;
            var bodyStatement = parameter.Parent.Parent.ChildNodes().OfType<BlockSyntax>().FirstOrDefault();
            if (bodyStatement == null)
                return Enumerable.Empty<CodeAction>();

            var parameterSymbol = semanticModel.GetDeclaredSymbol(node);
            var type = parameterSymbol.Type;
            if (type == null || type.IsValueType || HasNotNullContract(semanticModel, parameterSymbol, bodyStatement))
                return Enumerable.Empty<CodeAction>();
            return new[] { CodeActionFactory.Create(
                node.Identifier.Span,
                DiagnosticSeverity.Info,
                GettextCatalog.GetString ("Add contract requires parameter must not be null"),
                t2 => {
                    var newBody = bodyStatement.WithStatements (SyntaxFactory.List<StatementSyntax>(new [] { CreateContractRequiresCall(node.Identifier.ToString()) }.Concat (bodyStatement.Statements)));

                    var newRoot = (CompilationUnitSyntax) root.ReplaceNode((SyntaxNode)bodyStatement, newBody);

                    if (UsingStatementNotPresent(newRoot)) newRoot = AddUsingStatement(node, newRoot);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            ) };
        }

        static ExpressionStatementSyntax CreateContractRequiresCall(string paramName)
        {
            var contract = SyntaxFactory.IdentifierName("Contract");
            var requires = SyntaxFactory.IdentifierName("Requires");
            var memberaccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, contract, requires);

            var argument = SyntaxFactory.Argument(SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, SyntaxFactory.IdentifierName(paramName), SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));
            var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

            var contractRequiresCall =
                SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(memberaccess,
                SyntaxFactory.ArgumentList(argumentList)));

            return contractRequiresCall.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        static bool UsingStatementNotPresent(CompilationUnitSyntax cu)
        {
            return !cu.Usings.Any((UsingDirectiveSyntax u) => u.Name.ToString() == "System.Diagnostics.Contracts");
        }

        static CompilationUnitSyntax AddUsingStatement(ParameterSyntax node, CompilationUnitSyntax cu)
        {
            return cu.AddUsingDirective(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.Contracts")).WithAdditionalAnnotations(Formatter.Annotation)
                , node
                , true);
        }

        static bool HasNotNullContract(SemanticModel semanticModel, IParameterSymbol parameterSymbol, BlockSyntax bodyStatement)
        {
            foreach (var expressions in bodyStatement.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                var identifiers = expressions.DescendantNodes().OfType<IdentifierNameSyntax>();

                if (Enumerable.SequenceEqual(identifiers.Select(i => i.Identifier.Text), new List<string>() { "Contract", "Requires", parameterSymbol.Name }))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
