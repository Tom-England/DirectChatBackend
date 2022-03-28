using System.Net;
using System.Net.Sockets;
namespace Network{
    class Middleman{

		Client mm_client = new Client();
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
            listener.create_server("192.168.1.191");
            bool running = true;
            clients.Add(listener.get_client());
			//clients.Add(listener.get_client());
			Console.WriteLine("client ip: {0}", get_ip(clients[0]));
			//Console.WriteLine("client ip: {0}", get_ip(clients[1]));
            //clients.Add(listener.get_client());
			Message m;
			while (running) {
				List<int> dead_client_indexes = new List<int>(); // Dead Client Indexes could be a good band name?
				for (int i = 0; i < clients.Count; i++){
					message_stack.AddLast(listener.get_message(clients[i]));
					Console.WriteLine("Added {0} to stack", message_stack.Last.Value.text);
					if (!clients[i].Connected) {
						clients[i].Close();
						dead_client_indexes.Add(i);
					}
				}
				foreach(int i in dead_client_indexes){
					List<TcpClient> new_client_list = new List<TcpClient>();
					for (int j = 0; j < clients.Count; j++){
						if (j != i) { new_client_list.Add(clients[j]); }
					}
					clients = new_client_list;
				}
				LinkedListNode<Message> node=message_stack.First;
				while(node != null){
					m = node.Value;
					LinkedListNode<Message> next = node.Next;
					if (!m.sent){
						// Check if desination client is connected then forward
						foreach (TcpClient c in clients){
							if (get_ip(c) == m.destination) {
								// Send the message
								// Remove the message from the list
								Console.WriteLine("Removing message {0} going to IP {1}", m.text, m.destination);
								mm_client.send(m, c);
								m.sent = true;
								message_stack.Remove(node);

							}
						}
						//Console.WriteLine(m.text);
					}
					node = next;
				}
			}
            
            listener.stop_server();            
        }
    }
}