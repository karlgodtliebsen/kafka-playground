namespace KafkaFlow.Domain
{
    public static class JsonExtensions
    {

        public static string ToJson(this object obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }

    }
}
