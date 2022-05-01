using System;

namespace Tests
{
	class Test{
		public void main(){
			string message = "Hello, World!";
			message = message.PadRight(Network.Constants.MESSAGE_SIZE - message.Length);

			cryptography.CryptoHelper c = new cryptography.CryptoHelper();
			c.new_ecdh();

			byte[] key = c.generate_shared_secret(c.public_key, c.private_key);


			Console.WriteLine("Public Key: {0}", BitConverter.ToString(c.public_key));
			Console.WriteLine("Private Key: {0}", BitConverter.ToString(c.private_key));
			Console.WriteLine("Shared Secret: {0}", BitConverter.ToString(key));

			byte[] cipher = c.encrypt(message, key, c.AES.IV);

			Console.WriteLine("Plaintext: {0}", message);
			Console.WriteLine("Ciphertext: {0}", BitConverter.ToString(cipher));
			
			string restored = c.decrypt(cipher, key, c.AES.IV);

			Console.WriteLine("Plaintext: {0}", restored);
		}
	}
}