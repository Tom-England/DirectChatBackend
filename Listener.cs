using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network{
    class Listener{

        TcpListener server=null;
        
		public void create_server(string IPAddr){
            try {
                // Set the TcpListener on port 13000.
                Int32 port = Constants.PORT;
                //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                IPAddress localAddr = IPAddress.Parse(IPAddr);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);
            }
            catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            
        }


        public void create_server(){
            create_server(Constants.IP);
        }

		public void start_server(){
			server.Start();
		}

        public void stop_server(){
            server.Stop();
        }

        public bool get_client(ref List<TcpClient> clients){
            if (server.Pending()){
                clients.Add(server.AcceptTcpClient());
                return true;
            }
			return false;
        }
		public User.UserTransferable get_user(TcpClient c){
			User u = new User();

			Byte[] bytes = new Byte[Constants.USER_STRUCT_SIZE];
			User.UserTransferable ut = new User.UserTransferable();

			NetworkStream stream = c.GetStream();

			if (stream.CanRead) {
				do {
					stream.Read(bytes, 0, bytes.Length);
				} while (stream.DataAvailable);
				ut = u.from_bytes(bytes);
			}
			return ut;
		}
        public Message get_message(TcpClient client){
            
			//Console.WriteLine("reading message");

            MessageHandler mh = new MessageHandler();

            // Buffer for reading data
            Byte[] bytes = new Byte[Constants.MESSAGE_STRUCT_SIZE];
            Message data = new Message();


            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

			if (stream.CanRead){
				//Console.WriteLine("Available Bytes: {0}", client.Available);
				// Loop to receive all the data sent by the client.
				/*while((i = stream.Read(bytes, 0, bytes.Length))!=0)
				{
					data = mh.from_bytes(bytes);
					//send_response(stream);
					//stream.Flush();
					stream.Flush();
					break;
            	}*/
				do {
					stream.Read(bytes, 0, bytes.Length);
				} while (stream.DataAvailable);
				data = mh.from_bytes(bytes);
			}
            
			return data;
	    }

        public void send_response(NetworkStream stream){
            // Get a stream object for reading and writing
			//Console.WriteLine("Sending Response");
            // Send back a response.
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(Constants.ACK);
            stream.Write(msg, 0, msg.Length);
        }
    }
}