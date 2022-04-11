using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Network{
    class Client{

        Storage.DatabaseHandler dbh = new Storage.DatabaseHandler();
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

        public void send(String message, String dest, Guid _id){
            bool acked = false;

            Message m = new Message(message, dest, _id);
            MessageHandler mh = new MessageHandler();

            Byte[] data = mh.get_bytes(m);
            //NetworkStream stream = client.GetStream();
			Console.WriteLine("Sending {0} bytes", data.Length);	
            while (!acked) {
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", message);

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
			while (!acked) {
				NetworkStream str = c.GetStream();
				str.Write(data, 0, data.Length);
				Console.WriteLine("Sent: {0}", m.status);

				data = new Byte[Constants.MESSAGE_STRUCT_SIZE];
				Message responseData;

				Int32 bytes = str.Read(data, 0, data.Length);
                responseData = mh.from_bytes(data);
                Console.WriteLine("Received: {0}", responseData.status);
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

			while (!acked) {
				stream.Write(data, 0, data.Length);
				Console.WriteLine("Sent Status: {0}", m.status);

				if (!ack_needed) { acked = true; break; }
				//Console.WriteLine("Waiting for ACK");
				
				//data = new Byte[Constants.MESSAGE_STRUCT_SIZE];
				Message responseData = read_message_from_stream(stream);

				//Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = mh.from_bytes(data);
                Console.WriteLine("Received: {0}", responseData.status);
                if (responseData.status == Status.ack) { acked = true; }
				Thread.Sleep(100);
			}
		}

        public void send(Message m, TcpClient c){
            bool acked = false;

            NetworkStream stream = c.GetStream();
            MessageHandler mh = new MessageHandler();
            byte[] data = mh.get_bytes(m);

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
			//Console.WriteLine("Checking for messages");
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
				data = read_message_from_stream(c);
				if (data.created && data.status == Status.message) {
					//Console.WriteLine("Recieved: {0}", data.text);
					dbh.add_message(data.text, data.sender_id);
				}
				
			}
			// 4. Exit
			//Console.WriteLine("Done");
			send_status(Status.ack, c.client, id, false);

		}
		public void check_messages_old(Client c){
			// Check for messages
			List<Message> messages = new List<Message>();
			NetworkStream stream;
			MessageHandler mh = new MessageHandler();
			int i;

			// Check for new messages
			Byte[] bytes = new Byte[Network.Constants.MESSAGE_STRUCT_SIZE];
        	Network.Message data = new Network.Message();
			stream = c.get_stream();
			if (stream.DataAvailable){
				while((i = stream.Read(bytes, 0, bytes.Length))!=0 && stream.DataAvailable)
				{
					//Console.WriteLine("Doing something or other");
					data = mh.from_bytes(bytes);
					Console.WriteLine("Recieved: {0}", data.text);
					//stream.Flush();
					//return data;
				}
			} else {
				Console.WriteLine("Nothing to read");
				
			}
		}
        public void run_client(){
            User u = new User("user");
			Client c = new Client();
            c.create_client(Constants.IP);
            string msg = "";
            //dbh.create();
            dbh.connect();
            //dbh.setup();
            dbh.add_user(u.Name, u.Id);
			while (msg != "quit"){

				check_messages(c, u.Id);

                dbh.get_all_messages_from_user(u.Id);

                // Get a message
                Console.Write(">>> ");
                msg = Console.ReadLine();
				if (msg == "quit") {break;}
                List<string> msg_segments = new List<string>();
                bool split = false;
                //Console.WriteLine("String length: " + msg.Length);

				send_status(Status.send, c.client, u.Id, true);
				//Console.WriteLine("Send send status");
				//Message data = read_message_from_stream(c);
				//Console.WriteLine(data.status);
				//if (data.status == Status.ack) {
					if (msg.Length > Constants.MESSAGE_SIZE){
						split = true;
						int start = 0;
						for (int i = 0; i <= msg.Length / Constants.MESSAGE_SIZE; i++){
							msg_segments.Add(split_string(msg, start, start + Constants.MESSAGE_SIZE));
							start += 161;
						}
					}
					
					if (split){
						foreach(string str in msg_segments){
							c.send(str, Constants.IP, u.Id);
						}
					} else {
						c.send(msg, Constants.IP, u.Id);
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
