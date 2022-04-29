using System.Security.Cryptography;
using Elliptic;
using System.Text;

namespace cryptography{
	class CryptoHelper{
		
		public byte[] bytes;
		public byte[] private_key;
		public byte[] public_key;
		public Aes AES = Aes.Create();

		public void new_ecdh(){
			bytes = new byte[32];
			RandomNumberGenerator.Create().GetBytes(bytes);
			generate_keys_from_bytes();
		}

		public void generate_keys_from_bytes(){
			private_key = Curve25519.ClampPrivateKey(bytes);
			public_key = Curve25519.GetPublicKey(private_key);
		}

		public byte[] generate_shared_secret(byte[] alice_key, byte[] bob_key){
			return Curve25519.GetSharedSecret(alice_key, bob_key);
		}

		public void print_keys(){
			Console.WriteLine("Private: {0}\nPublic: {1}", BitConverter.ToString(private_key), BitConverter.ToString(public_key));
		}

		/***
		*** Takes in the public key of bob and returns the shared secret
		***/
		public byte[] get_shared_secret(byte[] bob_key){
			byte[] shared_secret = new byte[32];
			return shared_secret;
		}


		public byte[] encrypt(string text, byte[] key, byte[] iv) {
			Console.WriteLine("Starting Encrypt");
			Console.WriteLine("Key: {0}\nIV: {1}", BitConverter.ToString(key), BitConverter.ToString(iv));
			using (Aes AES_enc = Aes.Create()){
				AES_enc.IV = iv;
				AES_enc.Key = key;
				AES_enc.Padding = PaddingMode.PKCS7;
				/*
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
                }*/
				using (var encryptor = AES_enc.CreateEncryptor(key, iv))
				{
					var plainText = Encoding.UTF8.GetBytes(text);
					var cipherText = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
					Console.WriteLine("Done");
					Console.WriteLine("Length {0} bytes", cipherText.Length);
					return cipherText;
				}
            }
		}

		public string decrypt(byte[] cipher, byte[] key, byte[] iv){
			Console.WriteLine("Trying to decrypt message of length {0}", cipher.Length);
			Console.WriteLine("Key: {0}\nIV: {1}", BitConverter.ToString(key), BitConverter.ToString(iv));
			string text;
			// Create an Aes object
            // with the specified key and IV.
            using (Aes aes_dec = Aes.Create())
            {
                aes_dec.Key = key;
                aes_dec.IV = iv;
				aes_dec.Padding = PaddingMode.PKCS7;
				/*
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
                }*/

				using (var encryptor = aes_dec.CreateDecryptor(key, iv))
				{
					var decryptedBytes = encryptor.TransformFinalBlock(cipher, 0, cipher.Length);
					text = Encoding.UTF8.GetString(decryptedBytes);
				}
            }

            return text;
		}
	}
}