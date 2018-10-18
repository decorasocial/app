using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using decora.Droid;
using SQLite;
using Xamarin.Forms;

[assembly:Dependency(typeof(Database))]
namespace decora.Droid
{
    class Database : IDatabase
    {
        public SQLiteConnection GetConnection()
        {
            string db = "porta_decoracao_db.db3" ;

            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),db);

            //var platform = DependencyService.Get<Platform>().GetPlatform();

            return new SQLiteConnection(path);

        }
    }
}