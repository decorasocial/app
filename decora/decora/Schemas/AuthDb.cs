using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using decora.Models;
//using SQLite;
using Xamarin.Forms;

namespace decora.Schemas
{
    public class AuthDb
    {
        /*public SQLiteConnection _database;

        public AuthDb()
        {
            _database = DependencyService.Get<IDatabase>().GetConnection();
            _database.CreateTable<Auth>();
        }

        public string GetAuth()
        {
            List<Auth> lst = _database.Table<Auth>().ToList();

            bool isEmpty = !lst.Any();

            if (isEmpty)
                return null;

            return lst[0].code;
        }

        public Auth GetUser()
        {
            List<Auth> lst = _database.Table<Auth>().ToList();

            bool isEmpty = !lst.Any();

            if (isEmpty)
                return null;

            return lst[0];
        }

        public int InsertAuth(Auth auth)
        {
            _database.Execute("DELETE FROM Auth");

            return _database.Insert(auth);
        }

        public void update(Auth auth)
        {
            _database.Update(auth);
        }

        public void drop()
        {
            _database.Execute("DROP TABLE Auth");
            
        }*/
    }
}
