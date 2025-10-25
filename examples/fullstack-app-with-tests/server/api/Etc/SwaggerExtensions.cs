using System.Reflection;
using NJsonSchema;
using NSwag.Generation;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

public static class SwaggerExtensions
{
    /// <summary>
    /// Adds a type to Swagger schemas
    /// </summary>
    public static void AddTypeToSwagger<T>(this OpenApiDocumentGeneratorSettings settings)
    {
        settings.DocumentProcessors.Add(new TypeMapDocumentProcessor<T>());
    }

    /// <summary>
    /// Adds string constants from a static class to OpenAPI schema
    /// Usage: config.AddStringConstants(typeof(SieveConstants));
    /// </summary>
    public static void AddStringConstants(this OpenApiDocumentGeneratorSettings settings, Type type)
    {
        var processorType = typeof(StringConstantsDocumentProcessor<>).MakeGenericType(type);
        var processor = (IDocumentProcessor)Activator.CreateInstance(processorType)!;
        settings.DocumentProcessors.Add(processor);
    }
}

public class TypeMapDocumentProcessor<T> : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        var schema = context.SchemaGenerator.Generate(typeof(T));
        context.Document.Definitions[typeof(T).Name] = schema;
    }
}

/// <summary>
/// Document processor that extracts string constant values from a static class
/// and adds them to the OpenAPI schema as an object with string properties.
/// This generates both an interface and a const object in TypeScript.
/// </summary>
public class StringConstantsDocumentProcessor<T> : IDocumentProcessor where T : class
{
    public void Process(DocumentProcessorContext context)
    {
        var type = typeof(T);

        // Create the schema with x-enumNames extension to hint at constant values
        var schema = new JsonSchema
        {
            Type = JsonObjectType.Object,
            Description = $"String constants from {type.Name}",
            AdditionalPropertiesSchema = null,
            AllowAdditionalProperties = false
        };

        // Get all static string properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead);

        var constantValues = new Dictionary<string, string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(null) as string;
            if (value != null)
            {
                constantValues[prop.Name] = value;

                schema.Properties[prop.Name] = new JsonSchemaProperty
                {
                    Type = JsonObjectType.String,
                    IsReadOnly = true,
                    Default = value,
                    Description = $"Constant value: \"{value}\""
                };
            }
        }

        // Store constant values as extension data for potential use by code generators
        schema.ExtensionData = new Dictionary<string, object?>
        {
            ["x-constant-values"] = constantValues
        };

        context.Document.Definitions[type.Name] = schema;
    }
}
