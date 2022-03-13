using System.Net;
using System.Net.Sockets;
namespace Network{
    class Middleman{

        List<TcpClient> clients = new List<TcpClient>();
        Listener listener;
        LinkedList<Message> message_stack = new LinkedList<Message>();
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
        public void listen(){
            listener.create_server();
            bool running = true;
            clients.Add(listener.get_client());
			Console.WriteLine("client ip: {0}", get_ip(clients[0]));
            //clients.Add(listener.get_client());
			Message m;
			while (running) {
				foreach (TcpClient c in clients){
					message_stack.AddLast(listener.get_message(c));
					Console.WriteLine("Added {0} to stack", message_stack.Last.Value.text);
				}
				for(LinkedListNode<Message> node=message_stack.First; node != null; node=node.Next){
					m = node.Value;
					if (!m.sent){
						// Check if desination client is connected then forward
						foreach (TcpClient c in clients){
							if (get_ip(c) == m.destination) {
								// Send the message
								// Remove the message from the list
								Console.WriteLine("Removing message {0} going to IP {1}", m.text, m.destination);
								m.sent = true;
							}
						}
						Console.WriteLine(m.text);
					}
				}
			}
            
            listener.stop_server();            
        }
    }
}