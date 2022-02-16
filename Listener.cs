using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network{
    class Listener{
        public void start_server(){
            // Get host IP address
            // (localhost here)
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11000);
        
            try {
                Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);

                listener.Listen(10);

                Console.WriteLine("Awaiting Connection");

                Socket handler = listener.Accept();

                string data = null;
                byte[] bytes = null;

                while(true){
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1){
                        break;
                    }
                }

                Console.WriteLine("Text Recieved: {0}", data);

                byte[] msg = Encoding.ASCII.GetBytes(data);
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e){
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key...");
            Console.ReadKey();
        }
    }
}