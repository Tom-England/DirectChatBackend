using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network{
    class Client{

        Message create_message(String text){
            Message m = new Message();
            m.text = text;
            return m;
        }

        public void Connect(String server, String message)
        {
            Message m = create_message(message);
            MessageHandler mh = new MessageHandler();
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = Constants.PORT;
                TcpClient client = new TcpClient(server, port);

                m.destination = server;
                Console.WriteLine(server.Length);

                // Translate the passed message into ASCII and store it as a Byte array.
                //Byte[] data = System.Text.Encoding.ASCII.GetBytes(MessageHandler.message_to_string(m));
                Byte[] data = mh.get_bytes(m);
                Console.WriteLine("Sizeof data: {0}", data.Length);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
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

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            //Console.WriteLine("\n Press Enter to continue...");
            //Console.Read();
        }
    }
}