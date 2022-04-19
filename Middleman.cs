using System.Net;
using System.Net.Sockets;
namespace Network{
    class Middleman{

		Client mm_client = new Client();
        List<TcpClient> clients = new List<TcpClient>();
        Listener listener;
        LinkedList<Message> message_stack = new LinkedList<Message>();
		User u = new User("DNS");
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

		void handle_client(Message m, TcpClient c){
			switch (m.status){
				case Status.message:
					Console.WriteLine("Message");
					Console.WriteLine("Recieved: {0}", m.text);
					message_stack.AddLast(m);
					mm_client.send_status(Status.ack, c, u.Id, false);
					break;
				case Status.send:
					Console.WriteLine("Send");
					mm_client.send_status(Status.ack, c, u.Id, false);
					break;
				case Status.recieve:
					Console.WriteLine("Recieve");
					mm_client.send_status(Status.ack, c, u.Id, false);

					LinkedListNode<Message> node=message_stack.First;
					Message message;
					while(node != null){
						message = node.Value;
						LinkedListNode<Message> next = node.Next;
						if (get_ip(c) == message.destination) {
							// Message is to be sent
							mm_client.send(message, c);
							message_stack.Remove(node);
						}
						node = next;
					}
					mm_client.send_status(Status.done, c, u.Id);
					Console.WriteLine("Done Recieve");
					break;
			}
		}
        public void listen(){
            listener.create_server(Constants.IP);
            bool running = true;
            //clients.Add(listener.get_client());
			//clients.Add(listener.get_client());
			//Console.WriteLine("client ip: {0}", get_ip(clients[0]));
			//Console.WriteLine("client ip: {0}", get_ip(clients[1]));
            //clients.Add(listener.get_client());
			Message m;
			listener.start_server();
			while (running) {
				listener.get_client(ref clients);
				List<int> dead_client_indexes = new List<int>(); // Dead Client Indexes could be a good band name?
				for (int i = 0; i < clients.Count; i++){
					if (clients[i].Connected){
						m = listener.get_message(clients[i]);
						handle_client(m, clients[i]);
					} else {
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
            listener.stop_server();            
        }
    }
}
