using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Opc.Ua.Client.Controls.Endpoints
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HostDlg : Page
    {
        private Popup dialogPopup = new Popup();

        string hostname = String.Empty;

        public HostDlg()
        {
            this.InitializeComponent();
            HostName.Text   = Utils.GetHostName();
            hostname        = HostName.Text;
        }

        public async Task<String> ShowDialog()
        {

            TaskCompletionSource<String> tcs = new TaskCompletionSource<String>();
            // display dialog
            dialogPopup.Child = this;
            dialogPopup.IsOpen = true;
            dialogPopup.Closed += (o, e) =>
            {
                tcs.SetResult(hostname);
            };
            return await tcs.Task;
        }

        private void OkBTN_Click(object sender, RoutedEventArgs e)
        {
            hostname = HostName.Text.Trim();
            dialogPopup.IsOpen  = false;
        }

        private void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            dialogPopup.IsOpen = false;
        }

        private void HostName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
