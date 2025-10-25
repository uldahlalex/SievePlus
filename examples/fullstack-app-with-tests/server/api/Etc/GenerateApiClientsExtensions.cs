using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.TypeScript;
using NSwag.Generation;

namespace api;

public static class GenerateApiClientsExtensions
{
    public static async Task GenerateApiClientsFromOpenApi(this WebApplication app, string path)
    {
        // Step 1: Generate OpenAPI document with full documentation
        var document = await app.Services.GetRequiredService<IOpenApiDocumentGenerator>()
            .GenerateAsync("v1");

        // Step 2: Serialize the document to JSON to verify it contains documentation
        var openApiJson = document.ToJson();

        // Optional: Save the OpenAPI JSON to verify it has documentation
        var openApiPath = Path.Combine(Directory.GetCurrentDirectory(), "openapi-with-docs.json");
        await File.WriteAllTextAsync(openApiPath, openApiJson);

        // Step 3: Parse the document back from JSON to ensure we're only using what's in the OpenAPI spec
        var documentFromJson = await OpenApiDocument.FromJsonAsync(openApiJson);

        // Step 4: Generate TypeScript client from the parsed OpenAPI document
        var settings = new TypeScriptClientGeneratorSettings
        {
            Template = TypeScriptTemplate.Fetch,
            // = true,  // Enable JSDoc generation
            TypeScriptGeneratorSettings =
            {
                TypeStyle = TypeScriptTypeStyle.Interface,
                DateTimeType = TypeScriptDateTimeType.String,
                NullValue = TypeScriptNullValue.Undefined,
                TypeScriptVersion = 5.2m,
                GenerateCloneMethod = false,
                MarkOptionalProperties = false,
                GenerateConstructorInterface = true,
                ConvertConstructorInterfaceData = true,
                EnumStyle = TypeScriptEnumStyle.Enum
            }
        };

        // Step 5: Generate TypeScript client from the parsed OpenAPI document
        var generator = new TypeScriptClientGenerator(documentFromJson, settings);
        var code = generator.GenerateFile();

        // Step 6: Post-process to add const objects for constant schemas
        code = AddConstantObjects(code, documentFromJson);

        var outputPath = Path.Combine(Directory.GetCurrentDirectory() + path);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await File.WriteAllTextAsync(outputPath, code);


        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("OpenAPI JSON with documentation saved at: " + openApiPath);
        logger.LogInformation("TypeScript client generated at: " + outputPath);
    }

    /// <summary>
    /// Adds const objects with actual values for schemas marked with constant values
    /// </summary>
    private static string AddConstantObjects(string code, OpenApiDocument document)
    {
        var constantExports = new List<string>();

        foreach (var schema in document.Definitions)
        {
            // Check if schema has constant values extension data
            if (schema.Value.ExtensionData?.TryGetValue("x-constant-values", out var constantValuesObj) == true)
            {
                if (constantValuesObj is Dictionary<string, object?> constantValues)
                {
                    // Generate const object export
                    var constName = schema.Key;
                    var constLines = new List<string>
                    {
                        $"",
                        $"/** Constant values for {constName} */",
                        $"export const {constName} = {{"
                    };

                    foreach (var kvp in constantValues)
                    {
                        constLines.Add($"    {kvp.Key}: \"{kvp.Value}\",");
                    }

                    // Remove trailing comma from last property
                    if (constLines.Count > 2)
                    {
                        constLines[^1] = constLines[^1].TrimEnd(',');
                    }

                    constLines.Add($"}} as const;");

                    constantExports.Add(string.Join(Environment.NewLine, constLines));
                }
            }
        }

        // Insert const objects after their interface definitions
        if (constantExports.Count > 0)
        {
            // Find the last interface definition and append const objects there
            var lastInterfaceIndex = code.LastIndexOf("export interface");
            if (lastInterfaceIndex != -1)
            {
                // Find the end of the last interface (closing brace)
                var closeBraceIndex = code.IndexOf('}', lastInterfaceIndex);
                if (closeBraceIndex != -1)
                {
                    var insertIndex = closeBraceIndex + 1;
                    var allConstants = string.Join(Environment.NewLine, constantExports);
                    code = code.Insert(insertIndex, Environment.NewLine + allConstants);
                }
            }
        }

        return code;
    }
}