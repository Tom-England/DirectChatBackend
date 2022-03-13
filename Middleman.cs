using System.Net;
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

		public String get_ip(TcpClient c){
			IPEndPoint ep = c.Client.RemoteEndPoint as IPEndPoint;
			if (ep == null)
				return "unknown";
			return ep.Address.ToString();
		}
        public async void listen(){
            listener.create_server();
            bool running = true;
            clients.Add(listener.get_client());
			Console.WriteLine("client ip: {0}", get_ip(clients[0]));
            //clients.Add(listener.get_client());
			while (running) {
				foreach (TcpClient c in clients){
					message_stack.Add(listener.get_message(c));
				}
				for (int i = 0; i < message_stack.Count; i++){
					if (!message_stack[i].sent){
						// Check if desination client is connected then forward
						foreach (TcpClient c in clients){
							if (get_ip(c) == message_stack[i].destination) {
								// Send the message
								// Remove the message from the list
								message_stack.RemoveAt(i);
							}
						}
						Console.WriteLine(message_stack[i].text);
					}
					
				}
			}
            
            listener.stop_server();            
        }
    }
}