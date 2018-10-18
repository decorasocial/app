using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Plugin.Connectivity;
using decora.Views;

namespace decora
{
    public class CheckConnection
    {

        public static bool validate()
        {

            var isConnect = CrossConnectivity.Current.IsConnected;
            return (isConnect == true);
        }

    }
}
