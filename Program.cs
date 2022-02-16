using Network;
using System;
using System.Collections.Generic;

namespace Network{
    class MainClient{
        static string split_string(string str, int start, int end){
            string substring = "";
            if (end >= str.Length) {end = str.Length-1;}
            for (int i = start; i <= end; i++){
                substring += str[i];
            }
            return substring;
        }
        public static void Main(String[] args){
            if (args[0] == "client"){
                
                string msg = Console.ReadLine();
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
                Client c = new Client();
                if (split){
                    foreach(string str in msg_segments){
                        c.Connect("127.0.0.1", str);
                        Console.WriteLine(str);
                        Console.WriteLine("-----------");
                    }
                } else {
                    //Client c = new Client();
                    c.Connect("127.0.0.1", msg);
                }
                
            } else if (args[0] == "server"){
                Listener my_listener = new Listener();
                my_listener.start();
            }
        }
    }
}