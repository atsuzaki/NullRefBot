using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DSharpPlus.Entities;

namespace NullRefBot.Utils
{
	public static class EncryptionUtils
	{
		private const string INIT_VECTOR = "0join1null29ref6";
		private const int KEYSIZE = 256;

		public static string EncryptString(string plainText, string passPhrase)
		{
			byte[] initVectorBytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
			byte[] keyBytes = password.GetBytes(KEYSIZE / 8);
			RijndaelManaged symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
			ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
			cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
			cryptoStream.FlushFinalBlock();
			byte[] cipherTextBytes = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return Convert.ToBase64String(cipherTextBytes);
		}

		public static string DecryptString(string cipherText, string passPhrase)
		{
			byte[] initVectorBytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
			byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
			PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
			byte[] keyBytes = password.GetBytes(KEYSIZE / 8);
			RijndaelManaged symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
			ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
			MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
			byte[] plainTextBytes = new byte[cipherTextBytes.Length];
			int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
			memoryStream.Close();
			cryptoStream.Close();
			return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
		}

		public static string EncryptString(string txt) => EncryptString(txt, Bot.Instance.Config.EncryptionKey);
		public static string DecryptString(string txt) => DecryptString(txt, Bot.Instance.Config.EncryptionKey);

		public static string GetEncryptedID(this SnowflakeObject snowflake)
		{
			string idString = snowflake.Id.ToString();
			return EncryptString(idString, Bot.Instance.Config.EncryptionKey);
		}
	}
}