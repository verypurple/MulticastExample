using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Multicast
{
    public class MulticastClient
    {
        public TypedEventHandler<MulticastClient, EndpointInformation> ServerDiscovered;

        protected ServerInformation information;
        protected DatagramSocket socket;

        public async Task Discover()
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }

            EndpointInformation message = new EndpointInformation()
            {
                Address = information.LocalAddress,
                Type = EndpointType.Client
            };

            await SendMessage(message);
        }

        public async Task Bind(string localAddress, string multicastAddress, string multicastPort)
        {
            information = new ServerInformation(localAddress, multicastAddress, multicastPort);

            socket = new DatagramSocket();
            socket.Control.MulticastOnly = true;

            await socket.BindEndpointAsync(new HostName(information.LocalAddress), information.MulticastPort);
            socket.JoinMulticastGroup(new HostName(information.MulticastAddress));

            socket.MessageReceived += OnMessageReceived;
        }

        protected void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
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

        protected async Task SendMessage(EndpointInformation message)
        {
            EndpointPair endpoint = new EndpointPair(
                new HostName(information.LocalAddress),
                information.MulticastPort,
                new HostName(information.MulticastAddress),
                information.MulticastPort
            );

            using (IOutputStream outputStream = await socket.GetOutputStreamAsync(endpoint))
            using (DataWriter writer = new DataWriter(outputStream))
            {
                byte[] data = message.Serialize();
                writer.WriteBytes(data);
                await writer.StoreAsync();
            }
        }
    }
}
