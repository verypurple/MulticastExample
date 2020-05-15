using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Multicast
{
    [DataContract]
    public class ServerInformation
    {
        [DataMember]
        public string LocalAddress { get; set; }

        [DataMember]
        public string MulticastAddress { get; set; }

        [DataMember]
        public string MulticastPort { get; set; }

        public ServerInformation(string localAddress, string multicastAddress, string multicastPort)
        {
            LocalAddress = localAddress;
            MulticastAddress = multicastAddress;
            MulticastPort = multicastPort;
        }

        public IBuffer Serialize()
        {
            MemoryStream stream = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(typeof(ServerInformation));

            serializer.WriteObject(stream, this);
            byte[] bytes = stream.ToArray();

            return CryptographicBuffer.CreateFromByteArray(bytes);
        }

        public static ServerInformation Deserialize(IBuffer buffer)
        {
            byte[] bytes;
            CryptographicBuffer.CopyToByteArray(buffer, out bytes);

            MemoryStream stream = new MemoryStream(bytes);
            DataContractSerializer serializer = new DataContractSerializer(typeof(ServerInformation));

            return serializer.ReadObject(stream) as ServerInformation;
        }
    }
}
