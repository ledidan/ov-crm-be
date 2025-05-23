


using Data.ThirdPartyModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Helpers
{

    public static class GetRegionTimeZone
    {
        public static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); // Linux (if applicable)
                }
                catch (TimeZoneNotFoundException ex)
                {
                    throw new Exception("Vietnam time zone not found on this system.", ex);
                }
            }
        }
    }

    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return true; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput); // Or use JsonDocument.Parse with System.Text.Json
                    return true;
                }
                catch (Newtonsoft.Json.JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine($"Invalid JSON Error: {jex.Message} for input: {strInput}");
                    return false;
                }
                catch (System.Text.Json.JsonException sex)
                {
                    Console.WriteLine($"Invalid JSON Error (System.Text.Json): {sex.Message} for input: {strInput}");
                    return false;
                }
            }
            return false;
        }
    }
    public class ImportLogger : IImportLogger
    {
        public async Task<string> SaveImportErrorsToFile<T>(List<ImportError<T>> errors, string prefix = "import_errors")
        {
            if (errors == null || !errors.Any()) return null;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{prefix}_{timestamp}.log";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImportLogs");
            var filePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            var lines = errors.Select(e =>
            {
                var type = e.Flag ? "[VALIDATION]" : "[SYSTEM]";
                var basic = $"[Row {e.Row}] {type} {e.Message}";

                var dataJson = e.Data != null ? $"Data: {System.Text.Json.JsonSerializer.Serialize(e.Data)}" : "";
                return $"{basic} {dataJson}";
            });

            await File.WriteAllLinesAsync(filePath, lines);

            return $"/ImportLogs/{fileName}";
        }
    }
}