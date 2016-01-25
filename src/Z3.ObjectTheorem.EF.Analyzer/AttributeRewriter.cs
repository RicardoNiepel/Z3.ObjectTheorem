using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.Analyzer
{
    public class AttributeAssumptionsRewriter : CSharpSyntaxRewriter
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IEnumerable<IClassAssumption> _classAssumptions;
        private readonly ObjectTheoremResult _objectTheoremResult;
        private readonly IEnumerable<IPropertyAssumption> _propertyAssumptions;
        private readonly SemanticModel _semanticModel;

        public AttributeAssumptionsRewriter(SemanticModel semanticModel,
            IEnumerable<IClassAssumption> classAssumptions, IEnumerable<IPropertyAssumption> propertyAssumptions,
            ObjectTheoremResult objectTheoremResult,
            CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
            _classAssumptions = classAssumptions;
            _propertyAssumptions = propertyAssumptions;
            _objectTheoremResult = objectTheoremResult;
            _cancellationToken = cancellationToken;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var namedTypeSymbol = _semanticModel.GetDeclaredSymbol(node, _cancellationToken);

            var classAssumptions = _classAssumptions.Where(ca => ca.Class == namedTypeSymbol).ToList();
            if (classAssumptions.Count == 0)
            {
                return base.VisitClassDeclaration(node);
            }

            var newClass = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            foreach (var classAssumption in classAssumptions)
            {
                foreach (string attributeToDelete in classAssumption.GetAttributesToDelete(_objectTheoremResult))
                {
                    newClass = RemoveAttribute(newClass, attributeToDelete);
                }

                foreach (string attributeToAdd in classAssumption.GetAttributesToAdd(_objectTheoremResult))
                {
                    newClass = EnsureAttribute(newClass, attributeToAdd);
                }
            }

            return newClass;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var propertySymbol = _semanticModel.GetDeclaredSymbol(node, _cancellationToken);

            var propertyAssumptions = _propertyAssumptions.Where(pa => pa.Property == propertySymbol).ToList();
            if (propertyAssumptions.Count == 0)
            {
                return base.VisitPropertyDeclaration(node);
            }

            var newProperty = node;

            foreach (var propertyAssumption in propertyAssumptions)
            {
                foreach (string attributeToDelete in propertyAssumption.GetAttributesToDelete(_objectTheoremResult))
                {
                    newProperty = RemoveAttribute(newProperty, attributeToDelete);
                }

                foreach (string attributeToAdd in propertyAssumption.GetAttributesToAdd(_objectTheoremResult))
                {
                    newProperty = EnsureAttribute(newProperty, attributeToAdd);
                }

                foreach (AttributeSyntax attributeToAdd in propertyAssumption.GetAttributeSyntaxexToAdd(_objectTheoremResult))
                {
                    newProperty = EnsureAttribute(newProperty, attributeToAdd);
                }
            }

            return base.VisitPropertyDeclaration(newProperty);
        }

        private static PropertyDeclarationSyntax AddAttribute(PropertyDeclarationSyntax property, string attributeName)
        {
            var newAttributes = SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                                    )
                                );
            property = property.AddAttributeLists(newAttributes);
            return property;
        }

        private static SimpleNameSyntax GetSimpleNameFromNode(AttributeSyntax node)
        {
            var identifierNameSyntax = node.Name as IdentifierNameSyntax;
            var qualifiedNameSyntax = node.Name as QualifiedNameSyntax;

            return
                identifierNameSyntax
                ??
                qualifiedNameSyntax?.Right
                ??
                (node.Name as AliasQualifiedNameSyntax).Name;
        }

        private ClassDeclarationSyntax AddAttribute(ClassDeclarationSyntax classDecl, string attributeName)
        {
            var newAttributes = SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                                    )
                                );
            classDecl = classDecl.AddAttributeLists(newAttributes);
            return classDecl;
        }

        private ClassDeclarationSyntax EnsureAttribute(ClassDeclarationSyntax classDecl, string attributeName)
        {
            if (!HasAttribute(classDecl.AttributeLists, attributeName))
            {
                return AddAttribute(classDecl, attributeName);
            }
            return classDecl;
        }

        private PropertyDeclarationSyntax EnsureAttribute(PropertyDeclarationSyntax property, AttributeSyntax attributeSyntax)
        {
            var attributeName = GetSimpleNameFromNode(attributeSyntax).Identifier.ValueText;
            if (HasAttribute(property.AttributeLists, attributeName))
            {
                property = RemoveAttribute(property, attributeName);
            }

            var newAttributes = SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(attributeSyntax)
                                );
            property = property.AddAttributeLists(newAttributes);
            return property;
        }

        private PropertyDeclarationSyntax EnsureAttribute(PropertyDeclarationSyntax property, string attributeName)
        {
            if (!HasAttribute(property.AttributeLists, attributeName))
            {
                return AddAttribute(property, attributeName);
            }
            return property;
        }

        private bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
        {
            return attributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => GetSimpleNameFromNode(a).Identifier.ValueText == attributeName);
        }

        private ClassDeclarationSyntax RemoveAttribute(ClassDeclarationSyntax node, string attributeName)
        {
            var changedAttributeLists = RemoveAttribute(attributeName, node.AttributeLists);
            var newClass = node.WithAttributeLists(changedAttributeLists);
            return newClass;
        }

        private PropertyDeclarationSyntax RemoveAttribute(PropertyDeclarationSyntax node, string attributeName)
        {
            var changedAttributeLists = RemoveAttribute(attributeName, node.AttributeLists);
            var newProperty = node.WithAttributeLists(changedAttributeLists);
            return newProperty;
        }

        private SyntaxList<AttributeListSyntax> RemoveAttribute(string attributeName, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var changedAttributeLists = new SyntaxList<AttributeListSyntax>();
            foreach (var attributeList in attributeLists)
            {
                var nodesToRemove =
                    attributeList
                    .Attributes
                    .Where(a => GetSimpleNameFromNode(a).Identifier.ValueText == attributeName)
                    .ToArray();

                // If the lists are the same length, we are removing all attributes and can just avoid populating newAttributes
                if (nodesToRemove.Length != attributeList.Attributes.Count)
                {
                    var newAttributeList = (AttributeListSyntax)VisitAttributeList(
                    attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));

                    changedAttributeLists = changedAttributeLists.Add(newAttributeList);
                }
            }

            return changedAttributeLists;
        }
    }
}