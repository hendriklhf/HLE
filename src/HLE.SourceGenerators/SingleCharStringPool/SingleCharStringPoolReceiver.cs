using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HLE.SourceGenerators.SingleCharStringPool;

public sealed class SingleCharStringPoolReceiver : ISyntaxReceiver
{
    public int AmountOfCachedSingleCharStrings { get; private set; } = -1;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not EqualsValueClauseSyntax
            {
                Parent: VariableDeclaratorSyntax
                {
                    Identifier.Text: "AmountOfCachedSingleCharStrings",
                    Parent.Parent.Parent: ClassDeclarationSyntax
                    {
                        Identifier.Text: "SingleCharStringPool",
                        Parent: FileScopedNamespaceDeclarationSyntax namespaceDeclaration
                    }
                }
            } equalsValue ||
            namespaceDeclaration.Name.ToString() != "HLE.Text")
        {
            return;
        }

        string fieldValue = equalsValue.Value.ToString();
        AmountOfCachedSingleCharStrings = int.Parse(fieldValue);
    }
}
