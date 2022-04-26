using System.Net;
using System.Runtime.InteropServices; // For converting the message to a byte array

namespace Network{
    struct Message{

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] text;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.IP_SIZE)]
        public String destination;
        public bool sent;
		public Status status = Status.none;
		public bool created = false;
		public Guid sender_id;	
		public Message(String _dest, Status _status, Guid _id){
			destination = _dest;
			status = _status;
			sent = false;
			text = new byte[32];
			sender_id = _id;
		}
        public Message(byte[] _text, String _dest, Guid _id){
            text = _text;
            destination = _dest;
            sent = false;
			status = Status.message;
			created = true;
			sender_id = _id;
        }
    }

    class MessageHandler{
        public byte[] get_bytes(Message m) {
            int size = Marshal.SizeOf(m);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public Message from_bytes(byte[] arr) {
            Message m = new Message();

            int size = Marshal.SizeOf(m);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            m = (Message)Marshal.PtrToStructure(ptr, m.GetType());
            Marshal.FreeHGlobal(ptr);

            return m;
        }
    }
}
