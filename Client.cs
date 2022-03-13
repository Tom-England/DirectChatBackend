using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network{
    class Client{

        TcpClient client;
		NetworkStream stream;
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

        public void send(String message, String dest){
            bool acked = false;

            Message m = new Message(message, dest);
            MessageHandler mh = new MessageHandler();

            Byte[] data = mh.get_bytes(m);
            //NetworkStream stream = client.GetStream();
			
            while (!acked) {
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[Constants.MESSAGE_SIZE];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                if (responseData == Constants.ACK) { acked = true; }
            }
			//stream.Dispose();
        }

        public void send(Message m, TcpClient c){
            bool acked = false;

            NetworkStream stream = c.GetStream();
            MessageHandler mh = new MessageHandler();
            byte[] data = mh.get_bytes(m);

            while (!acked) {
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", m.text);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[Constants.MESSAGE_SIZE];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                if (responseData == Constants.ACK) { acked = true; }
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

        public void run_client(){
            Client c = new Client();
            c.create_client(Constants.IP);
            string msg = "";
            while (msg != "quit"){
                // Get a message
                Console.Write(">>> ");
                msg = Console.ReadLine();
				if (msg == "quit") {break;}
                List<string> msg_segments = new List<string>();
                bool split = false;
                Console.WriteLine("String length: " + msg.Length);
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
                        c.send(str, "127.0.0.1");
                        //c.Connect("90.219.214.223", str);
                        //Console.WriteLine(str);
                        //Console.WriteLine("-----------");
                    }
                } else {
                    //Client c = new Client();
                    c.send(msg, Constants.IP);
                }
            }
			//stream.Dispose();
            c.close_client();
        }
    }
}