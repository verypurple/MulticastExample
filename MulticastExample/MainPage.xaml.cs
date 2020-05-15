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
using Windows.Networking.Connectivity;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MulticastExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string localAddress;

        public MainPage()
        {
            this.InitializeComponent();

            // Defaults to the adapter that has internet access.
            // This may be wrong if you have multiple interfaces but it's good enough as an example, ig.
            var currentAdapter = NetworkInformation.GetInternetConnectionProfile()
                .NetworkAdapter
                .NetworkAdapterId;

            localAddress = NetworkInformation.GetHostNames()
                .Where(h => h.IPInformation != null)
                .Where(h => h.IPInformation.NetworkAdapter.NetworkAdapterId == currentAdapter)
                .Select(h => h.CanonicalName)
                .First();
        }

        private void StartServer(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Scenario_Server), localAddress);
        }

        private void StartClient(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Scenario_Client), localAddress);
        }
    }
}
