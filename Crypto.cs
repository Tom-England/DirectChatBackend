using System.Security.Cryptography;

namespace cryptography{
	class CryptoHelper{
		public Aes AES = Aes.Create();

		public byte[] encrypt(string text, byte[] key, byte[] iv) {
			byte[] cipher;
			using (Aes AES_enc = Aes.Create()){
				AES_enc.IV = iv;
				AES_enc.Key = key;
				// Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = AES_enc.CreateEncryptor(AES_enc.Key, AES_enc.IV);

                // Create the streams used for encryption.
                using (MemoryStream ms_encrypt = new MemoryStream())
                {
                    using (CryptoStream cs_encrypt = new CryptoStream(ms_encrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw_encrypt = new StreamWriter(cs_encrypt))
                        {
                            //Write all data to the stream.
                            sw_encrypt.Write(text);
                        }
                        cipher = ms_encrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return cipher;
		}

		public string decrypt(byte[] cipher, byte[] key, byte[] iv){
			string text;
			// Create an Aes object
            // with the specified key and IV.
            using (Aes aes_dec = Aes.Create())
            {
                aes_dec.Key = key;
                aes_dec.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aes_dec.CreateDecryptor(aes_dec.Key, aes_dec.IV);

                // Create the streams used for decryption.
                using (MemoryStream ms_decrypt = new MemoryStream(cipher))
                {
                    using (CryptoStream cs_decrypt = new CryptoStream(ms_decrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr_decrypt = new StreamReader(cs_decrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            text = sr_decrypt.ReadToEnd();
                        }
                    }
                }
            }

            return text;
		}
	}
}