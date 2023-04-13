using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.iOS.Storage))]
namespace MineChat.iOS
{
    public class Storage : IStorage
    {
        public Storage()
        {
            
        }

        public string LoadFile(string fileName)
        {
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filename = System.IO.Path.Combine(documentsDirectory, fileName);

            if (File.Exists(filename))
            {
                string s = File.ReadAllText(filename);
                return s;
            }

            throw new Exception("File does not exist");
        }

        public void SaveFile(string fileName, string data)
        {
            try
            {
                var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string path = System.IO.Path.Combine(documentsDirectory, fileName);

                StreamWriter stream = File.CreateText(path);
                stream.Write(data);
                stream.Close();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
