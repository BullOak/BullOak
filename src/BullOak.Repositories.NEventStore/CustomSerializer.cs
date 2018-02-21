namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using BullOak.Repositories.StateEmit;
    using global::NEventStore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class CustomSerializer : global::NEventStore.Serialization.ISerialize
    {
        public struct Envelope
        {
            public string payloadTypeName;
            public string payload;
        }

        public IHoldAllConfiguration Configuration { get; set; }

        public CustomSerializer(IHoldAllConfiguration config)
            => Configuration = config;

        public T Deserialize<T>(Stream input)
        {
            Type tType = typeof(T);
            T value = default(T);
            using (var reader = new BinaryReader(input, Encoding.UTF8, true))
            {
                var data = reader.ReadString();
                if (typeof(List<EventMessage>) == (tType))
                {
                    var envelopes = JsonConvert.DeserializeObject<Envelope[]>(data)
                        .Select(x => new { Type = Type.GetType(x.payloadTypeName), Data = x.payload })
                        .Select(x =>
                        {
                            if (x.Type.IsInterface)
                            {
                                //Do Magic
                                var eventInstaance = Configuration.StateFactory.GetState(x.Type);
                                var switchable = eventInstaance as ICanSwitchBackAndToReadOnly;
                                switchable.CanEdit = true;
                                JObject message = JObject.Parse(x.Data);
                                var body = (JObject)message["Body"];
                                var canEdit = body.Property("canEdit");
                                canEdit.Remove();
                                var jsonReader = body.CreateReader();
                                var serializer = new Newtonsoft.Json.JsonSerializer();
                                serializer.Populate(jsonReader, eventInstaance);

                                switchable.CanEdit = false;

                                return new EventMessage
                                {
                                    Body = eventInstaance,
                                    Headers = new Dictionary<string, object>
                                    {
                                        {"EventType", x.Type }
                                    }
                                };
                            }
                            else
                            {
                                var message = JObject.Parse(x.Data);
                                var payload = message.GetValue("Body").ToObject(x.Type);
                                return new EventMessage
                                {
                                    Body = payload
                                };
                            }
                        })
                        .ToList();

                    value = (T)(object)envelopes;
                }
                else if (typeof(IDictionary<string, object>).IsAssignableFrom(tType))
                {
                    value = (T)(object)JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                }
            }

            return value;
        }

        public void Serialize<T>(Stream output, T graph)
        {
            switch ((object)graph)
            {
                case IEnumerable<EventMessage> messages:
                    SerializeMessages(output, messages);
                    break;
                case IDictionary<string, object> headers:
                    SerializeHeaders(output, headers);
                    break;
            }
            //throw new NotImplementedException();
        }

        private void SerializeMessages(Stream output, IEnumerable<EventMessage> messages)
        {
            var envelopes = messages.Select(x =>
            {
                Type eventType = x.Headers.TryGetValue("EventType", out object t)
                    ? (Type)t
                    : x.Body.GetType();

                return new Envelope
                {
                    payload = JsonConvert.SerializeObject(x),
                    payloadTypeName = eventType.AssemblyQualifiedName,
                };
            }).ToArray();

            var serialized = JsonConvert.SerializeObject(envelopes);

            using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
            {
                writer.Write(serialized);
            }
        }

        private void SerializeHeaders(Stream output, IDictionary<string, object> headers)
        {
            byte[] bytes;
            if (headers.Count != 0)
            {
                throw new NotImplementedException();
            }
            else
                bytes = new byte[0];

            using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
            {
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }
    }
}
