using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Multicast
{
    [DataContract]
    public class EndpointInformation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public EndpointType Type { get; set; }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(typeof(EndpointInformation));

            serializer.WriteObject(stream, this);
            return stream.ToArray();
        }

        public static EndpointInformation Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            DataContractSerializer serializer = new DataContractSerializer(typeof(EndpointInformation));

            return serializer.ReadObject(stream) as EndpointInformation;
        }
    }
}
