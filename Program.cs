using Network;
using System;

namespace Network{
    class MainClient{
        public static void Main(String[] args){
            Console.WriteLine("Hello, World {0}", args.Length);
            if (args[0] == "client"){
                Client c = new Client();
                c.StartClient();
            } else if (args[0] == "server"){
                Listener my_listener = new Listener();
                my_listener.start_server();
            }
        }
    }
}