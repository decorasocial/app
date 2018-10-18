using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace decora
{
    public class ImageRender
    {

        public static string file_default { get; set; }

        public static UriImageSource display(string path, string file)
        {

            string urlBase = "https://firebasestorage.googleapis.com/v0/b/decora-social.appspot.com/o/{0}%2F{1}?alt=media&token={2}";
            string token = "776f3932-ed8a-4a25-a83d-5d84d2385179";

            Uri urlCheck = new Uri(string.Format(urlBase, path, file, token));

            return new UriImageSource() { Uri = urlCheck };
            
        }

        public static string newName(Stream file)
        {

            FileStream tmpStream = file as FileStream;
            string extensionWithDot = Path.GetExtension(tmpStream.Name);

            DateTime foo = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            Random rnd = new Random();

            return string.Format("{0}{1}", getMD5Hash(string.Format("{0}{1}",rnd.Next(1, 100), unixTime)), extensionWithDot);

        }

        public static string getMD5Hash(string input)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

    }
}
