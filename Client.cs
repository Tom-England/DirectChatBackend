using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Network{

	interface IClient{
		void create_client(String server);
		void close_client();
		void send(byte[] message, String dest, Guid _id);
		void send(Message m, TcpClient c);
		void send_status(Status s, String dest, TcpClient c, Guid id);
		void send_status(Status s, TcpClient c, Guid id, bool ack_needed = true);
		Message read_message_from_stream(Client c);
		Message read_message_from_stream(NetworkStream s);
		void check_messages(Client c, Guid id);
		void run_client();
	}
    public class Client:IClient{

        public Storage.DatabaseHandler dbh = new Storage.DatabaseHandler();
        TcpClient client;
		NetworkStream stream;
		bool send_ready = false;
		Status status;
        public void create_client(String server){
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = Constants.PORT;
                client = new TcpClient(server, port);
				stream = client.GetStream();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public void close_client(){
            client.Close();
        }

		public void send_user(User.UserTransferable u, User usr, TcpClient c){
			bool acked = false;
			MessageHandler mh = new MessageHandler();
			byte[] u_data = usr.get_bytes(u);
			Console.WriteLine("Sending {0} bytes", u_data.Length);
			NetworkStream stream = c.GetStream();
			while (!acked) {
				stream.Write(u_data, 0, u_data.Length);
				Console.WriteLine("Sent: {0}", u.name);
				Byte[] data = new Byte[Constants.MESSAGE_STRUCT_SIZE];
				stream.Read(data, 0, data.Length);
				Message responseData = mh.from_bytes(data);
				if (responseData.status == Status.ack) {acked = true;} else {Console.WriteLine("No Ack");}
				Thread.Sleep(100);
			}
		}
        public void send(byte[] message, String dest, Guid _id){
            bool acked = false;

            Message m = new Message(message, dest, _id);
            MessageHandler mh = new MessageHandler();

            Byte[] data = mh.get_bytes(m);
            //NetworkStream stream = client.GetStream();
			Console.WriteLine("Sending {0} bytes", data.Length);	
            while (!acked) {
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0} To: {1}", message, dest);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[Constants.MESSAGE_SIZE];

                // String to store the response ASCII representation.
                String responseData = String.Empty;
				/**
                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                if (responseData == Constants.ACK) { acked = true; }*/
				acked = true;
            }
			//stream.Dispose();
        }

		public void send_status(Status s, String dest, TcpClient c, Guid id){
			bool acked = false;
			Message m = new Message(dest, s, id);
			MessageHandler mh = new MessageHandler();
			Byte[] data = mh.get_bytes(m);
			Console.WriteLine("Sending {0} bytes", data.Length);
			Console.WriteLine("Trying to send {0}", m.status);
			while (!acked) {
				NetworkStream str = c.GetStream();
				str.Write(data, 0, data.Length);
				Console.WriteLine("Sent: {0}", m.status);

				data = new Byte[Constants.MESSAGE_STRUCT_SIZE];
				Message responseData;

				Int32 bytes = str.Read(data, 0, data.Length);
                responseData = mh.from_bytes(data);
                //Console.WriteLine("Received: {0}", responseData.status);
                if (responseData.status == Status.ack) { acked = true; }
				Thread.Sleep(100);
			}
		}

		public void send_status(Status s, TcpClient c, Guid id, bool ack_needed = true ){
			NetworkStream stream = c.GetStream();
			bool acked = false;
			Message m = new Message("-1", s, id);
			MessageHandler mh = new MessageHandler();
			Byte[] data = mh.get_bytes(m);
			Console.WriteLine("Sending {0} bytes", data.Length);
			Console.WriteLine("Trying to send {0}", m.status);
			while (!acked) {
				stream.Write(data, 0, data.Length);
				Console.WriteLine("Sent Status: {0}", m.status);

				if (!ack_needed) { acked = true; break; }
				//Console.WriteLine("Waiting for ACK");
				
				//data = new Byte[Constants.MESSAGE_STRUCT_SIZE];
				Message responseData = read_message_from_stream(stream);

				//Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = mh.from_bytes(data);
                //Console.WriteLine("Received: {0}", responseData.status);
                if (responseData.status == Status.ack) { acked = true; }
				Thread.Sleep(100);
			}
		}

        public void send(Message m, TcpClient c){
            bool acked = false;

            NetworkStream stream = c.GetStream();
            MessageHandler mh = new MessageHandler();
            byte[] data = mh.get_bytes(m);

			Console.WriteLine("Sending {0} bytes", data.Length);
            while (!acked) {
                stream.Write(data, 0, data.Length);
                //Console.WriteLine("Sent: {0}", m.text);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[Constants.MESSAGE_SIZE];

                // String to store the response ASCII 
				// representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
				/**
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                if (responseData == Constants.ACK) { acked = true; }*/
				acked = true;
            }
        }

        static string split_string(string str, int start, int end){
            string substring = "";
            if (end >= str.Length) {end = str.Length-1;}
            for (int i = start; i <= end; i++){
                substring += str[i];
            }
            return substring;
        }

		public Message read_message_from_stream(Client c){
			return read_message_from_stream(c.get_stream());
		}

		public Message read_message_from_stream(NetworkStream s){
			Byte[] bytes = new Byte[Network.Constants.MESSAGE_STRUCT_SIZE];
			Message data = new Message();
			MessageHandler mh = new MessageHandler();
			int i;
			//Console.WriteLine("Going to get stream");
			//Console.WriteLine("Starting Read");
			///Console.WriteLine(s);
			if (s.DataAvailable){
				while((i = s.Read(bytes, 0, bytes.Length))!=0)
				{
					//Console.WriteLine("Doing something or other");
					data = mh.from_bytes(bytes);
					break;
				}
			}
			
			return data;
		}

		public void check_messages(Client c, Guid id){

			LinkedList<Message> received_stack = new LinkedList<Message>();

			Console.WriteLine("Checking for messages");
			// 1. Send message to mm asking for messages
			send_status(Status.recieve, Constants.IP, c.client, id);
			//Console.WriteLine("Status Sent");
			// 2. Await ACK
			//Console.WriteLine("Checking for ACK");
			Message data = new Message();
			data = read_message_from_stream(c);
			//Console.WriteLine("Recieved: {0}", data.status);
			//if (data.status != Status.ack){ return; }

			// 3. Read messages until recieve DONE
			while (data.status != Status.done) {
				Console.WriteLine("Received {0}", data.status);
				if (data.created && data.status == Status.message) {
					received_stack.AddLast(data);
				}
				data = read_message_from_stream(c);
			}
			// 4. Exit
			//Console.WriteLine("Done");
			send_status(Status.ack, c.client, id, false);

			// Handle Users
			foreach (Message message in received_stack){
				if (!dbh.user_exists(message.sender_id)){
						Console.WriteLine("Requesting: {0}", message.sender_id);
						User.UserTransferable user_info = request_user(message.sender_id, c.client);
						Console.WriteLine("{0} {1}", user_info.id, user_info.name);
						dbh.add_user(user_info.name, message.sender_id, user_info.key);
					}
				string data_text = Convert.ToBase64String(message.text);
				dbh.add_message(data_text, message.sender_id);
			}
		}

		public void setup_id(User u, cryptography.CryptoHelper c){
			Guid temp_id = dbh.get_account_id();	
			if (temp_id != Guid.Empty) {
				c.AES.IV = dbh.get_iv();
				c.bytes = dbh.get_key();
				c.generate_keys_from_bytes();
				u.Id = temp_id;
			} else {
				c.new_ecdh();
				dbh.register(u.Id, c.bytes, c.AES.IV);
			}
			Console.WriteLine("ID: {0}", u.Id);
		}

		public void handshake(TcpClient c, User u, cryptography.CryptoHelper crypto){
			send_status(Status.handshake, c, u.Id);
			User.UserTransferable user = new User.UserTransferable(u.Name, u.Id, crypto.public_key, crypto.AES.IV);
			user.created = true;
			send_user(user, u, c);
			Console.WriteLine("Handshake Done");
		}

		public User.UserTransferable request_user(Guid target, TcpClient c){
			User.UserTransferable details;
			Message m = new Message("0.0.0.0", Status.request, target);
			send(m, c);
			Listener l = new Listener();
			do{
				details = l.get_user(c);
			}while (!details.created);
			send_status(Status.ack, c, target, false);
			//Console.WriteLine(BitConverter.ToString(details.key));
			return details;
		}

		public void setup_client() {
			dbh.connect();
			dbh.setup();
		}

        public void run_client(){
            User u = new User("user");
			Client c = new Client();

			cryptography.CryptoHelper crypto = new cryptography.CryptoHelper();

			dbh.connect();
			dbh.setup();
			setup_id(u, crypto);

            c.create_client(Constants.IP);
			handshake(c.client, u, crypto);

            string msg = "";
			Console.Write("Target >>> ");
			string target = Console.ReadLine();
			Console.Write("Target Guid >>> ");
			Guid guid = Guid.Parse(Console.ReadLine());
			Console.WriteLine();

			User.UserTransferable uT = request_user(guid, c.client);
			//crypto.AES.Key = crypto.create_shared_secret(uT.key);

			crypto.print_keys();
			
			Console.WriteLine("uT Pk: {0}", BitConverter.ToString(uT.key));

			byte[] key = crypto.generate_shared_secret(crypto.private_key, uT.key);

			Console.WriteLine("Shared Secret: {0}", BitConverter.ToString(key));

			while (msg != "quit"){

				check_messages(c, u.Id);

                dbh.print_all_messages(key, crypto.AES.IV);
				
                // Get a message
                Console.Write(">>> ");
                msg = Console.ReadLine();
				if (msg == "quit") {break;}
                List<string> msg_segments = new List<string>();
                bool split = false;
                //Console.WriteLine("String length: " + msg.Length);

				//send_status(Status.send, c.client, u.Id, true);
				//Console.WriteLine("Send send status");
				//Message data = read_message_from_stream(c);
				//Console.WriteLine(data.status);
				//if (data.status == Status.ack) {
					if (msg.Length > Constants.MESSAGE_SIZE){
						split = true;
						int start = 0;
						for (int i = 0; i <= msg.Length / Constants.MESSAGE_SIZE; i++){
							msg_segments.Add(split_string(msg, start, start + Constants.MESSAGE_SIZE));
							start += Constants.MESSAGE_SIZE + 1;
						}
					}
					
					if (split){
						foreach(string str in msg_segments){
							byte[] enc_str = crypto.encrypt(str.PadRight(Constants.MESSAGE_SIZE), key, uT.iv);
							c.send(enc_str, target, u.Id);
						}
					} else {
						byte[] enc_str = crypto.encrypt(msg.PadRight(Constants.MESSAGE_SIZE), key, uT.iv);
						Console.WriteLine("Length of msg: {0}", enc_str.Length);
						c.send(enc_str, target, u.Id);
					}
				//}
				//else { break; }
            }
            c.close_client();
            dbh.disconnect();
        }

		public NetworkStream get_stream(){
			return client.GetStream();
		}

		public Status get_status(){
			return status;
		}

		void set_status(Status _status){
			status = _status;
		}

		public bool get_send_ready(){
			return send_ready;
		}
    }
}
