using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Multicast
{
    public class MulticastServer
    {
        protected ServerInformation information;
        protected DatagramSocket socket;
        protected Guid taskId;

        public MulticastServer(Guid taskId)
        {
            this.taskId = taskId;
        }

        protected MulticastServer()
        {
        }

        public async Task Bind(string localAddress, string multicastAddress, string multicastPort)
        {
            information = new ServerInformation(localAddress, multicastAddress, multicastPort);

            socket = new DatagramSocket();
            socket.Control.MulticastOnly = true;
            socket.EnableTransferOwnership(taskId);

            await socket.BindEndpointAsync(new HostName(information.LocalAddress), information.MulticastPort);
            socket.JoinMulticastGroup(new HostName(information.MulticastAddress));

            socket.MessageReceived += OnMessageReceived;
        }

        public async Task TransferOwnership()
        {
            socket.MessageReceived -= OnMessageReceived;
            await socket.CancelIOAsync();

            IBuffer data = information.Serialize();
            SocketActivityContext context = new SocketActivityContext(data);

            socket.TransferOwnership(nameof(MulticastServer), context);
            socket = null;
        }

        public void ReclaimOwnership()
        {
            if (SocketActivityInformation.AllSockets.TryGetValue(nameof(MulticastServer), out SocketActivityInformation socketInformation))
            {
                socket = socketInformation.DatagramSocket;
                socket.MessageReceived += OnMessageReceived;
            }
        }

        private async void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            string remoteAddress = args.RemoteAddress.CanonicalName;

            // Reject messages from this computer
            if (remoteAddress == information.LocalAddress)
            {
                return;
            }

            using (DataReader reader = args.GetDataReader())
            {
                byte[] data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);

                EndpointInformation message = EndpointInformation.Deserialize(data);

                if (message != null)
                {
                    // Did message originate from a client?
                    if (message.Type == EndpointType.Client)
                    {
                        message = new EndpointInformation()
                        {
                            Address = information.LocalAddress,
                            Type = EndpointType.Server
                        };

                        await SendMessage(message);
                    }
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
