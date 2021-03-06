using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Network{
    class Middleman{

		Client mm_client = new Client();
        List<TcpClient> clients = new List<TcpClient>();
        Listener listener;
        LinkedList<Message> message_stack = new LinkedList<Message>();
		LinkedList<User.UserTransferable> user_stack = new LinkedList<User.UserTransferable>();
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

		void handle_client(Object c_obj){
			TcpClient c = (TcpClient) c_obj;
			Guid id = Guid.Empty;
			while (true) {
                try
                {
					Message m = listener.get_message(c);
					switch (m.status)
					{
						case Status.request:
							Console.WriteLine("request for {0}", m.sender_id);
							User.UserTransferable uT = new User.UserTransferable();
							foreach (User.UserTransferable userT in user_stack)
							{
								if (userT.id == m.sender_id)
								{
									uT = userT;
								}
							}
							mm_client.send_user(uT, u, c);
							break;
						case Status.handshake:
							mm_client.send_status(Status.ack, c, u.Id, false);
							User.UserTransferable usr = listener.get_user(c);
							while (!usr.created)
							{
								usr = listener.get_user(c);
							}
							user_stack.AddLast(usr);
							Console.WriteLine("Hello {0}: {1}", usr.name, usr.id);
							mm_client.send_status(Status.ack, c, u.Id, false);
							id = usr.id;
							break;
						case Status.message:
							//Console.WriteLine("Message");
							Console.WriteLine("Recieved: {0}", m.text);
							message_stack.AddLast(m);
							mm_client.send_status(Status.ack, c, u.Id, false);
							break;
						case Status.send:
							//Console.WriteLine("Send");
							mm_client.send_status(Status.ack, c, u.Id, false);
							break;
						case Status.recieve:
							Console.WriteLine("{0} {1} Recieve", DateTimeOffset.Now.ToUnixTimeSeconds(), id);
							mm_client.send_status(Status.ack, c, u.Id, false);

							LinkedListNode<Message> node = message_stack.First;
							Message message;
							Console.WriteLine("{0} nodes", message_stack.Count);
							while (node != null)
							{
								
								message = node.Value;
								LinkedListNode<Message> next = node.Next;
								if (id == message.destination)
								{
									// Message is to be sent
									Console.WriteLine("Sending Message with status {0}", message.status);
									Console.WriteLine("c: {0}", message.sender_id);
									mm_client.send(message, c);
									message_stack.Remove(node);
								}
								else
								{
									Console.WriteLine("{0} != {1}", id, message.destination.ToString());
								}
								node = next;
							}
							mm_client.send_status(Status.done, c, u.Id);
							Console.WriteLine("Done Recieve");
							break;
					}
					Thread.Sleep(0);
				} catch (System.IO.IOException e)
                {
					Console.WriteLine("Client Write Failed, closing client...");
					break;
                }
			}
			
		}
        public void listen(){
			Console.WriteLine("ID: {0}", u.Id);
            listener.create_server(Constants.IP);
            bool running = true;
            //clients.Add(listener.get_client());
			//clients.Add(listener.get_client());
			//Console.WriteLine("client ip: {0}", get_ip(clients[0]));
			//Console.WriteLine("client ip: {0}", get_ip(clients[1]));
            //clients.Add(listener.get_client());
			listener.start_server();
			List<Thread> threads = new List<Thread>();
			while (running) {
				if (listener.get_client(ref clients)){
					// Start thread
					threads.Add(new Thread(handle_client));
					threads.Last().Start(clients.Last());
					//Console.WriteLine("Connected: {0} : {1}", get_ip(clients.Last()));
				}
				foreach (Thread t in threads) {
					if (!t.IsAlive) {
						t.Join();
					}
				}
				/*
				List<int> dead_client_indexes = new List<int>(); // Dead Client Indexes could be a good band name?
				for (int i = 0; i < clients.Count; i++){
					if (clients[i].Connected){
						//m = listener.get_message(clients[i]);
						//if (m.status != Status.none){
						//	handle_client(m, clients[i]);
						//} else {
						//	Console.WriteLine("Not init");
						//}
						Thread t = ;
						t.Start(clients[i]);
						threads.Add(t);
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
				} */
			}
            listener.stop_server();            
    	}
	}
}
