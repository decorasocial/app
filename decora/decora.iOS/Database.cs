using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using decora.iOS;
//using SQLite.Net;
using System.IO;

[assembly:Dependency(typeof(Database))]
namespace decora.iOS
{
    class Database : IDatabase
    {
        /*public SQLiteConnection GetConnection()
        {
            string db = "portal_decoracao.db3";

            var personalPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string libraryPath = Path.Combine(personalPath, "..", "Library");

            string path = Path.Combine(libraryPath, db);

            var platform = DependencyService.Get<Platform>().GetPlatform();

            return new SQLiteConnection(platform, path);

        }*/
    }
}
