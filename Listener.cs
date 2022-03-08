using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network{
    class Listener{

        TcpListener server=null;
        
        public void create_server(){
            try {
                // Set the TcpListener on port 13000.
                Int32 port = Constants.PORT;
                //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                IPAddress localAddr = IPAddress.Parse(Constants.IP);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);
            }
            catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            
        }

        public void stop_server(){
            server.Stop();
        }

        public TcpClient get_client(){
            // Start listening for client requests.
            server.Start();
            while (true){
                TcpClient client = server.AcceptTcpClient();
                return client;
            }
        }

        public Message get_message(TcpClient client){
            

            MessageHandler mh = new MessageHandler();

            // Buffer for reading data
            Byte[] bytes = new Byte[Constants.MESSAGE_STRUCT_SIZE];
            Message data = new Message();

            // Enter the listening loop.
            while(true)
            {

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while((i = stream.Read(bytes, 0, bytes.Length))!=0)
                {
                    data = mh.from_bytes(bytes);
                    send_response(stream);
                }
                
                //Console.WriteLine(data.text);
                // Shutdown and end connection
                
                return data;
            }
        }

        public void send_response(NetworkStream stream){
            // Get a stream object for reading and writing
            
            // Send back a response.
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(Constants.ACK);
            stream.Write(msg, 0, msg.Length);
        }
    }
}