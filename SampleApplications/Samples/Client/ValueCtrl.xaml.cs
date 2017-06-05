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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Opc.Ua.SampleClient
{
    public sealed partial class ValueCtrl : UserControl
    {
        public ValueCtrl()
        {
            this.InitializeComponent();
        }

        private void ComboBoxValue_DropDownOpened(object sender, object e)
        {
            Util.ComboBoxValue_DropDownOpened(sender, e);
        }

        private void ComboBoxValue_DropDownClosed(object sender, object e)
        {
            Util.ComboBoxValue_DropDownClosed(sender, e);
        }

    }
}
