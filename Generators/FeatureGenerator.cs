using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Generators
{
    [Generator]
    public class FeatureGenerator : ISourceGenerator
    {
        #region AttributeDeclarations
        private const string FeatureConfigAttribute = @"using System;

namespace Generators
{
    public class FeatureConfigAttribute : Attribute
    {
        public string CommandsProject { get; set; }
        public string QueriesProject { get; set; }
        public string ProfilesProject { get; set; }
        public string FilterSpecificationsProject { get; set; }
        public string ValidatorsProject { get; set; }
        public string RepositoriesProject { get; set; }
        public string ConstantsProject { get; set; }
        public string ManagersProject { get; set; }
        public string EndpointsProject { get; set; }
        public string ControllersProject { get; set; }
    }
}";

        private const string FeatureAttribute = @"using System;

namespace Generators
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FeatureAttribute : Attribute
    {
        public FeatureAttribute(
            bool addEditEnabled = false,
            bool deleteEnabled = false,
            bool getAllEnabled = false,
            bool getAllPagedEnabled = false,
            bool getByIdEnabled = false,
            bool exportEnabled = false)
        {
            AddEditEnabled = addEditEnabled;
            DeleteEnabled = deleteEnabled;
            GetAllEnabled = getAllEnabled;
            GetAllPagedEnabled = getAllPagedEnabled;
            GetByIdEnabled = getByIdEnabled;
            ExportEnabled = exportEnabled;
        }

        public bool AddEditEnabled { get; private set; }
        public bool DeleteEnabled { get; private set; }
        public bool GetAllEnabled { get; private set; }
        public bool GetAllPagedEnabled { get; private set; }
        public bool GetByIdEnabled { get; private set; }
        public bool ExportEnabled { get; private set; }
    }
}
";
        private const string FeatureIgnoreAttribute = @"using System;

namespace Generators
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class FeatureIgnoreAttribute : Attribute { }
}
";
        #endregion
        public void Initialize(GeneratorInitializationContext context)
        {
// #if DEBUG
//             if (!Debugger.IsAttached) Debugger.Launch();
// #endif

            context.RegisterForPostInitialization((c) =>
            {
                c.AddSource("FeatureConfigAttribute", FeatureConfigAttribute);
                c.AddSource("FeatureAttribute", FeatureAttribute);
                c.AddSource("FeatureIgnoreAttribute", FeatureIgnoreAttribute);
            });
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver) return;
            CreateFeature(receiver, context);
        }

        private void CreateFeature(SyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            foreach (var @class in receiver.Classes.Where(@class => @class.Key.ContainingSymbol.Equals(@class.Key.ContainingNamespace, SymbolEqualityComparer.Default)))
            {                
                CreateCommands(context, @class);
                CreateQueries(context, @class);
            }
        }

        private static void CreateCommands(
            GeneratorExecutionContext context,
            KeyValuePair<INamedTypeSymbol, List<IPropertySymbol>> @class)
        {
            var name = @class.Key.Name;
            var properties = @class.Value.Select(p => new PropertyDefinition(p.Type.ToString(), p.ToDisplayString().Split('.').Last())).ToList();

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Commands")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"AddEdit{name}Command")
                .WithDerivations(new List<string> { "IRequest<Result<int>>" })
                .WithProperties(properties ?? new List<PropertyDefinition>())
                .Build(name);

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Commands")
                .WithUsings(new List<string> { "AutoMapper", "MediatR", "System.Threading", "System.Threading.Tasks", "Server" })
                .WithType()
                .WithName($"AddEdit{name}CommandHandler")
                .WithDerivations(new List<string> { $"IRequestHandler<AddEdit{name}Command, Result<int>>" })
                .WithFields(new List<FieldDefinition> { new("private readonly", "IMapper", "_mapper", null), new("private readonly", "IUnitOfWork<int>", "_unitOfWork", null) })
                .WithConstructors(new List<ConstructorDefinition> { new(new List<ParameterDefinition> { new("IMapper", "mapper"), new("IUnitOfWork<int>", "unitOfWork") }) })
                .WithMethods(new List<MethodDefinition>
                {
                    new(
                        "public async",
                        "Task<Result<int>>",
                        "Handle",
                        "//Body here\r\n            return null;",
                        new List<ParameterDefinition>
                        {
                            new($"AddEdit{name}Command", "command"),
                            new("CancellationToken", "cancellationToken")
                        })
                })
                .Build(name);

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Commands")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"Delete{name}Command")
                .WithDerivations(new List<string> { "IRequest<Result<int>>" })
                .WithProperties(new List<PropertyDefinition> { new("int", "Id") })
                .Build(name);

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Commands")
                .WithUsings(new List<string> { "AutoMapper", "MediatR", "System.Threading", "System.Threading.Tasks", "Server" })
                .WithType()
                .WithName($"Delete{name}CommandHandler")
                .WithDerivations(new List<string> { $"IRequestHandler<Delete{name}Command, Result<int>>" })
                .WithFields(new List<FieldDefinition> { new("private readonly", "IMapper", "_mapper", null), new("private readonly", "IUnitOfWork<int>", "_unitOfWork", null) })
                .WithConstructors(new List<ConstructorDefinition> { new(new List<ParameterDefinition> { new("IMapper", "mapper"), new("IUnitOfWork<int>", "unitOfWork") }) })
                .WithMethods(new List<MethodDefinition>
                {
                    new(
                        "public async",
                        "Task<Result<int>>",
                        "Handle",
                        "//Body here\r\n            return null;",
                        new List<ParameterDefinition>
                        {
                            new($"Delete{name}Command", "command"),
                            new("CancellationToken", "cancellationToken")
                        })
                })
                .Build(name);
        }

        private static void CreateQueries(
            GeneratorExecutionContext context,
            KeyValuePair<INamedTypeSymbol, List<IPropertySymbol>> @class)
        {
            var name = @class.Key.Name;
            var properties = @class.Value.Select(p => new PropertyDefinition(p.Type.ToString(), p.ToDisplayString().Split('.').Last())).ToList();

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "System.Collections.Generic", "Server" })
                .WithType()
                .WithName($"GetAll{name}Query")
                .WithDerivations(new List<string> { $"IRequest<Result<List<GetAll{name}Response>>>" })
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"GetAll{name}Response")
                .WithProperties(properties ?? new List<PropertyDefinition>())
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "AutoMapper", "MediatR", "System.Threading", "System.Threading.Tasks", "System.Collections.Generic", "Server" })
                .WithType()
                .WithName($"GetAll{name}QueryHandler")
                .WithDerivations(new List<string> { $"IRequestHandler<GetAll{name}Query, Result<List<GetAll{name}Response>>>" })
                .WithFields(new List<FieldDefinition> { new("private readonly", "IMapper", "_mapper", null), new("private readonly", "IUnitOfWork<int>", "_unitOfWork", null) })
                .WithConstructors(new List<ConstructorDefinition> { new(new List<ParameterDefinition> { new("IMapper", "mapper"), new("IUnitOfWork<int>", "unitOfWork") }) })
                .WithMethods(new List<MethodDefinition>
                {
                    new(
                        "public async",
                        $"Task<Result<List<GetAll{name}Response>>>",
                        "Handle",
                        "//Body here\r\n            return null;",
                        new List<ParameterDefinition>
                        {
                            new($"GetAll{name}Query", "query"),
                            new("CancellationToken", "cancellationToken")
                        })
                })
                .Build(name);

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "System.Collections.Generic", "Server" })
                .WithType()
                .WithName($"GetAllPaged{name}Query")
                .WithDerivations(new List<string> { $"IRequest<Result<List<GetAllPaged{name}Response>>>" })
                .WithConstructors(new List<ConstructorDefinition>
                {
                    new(
                        new List<ParameterDefinition>
                        {
                            new("int", "pageNumber"),
                            new("int", "pageSize"),
                            new("string", "searchString"),
                            new("string", "orderBy")
                        },
                        false,
                        @"PageNumber = pageNumber;
            PageSize = pageSize;
            SearchString = searchString;
            if (!string.IsNullOrWhiteSpace(orderBy)) OrderBy = orderBy.Split(',');")
                })
                .WithProperties(new List<PropertyDefinition>
                {
                    new("int", "PageNumber"),
                    new("int", "PageSize"),
                    new("string", "SearchString"),
                    new("string[]", "OrderBy")
                })
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"GetAllPaged{name}Response")
                .WithProperties(properties ?? new List<PropertyDefinition>())
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "AutoMapper", "MediatR", "System.Threading", "System.Threading.Tasks", "System.Collections.Generic", "Server" })
                .WithType()
                .WithName($"GetAllPaged{name}QueryHandler")
                .WithDerivations(new List<string> { $"IRequestHandler<GetAllPaged{name}Query, Result<List<GetAllPaged{name}Response>>>" })
                .WithFields(new List<FieldDefinition> { new("private readonly", "IMapper", "_mapper", null), new("private readonly", "IUnitOfWork<int>", "_unitOfWork", null) })
                .WithConstructors(new List<ConstructorDefinition> { new(new List<ParameterDefinition> { new("IMapper", "mapper"), new("IUnitOfWork<int>", "unitOfWork") }) })
                .WithMethods(new List<MethodDefinition>
                {
                    new(
                        "public async",
                        $"Task<Result<List<GetAllPaged{name}Response>>>",
                        "Handle",
                        "//Body here\r\n            return null;",
                        new List<ParameterDefinition>
                        {
                            new($"GetAllPaged{name}Query", "query"),
                            new("CancellationToken", "cancellationToken")
                        })
                })
                .Build(name);

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"Get{name}ByIdQuery")
                .WithDerivations(new List<string> { $"IRequest<Result<Get{name}ByIdResponse>>" })
                .WithProperties(new List<PropertyDefinition> { new("int", "Id") })
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"Get{name}ByIdResponse")
                .WithProperties(properties ?? new List<PropertyDefinition>())
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "AutoMapper", "MediatR", "System.Threading", "System.Threading.Tasks", "Server" })
                .WithType()
                .WithName($"Get{name}ByIdQueryHandler")
                .WithDerivations(new List<string> { $"IRequestHandler<Get{name}ByIdQuery, Result<Get{name}ByIdResponse>>" })
                .WithFields(new List<FieldDefinition> { new("private readonly", "IMapper", "_mapper", null), new("private readonly", "IUnitOfWork<int>", "_unitOfWork", null) })
                .WithConstructors(new List<ConstructorDefinition> { new(new List<ParameterDefinition> { new("IMapper", "mapper"), new("IUnitOfWork<int>", "unitOfWork") }) })
                .WithMethods(new List<MethodDefinition>
                {
                    new(
                        "public async",
                        $"Task<Result<Get{name}ByIdResponse>>",
                        "Handle",
                        "//Body here\r\n            return null;",
                        new List<ParameterDefinition>
                        {
                            new($"Get{name}ByIdQuery", "query"),
                            new("CancellationToken", "cancellationToken")
                        })
                })
                .Build(name);

            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "MediatR", "Server" })
                .WithType()
                .WithName($"Export{name}Query")
                .WithDerivations(new List<string> { $"IRequest<Result<string>>" })
                .WithConstructors(new List<ConstructorDefinition>
                {
                    new(
                        new List<ParameterDefinition>
                        {
                            new("string", "searchString", string.Empty)
                        },
                        false,
                        "SearchString = searchString;")
                })
                .WithProperties(new List<PropertyDefinition> { new("string", "SearchString") })
                .Build(name);
            TypeBuilder.Create(context)
                .WithNamespace($"Generators.{name}.Queries")
                .WithUsings(new List<string> { "AutoMapper", "MediatR", "System.Threading", "System.Threading.Tasks", "Server" })
                .WithType()
                .WithName($"Export{name}QueryHandler")
                .WithDerivations(new List<string> { $"IRequestHandler<Export{name}Query, Result<string>>" })
                .WithFields(new List<FieldDefinition> { new("readonly IUnitOfWork<int>", "_unitOfWork") })
                .WithConstructors(new List<ConstructorDefinition> { new(new List<ParameterDefinition> { new("IUnitOfWork<int>", "unitOfWork") }) })
                .WithMethods(new List<MethodDefinition>
                {
                    new(
                        "public async",
                        $"Task<Result<string>>",
                        "Handle",
                        "//Body here\r\n            return null;",
                        new List<ParameterDefinition>
                        {
                            new($"Export{name}Query", "query"),
                            new("CancellationToken", "cancellationToken")
                        })
                })
                .Build(name);
        }

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            public Dictionary<INamedTypeSymbol, List<IPropertySymbol>> Classes { get; set; } = new();
            private readonly List<string> _assemblies = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                var assemblyName = context.SemanticModel.Compilation.AssemblyName;
                if (!_assemblies.Contains(assemblyName))
                    _assemblies.Add(assemblyName);
                if (context.Node is not ClassDeclarationSyntax { AttributeLists.Count: > 0} classDeclarationSyntax) return;
                var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (namedTypeSymbol == null || !namedTypeSymbol.GetAttributes().Any(ad =>
                    ad.AttributeClass != null &&
                    ad.AttributeClass.ToDisplayString().Equals("Generators.FeatureAttribute")) ||
                    Classes.ContainsKey(namedTypeSymbol)) return;
                var propertySymbols = namedTypeSymbol.GetMembers()
                    .Where(s => s.Kind == SymbolKind.Property)
                    .OfType<IPropertySymbol>()
                    .Where(ps => !ps.GetAttributes().Any(ad => ad.AttributeClass != null && ad.AttributeClass.ToDisplayString().Equals("Generators.FeatureIgnoreAttribute")))
                    .ToList();
                Classes.Add(namedTypeSymbol, propertySymbols);
            }
        }

        private class FeatureConfig
        {
            public string CommandsProject { get; set; }
            public string QueriesProject { get; set; }
            public string ProfilesProject { get; set; }
            public string FilterSpecificationsProject { get; set; }
            public string ValidatorsProject { get; set; }
            public string RepositoriesProject { get; set; }
            public string ConstantsProject { get; set; }
            public string ManagersProject { get; set; }
            public string EndpointsProject { get; set; }
            public string ControllersProject { get; set; }
        }
    }
}