using System.Net.Sockets;
namespace Network{
    class Middleman{

        List<TcpClient> clients = new List<TcpClient>();
        Listener listener;
        List<Message> message_stack = new List<Message>();
        public Middleman(){
            listener = new Listener();
        }

        public void test_msg(String test){
            
        }

        public void listen(){
            listener.create_server();
            //bool running = true;
            clients.Add(listener.get_client());
            clients.Add(listener.get_client());

            foreach (TcpClient c in clients){
                message_stack.Add(listener.get_message(c));
                c.Close();
            }
            
            
            listener.stop_server();

            foreach (Message m in message_stack){
                Console.WriteLine(m.text);
            }
        }
    }
}