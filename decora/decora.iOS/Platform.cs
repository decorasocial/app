using System;
using System.Collections.Generic;
using System.Text;
//using SQLite.Net.Interop;
using decora.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(Platform))]
namespace decora.iOS
{
    class Platform : IPlatform
    {
        /*public ISQLitePlatform GetPlatform()
        {
            return new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();
        }*/
    }
}
