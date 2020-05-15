using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MulticastExample
{
    using Multicast;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario_Client : Page
    {
        MulticastClient client;

        public Scenario_Client()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string localAddress = e.Parameter as string;

            client = new MulticastClient();
            await client.Bind(localAddress, "224.1.0.0", "9000");
            client.ServerDiscovered += OnServerDiscovered;
        }

        private async void Discover(object sender, RoutedEventArgs e)
        {
            await client.Discover();
        }

        private void OnServerDiscovered(MulticastClient sender, EndpointInformation args)
        {
            _ = CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MessagesListBox.Items.Add($"Discovered {args.Name} at {args.Address}");
            });
        }
    }
}
