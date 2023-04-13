using System;
using System.Text; 
using System.Security.Cryptography;
using System.IO;

namespace MineFriendsForms.Droid
{
	public class SimpleAES
	{
		private static byte[] key = //redacted
		private static byte[] vector = // redacted

		private ICryptoTransform encryptor, decryptor;
		private UTF8Encoding encoder;
		
		public SimpleAES()
		{
			RijndaelManaged rm = new RijndaelManaged();
			encryptor = rm.CreateEncryptor(key, vector);
			decryptor = rm.CreateDecryptor(key, vector);
			encoder = new UTF8Encoding();
		}
		
		public string Encrypt(string unencrypted)
		{
			return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
		}
		
		public string Decrypt(string encrypted)
		{
			return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
		}
		
		public byte[] Encrypt(byte[] buffer)
		{
			return Transform(buffer, encryptor);
		}
		
		public byte[] Decrypt(byte[] buffer)
		{
			return Transform(buffer, decryptor);
		}
		
		protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
		{
			MemoryStream stream = new MemoryStream();
			using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
			{
				cs.Write(buffer, 0, buffer.Length);
			}
			return stream.ToArray();
		}
	}
}

