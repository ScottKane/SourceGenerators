using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generators
{
    internal sealed class TypeBuilder : 
        INamespaceStage,
        IUsingsStage,
        ITypeStage,
        INameStage,
        ITypeInitializerStage
    {
        private static GeneratorExecutionContext _context;
        private string _namespace;
        private List<string> _usings = new();
        private string _type;
        private string _name;
        private List<string> _derivations = new();
        private List<string> _fields = new();
        private List<string> _constructors = new();
        private List<string> _properties = new();
        private List<string> _methods = new();

        private string _source;

        private TypeBuilder() { }

        public static INamespaceStage Create(GeneratorExecutionContext context)
        {
            _context = context;
            return new TypeBuilder();
        }

        public IUsingsStage WithNamespace(string @namespace)
        {
            _namespace = @namespace ?? string.Empty;
            return this;
        }

        public ITypeStage WithUsings(List<string> usings)
        {
            _usings = usings.Select(u => $"using {u};").ToList() ?? new();
            return this;
        }

        public INameStage WithType(TypeBuilderType type)
        {
            _type = type.ToString().ToLower();
            return this;
        }

        public ITypeInitializerStage WithName(string name)
        {
            _name = name ?? string.Empty;
            return this;
        }

        public ITypeInitializerStage WithDerivations(List<string> derivations)
        {
            _derivations = derivations ?? new();
            return this;
        }

        public ITypeInitializerStage WithFields(List<FieldDefinition> fields)
        {
            _fields = fields.Select(f => $"{f.AccessModifier} {f.Type} {f.Name}{(f.Default is null ? string.Empty : $" = {f.Default}")};").ToList() ?? new();
            return this;
        }

        public ITypeInitializerStage WithConstructors(List<ConstructorDefinition> constructors)
        {
            _constructors = constructors.Select(c => $@"{c.AccessModifier} {_name}({string.Join(", ", c.Parameters)})
        {{
            {(c.Inject ? string.Join("\r\n            ", c.Parameters.Select(p => $"_{p.Split(' ').Last()} = {p.Split(' ').Last()};")) : string.Empty)}{(c.Inject ? "\r\n" : string.Empty)}{c.Body}
        }}").ToList() ?? new();
            return this;
        }

        public ITypeInitializerStage WithProperties(List<PropertyDefinition> properties)
        {
            _properties = properties.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}{(p.Default is null ? string.Empty : $" = {p.Default};")}").ToList() ?? new();
            return this;
        }

        public ITypeInitializerStage WithMethods(List<MethodDefinition> methods)
        {
            _methods = methods.Select(m => $@"{m.AccessModifier} {m.ReturnType} {m.Name}({string.Join(", ", m.Parameters)})
        {{
            {m.Body}
        }}").ToList() ?? new();
            return this;
        }

        public void Build(string featureName = null)
        {
            var feature = $"{(featureName is null ? string.Empty : $"//Feature:{featureName}\r\n")}";
            var usings = _usings.Any() ? $"{string.Join("\r\n", _usings)}\r\n\r\n" : string.Empty;
            var derivations = _derivations.Any() ? $" : {string.Join(",\r\n        ", _derivations)}" : string.Empty;
            var fields = $"{(_fields.Any() ? $"        {string.Join("\r\n        ", _fields)}" : string.Empty)}{(_fields.Any() && (_constructors.Any() || _properties.Any() || _methods.Any()) ? "\r\n\r\n" : string.Empty)}";
            var constructors = $"{(_constructors.Any() ? $"        {string.Join("\r\n        ", _constructors)}" : string.Empty)}{(_constructors.Any() && (_properties.Any() || _methods.Any()) ? "\r\n\r\n" : string.Empty)}";
            var properties = $"{(_properties.Any() ? $"        {string.Join("\r\n        ", _properties)}" : string.Empty)}{(_properties.Any() && _methods.Any() ? "\r\n\r\n" : string.Empty)}";
            var methods = _methods.Any() ? $"        {string.Join("\r\n        ", _methods)}" : string.Empty;

            _source = $@"{feature}{usings}namespace {_namespace}
{{
    public partial class {_name}{derivations}
    {{
{fields}{constructors}{properties}{methods}
    }}
}}
";
            _context.AddSource(
                _name,
                SourceText.From(
                    _source,
                    Encoding.UTF8));
        }
    }

    internal sealed class ConstructorDefinition
    {
        internal ConstructorDefinition(string accessModifier, List<ParameterDefinition> parameters, bool inject = true, string body = "")
        {
            AccessModifier = accessModifier;
            Parameters = parameters.Select(p => $"{p.Type} {p.Name.Replace("_", string.Empty)}").ToList() ?? new();
            Body = body;
        }

        internal ConstructorDefinition(List<ParameterDefinition> parameters, bool inject = true, string body = "")
        {
            Parameters = parameters.Select(p => $"{p.Type} {p.Name.Replace("_", string.Empty)}").ToList() ?? new();
            Body = body;
        }

        public string AccessModifier { get; } = "public";
        public List<string> Parameters { get; }
        public bool Inject { get; }
        public string Body { get; }
    }

    internal sealed class FieldDefinition
    {
        internal FieldDefinition(string accessModifier, string type, string name, string @default = null)
        {
            AccessModifier = accessModifier;
            Type = type;
            Name = name;
            Default = @default;
        }

        internal FieldDefinition(string type, string name, string @default = null)
        {
            Type = type;
            Name = name;
            Default = @default;
        }

        public string AccessModifier { get; } = "private";
        public string Type { get; }
        public string Name { get; }
        public string Default { get; }
    }

    internal sealed class PropertyDefinition
    {
        internal PropertyDefinition(string accessModifier, string type, string name, string @default = null)
        {
            AccessModifier = accessModifier;
            Type = type;
            Name = name;
            Default = @default;
        }

        internal PropertyDefinition(string type, string name, string @default = null)
        {
            Type = type;
            Name = name;
            Default = @default;
        }

        public string AccessModifier { get; } = "public";
        public string Type { get; }
        public string Name { get; }
        public string Default { get; }
    }

    internal sealed class MethodDefinition
    {
        internal MethodDefinition(string accessModifier, string type, string name, string body, List<ParameterDefinition> parameters = null)
        {
            AccessModifier = accessModifier;
            ReturnType = type;
            Name = name;
            Body = body;
            Parameters = parameters.Select(p => $"{p.Type} {p.Name}{(p.Default is null ? string.Empty : $" = {p.Default}")}").ToList() ?? new();
        }

        internal MethodDefinition(string type, string name, string body, List<ParameterDefinition> parameters = null)
        {
            ReturnType = type;
            Name = name;
            Body = body;
            Parameters = parameters.Select(p => $"{p.Type} {p.Name}{(p.Default is null ? string.Empty : $" = {p.Default}")}").ToList() ?? new();
        }

        public string AccessModifier { get; } = "public";
        public string ReturnType { get; }
        public string Name { get; }
        public string Body { get; }
        public List<string> Parameters { get; }
    }

    internal sealed class ParameterDefinition
    {
        internal ParameterDefinition(string type, string name, string @default = null)
        {
            Type = type;
            Name = name;
            Default = @default;
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Default { get; set; }
    }

    public enum TypeBuilderType
    {
        Class,
        Struct
    }

    internal interface INamespaceStage
    {
        public IUsingsStage WithNamespace(string @namespace);
    }

    internal interface IUsingsStage
    {
        public ITypeStage WithUsings(List<string> usings);
    }

    internal interface ITypeStage
    {
        public INameStage WithType(TypeBuilderType type = TypeBuilderType.Class);
    }

    internal interface INameStage
    {
        public ITypeInitializerStage WithName(string name);
    }

    internal interface ITypeInitializerStage
    {
        public ITypeInitializerStage WithDerivations(List<string> derivations);
        public ITypeInitializerStage WithConstructors(List<ConstructorDefinition> constructors);
        public ITypeInitializerStage WithFields(List<FieldDefinition> fields);
        public ITypeInitializerStage WithProperties(List<PropertyDefinition> properties);
        public ITypeInitializerStage WithMethods(List<MethodDefinition> methods);
        public void Build(string featureName = null);
    }
}
