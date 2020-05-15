using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Multicast
{
    public class MulticastClient : AbstractMulticastSocket
    {
        public TypedEventHandler<MulticastClient, EndpointInformation> ServerDiscovered;

        public async Task Discover()
        {
            if (socket != null)
            {
                EndpointInformation message = new EndpointInformation()
                {
                    Name = GetComputerName(),
                    Address = information.LocalAddress,
                    Type = EndpointType.Client
                };

                await SendMessage(message);
            }
        }

        protected override void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            string remoteAddress = args.RemoteAddress.CanonicalName;

            // Reject messages from this computer
            if (remoteAddress == information.LocalAddress)
            {
                return;
            }

            DataReader reader = args.GetDataReader();
            byte[] data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);

            EndpointInformation message = EndpointInformation.Deserialize(data);

            if (message != null)
            {
                // Did message originate from a server?
                if (message.Type == EndpointType.Server)
                {
                    ServerDiscovered?.Invoke(this, message);
                }
            }
        }
    }
}
