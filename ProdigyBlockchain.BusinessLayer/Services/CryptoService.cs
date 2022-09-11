using Prodigy.BusinessLayer.Wallet;
using System;
using System.Security.Cryptography;
using System.Text;
using Crypto.RIPEMD;

namespace Prodigy.BusinessLayer.Services
{
    public interface ICryptoService
    {
        byte[] GenerateHash(byte[] hashingData);
        KeyPair CreateWalletKeyPair(string username, string password);
        bool DecryptSuccessful(string address, string privateKey, string username, string passphrase);
        string EncryptData(string Data, string publicKey);
        string DecryptData(string data, string publicKey, bool isBase = true);
    }

    public class CryptoService : ICryptoService
	{
		private string _PrivateKey = "EMPTY";

		private IWalletSettings _Settings;

		public CryptoService(string node_private_key, IWalletSettings settings)
		{
			_PrivateKey = node_private_key;
			_Settings = settings;
			_PrivateKey = settings.PrivateKey;
		}

		public CryptoService(string node_private_key)
        {
			_PrivateKey = node_private_key;
		}

		public KeyPair CreateWalletKeyPair(string username, string password)
		{
			KeyPair pair = new KeyPair();

			RijndaelManaged objrij = new RijndaelManaged();

			objrij.Mode = CipherMode.CBC;
			objrij.Padding = PaddingMode.PKCS7;
			objrij.KeySize = 0x80;
			objrij.BlockSize = 0x80;

			objrij.GenerateKey();

			pair.priv = Convert.ToBase64String(objrij.Key);
			pair.pub = _Settings.Prefix + "0" + HashKeyPair(pair.priv, username, password);

			return pair;
		}

		private string HashKeyPair(string privateKey, string username, string password)
		{
			RIPEMD160 r160 = new RIPEMD160Managed();

			byte[] myByte = System.Text.Encoding.ASCII.GetBytes(privateKey + "-:-" + username + "-:-" + password);
			// compute the byte to RIPEMD160 hash
			byte[] encrypted = r160.ComputeHash(myByte);
			// create a new StringBuilder process the hash byte
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < encrypted.Length; i++)
			{
				sb.Append(encrypted[i].ToString("X2"));
			}

			return sb.ToString();
		}

		public byte[] GenerateHash(byte[] hashingData)
		{
			using (SHA256 sha256 = new SHA256Managed())
			{
				using (SHA512 sha512 = new SHA512Managed())
				{
					return sha512.ComputeHash(sha256.ComputeHash(hashingData));
				}
			}
		}

		/// <summary>
		/// Hashes a string
		/// </summary>
		/// <param name="stringForHashing">Inoput string</param>
		/// <returns>Hashed string</returns>
		public static string CalculateHash(string stringForHashing)
		{
			SHA256Managed crypt = new SHA256Managed();
			string hash = String.Empty;
			byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(stringForHashing), 0, Encoding.ASCII.GetByteCount(stringForHashing));
			foreach (byte theByte in crypto)
			{
				hash += theByte.ToString("x2");
			}
			return hash;
		}

		/// <summary>
		/// Encypts text data
		/// </summary>
		/// <param name="Data">Text data</param>
		/// <param name="publicKey">Public key that will be used to encrypt the data</param>
		/// <returns>Encrypted string data</returns>
		public string EncryptData(string Data, string publicKey)
		{
			RijndaelManaged objrij = new RijndaelManaged();

			objrij.Mode = CipherMode.CBC;
			objrij.Padding = PaddingMode.PKCS7;
			objrij.KeySize = 0x80;
			objrij.BlockSize = 0x80;

			byte[] passBytes = Encoding.UTF8.GetBytes(_PrivateKey + "-:!:-" + publicKey);

			byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			int len = passBytes.Length;
			if (len > EncryptionkeyBytes.Length)
			{
				len = EncryptionkeyBytes.Length;
			}
			Array.Copy(passBytes, EncryptionkeyBytes, len);
			objrij.Key = EncryptionkeyBytes;
			objrij.IV = EncryptionkeyBytes;

			ICryptoTransform objtransform = objrij.CreateEncryptor();
			byte[] textDataByte = Encoding.UTF8.GetBytes(Data);

			return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
		}

		/// <summary>
		/// Decrypts binary data
		/// </summary>
		/// <param name="data">Byte array of encrypted data</param>
		/// <param name="publicKey">Public key that will be used to decrypt the data</param>
		/// <returns>Unencrypted text data</returns>
		public string DecryptData(byte[] data, string publicKey)
		{
			RijndaelManaged objrij = new RijndaelManaged();

			objrij.Mode = CipherMode.CBC;
			objrij.Padding = PaddingMode.PKCS7;
			objrij.KeySize = 0x80;
			objrij.BlockSize = 0x80;

			byte[] encryptedTextByte = data;
			byte[] passBytes = Encoding.UTF8.GetBytes(_PrivateKey + "-:!:-" + publicKey);
			byte[] encryptionkeyBytes = new byte[0x10];
			int len = passBytes.Length;
			if (len > encryptionkeyBytes.Length)
			{
				len = encryptionkeyBytes.Length;
			}
			Array.Copy(passBytes, encryptionkeyBytes, len);
			objrij.Key = encryptionkeyBytes;
			objrij.IV = encryptionkeyBytes;
			byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);

			return Encoding.UTF8.GetString(TextByte);
		}
		
		/// <summary>
		/// Decrypts text data
		/// </summary>
		/// <param name="data">String of encrypted data</param>
		/// <param name="publicKey">Public key that will be used to decrypt the data</param>
		/// <returns>Unencrypted text data</returns>
		public string DecryptData(string data, string publicKey, bool isBase = true)
		{
			RijndaelManaged objrij = new RijndaelManaged();

			objrij.Mode = CipherMode.CBC;
			objrij.Padding = PaddingMode.PKCS7;
			objrij.KeySize = 0x80;
			objrij.BlockSize = 0x80;

			byte[] encryptedTextByte;
			
			if(isBase)
				encryptedTextByte = Convert.FromBase64String(data);
			else
				encryptedTextByte = Encoding.UTF8.GetBytes(data);

			byte[] passBytes = Encoding.UTF8.GetBytes(_PrivateKey + "-:!:-" + publicKey);
			byte[] encryptionkeyBytes = new byte[0x10];
			int len = passBytes.Length;
			if (len > encryptionkeyBytes.Length)
			{
				len = encryptionkeyBytes.Length;
			}
			Array.Copy(passBytes, encryptionkeyBytes, len);
			objrij.Key = encryptionkeyBytes;
			objrij.IV = encryptionkeyBytes;
			byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);

			return Encoding.UTF8.GetString(TextByte);
		}


		public bool DecryptSuccessful(string address, string privateKey, string username, string passphrase)
		{
			string cleaned_address = address.Replace(_Settings.Prefix + "0", "");

			string hashedString = HashKeyPair(privateKey, username, passphrase);

			if (cleaned_address == hashedString)
			{
				return true;
			}

			return false;
		}

	}
}
