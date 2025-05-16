


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
}