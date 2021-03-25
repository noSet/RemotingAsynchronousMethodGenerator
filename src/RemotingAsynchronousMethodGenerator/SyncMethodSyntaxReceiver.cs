using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RemotingAsynchronousMethodGenerator
{
    internal class SyncMethodSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<IMethodSymbol> Methods { get; } = new List<IMethodSymbol>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                foreach (var attributeList in typeDeclarationSyntax.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (attribute.Name.ToString() == "GenerateAsynchronousMethodsAttribute" || attribute.Name.ToString() == "GenerateAsynchronousMethods")
                        {
                            foreach (var methodDeclarationSyntax in typeDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>())
                            {
                                IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;

                                if (methodSymbol.Parameters.Length <= 16 && methodSymbol.Parameters.All(p => p.RefKind == RefKind.None))
                                {
                                    this.Methods.Add(methodSymbol);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
