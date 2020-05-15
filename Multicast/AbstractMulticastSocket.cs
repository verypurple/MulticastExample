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
    public abstract class AbstractMulticastSocket
    {
        protected ServerInformation information;
        protected DatagramSocket socket;
        protected Guid taskId;

        public async Task Bind(string localAddress, string multicastAddress, string multicastPort)
        {
            information = new ServerInformation(localAddress, multicastAddress, multicastPort);

            socket = new DatagramSocket();
            socket.Control.MulticastOnly = true;

            if (taskId != Guid.Empty)
            {
                socket.EnableTransferOwnership(taskId);
            }

            await socket.BindEndpointAsync(new HostName(information.LocalAddress), information.MulticastPort);
            socket.JoinMulticastGroup(new HostName(information.MulticastAddress));

            socket.MessageReceived += OnMessageReceived;
        }

        protected abstract void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args);

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

        protected string GetComputerName()
        {
            return NetworkInformation.GetHostNames()
                .Where(h => h.Type == HostNameType.DomainName)
                .Select(h => h.DisplayName)
                .First();
        }
    }
}