using System.Text.Json;
using System.Text.Json.Serialization;

namespace tests;

public static class SerializerExtensions
{
    public static void PrintAsJson(this object obj, ITestOutputHelper? outputHelper = null, JsonSerializerOptions? opts = null)
    {
        var metaDataMessage = $"Object type: {obj.GetType().FullName}";
        var json = JsonSerializer.Serialize(obj, opts ?? new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        });
        var finalMessage = $"{metaDataMessage}\nJSON Representation:\n{json}\n";
        if (outputHelper == null)
        {
            Console.WriteLine(finalMessage);
        }
        else
        {
            outputHelper.WriteLine(finalMessage);
        }
            
    }
}