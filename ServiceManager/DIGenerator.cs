using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ServiceManager
{
    [Generator]
    public class DIGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not MySyntaxReceiver receiver)
                return;

            var registrations = new List<string>();

            foreach (var classDecl in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol == null) continue;

                foreach (var attr in symbol.GetAttributes())
                {
                    var name = attr.AttributeClass?.Name;
                    string lifetime = name?.Replace("Attribute", "");

                    if (lifetime is "Scoped" or "Singleton" or "Transient")
                    {
                        var service = symbol.Interfaces.Where(i => i.ToDisplayString() != "System.IDisposable")
                            .FirstOrDefault()?.ToDisplayString();

                        var impl = symbol.ToDisplayString();

                        if (service != null)
                        {
                            registrations.Add($"services.Add{lifetime}<{service}, {impl}>();");
                        }
                        else
                        {
                            registrations.Add($"services.Add{lifetime}<{impl}>();");
                        }
                    }
                }
            }

            var source = $@"
using Microsoft.Extensions.DependencyInjection;

namespace GeneratedDI
{{
    public static class ServiceRegistration
    {{
        public static void AddAutoServices(this IServiceCollection services)
        {{
            {string.Join("\n            ", registrations)}
        }}
    }}
}}";

            context.AddSource("DIRegistration.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }


    public class MySyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Any())
            {
                CandidateClasses.Add(classDecl);
            }
        }
    }


}
