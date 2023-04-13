using System;
using System.IO;
using System.Threading.Tasks;
using MineChat;

[assembly: Xamarin.Forms.Dependency (typeof ( MineFriendsForms.Droid.Storage))]
namespace MineFriendsForms.Droid
{
    public class Storage : IStorage
    {
        public Storage()
        {
        }

        public string LoadFile(string fileName)
        {
            
            string decyptedFile = string.Empty;

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, fileName);

            StreamReader stream = File.OpenText(filePath);
            string encryptedSettings = stream.ReadToEnd();
            stream.Close();

            if (!encryptedSettings.Contains("xml version"))
            {
                SimpleAES crypto = new SimpleAES();
                decyptedFile = crypto.Decrypt(encryptedSettings);
            }

            return decyptedFile;
        }

        public void SaveFile(string fileName, string data)
        {
            SimpleAES crypto = new SimpleAES();
            string encryptedSettings = crypto.Encrypt(data);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, fileName);            

            FileStream fs = File.Create(filePath);

            fs.Write(System.Text.Encoding.UTF8.GetBytes(encryptedSettings), 0, encryptedSettings.Length);
            fs.Close();
        }
    }
}
