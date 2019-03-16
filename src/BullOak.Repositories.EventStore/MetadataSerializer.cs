namespace BullOak.Repositories.EventStore
{
    using System.Text;
    using Newtonsoft.Json;

    internal static class MetadataSerializer
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        public static byte[] Serialize(IHoldMetadata metadata)
            => Encoding.GetBytes(JsonConvert.SerializeObject(metadata));

        public static (IHoldMetadata metadata, int version) DeserializeMetadata(byte[] metadata)
        {
            var data = Encoding.GetString(metadata);
            var asJson = Newtonsoft.Json.Linq.JObject.Parse(data);

            var metadataVersion = asJson[nameof(IHoldMetadata.MetadataVersion)].ToObject<int>();

            return MetadataFactory.GetMetadataFrom(metadataVersion, asJson);
        }
    }
}
