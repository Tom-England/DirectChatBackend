using System;
using System.Net;
using System.Runtime.InteropServices; // For converting the message to a byte array

namespace Network{
	public class User{
		private string name;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		private Guid id;
		public Guid Id{
			get { return id; }
			set { id = value; }
		}
		public User(){

		}
		public User(string _name){
			id = Guid.NewGuid();
			Name = _name;
		}

		public struct UserTransferable{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.MESSAGE_SIZE)]
			public string name;
			public Guid id;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] key;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] iv;
			public bool created = false;
			public UserTransferable(string _name, Guid _id, byte[] _key, byte[] _iv){
				name = _name;
				id = _id;
				key = _key;
				iv = _iv;
			}
		}

		public byte[] get_bytes(UserTransferable u) {
			int size = Marshal.SizeOf(u);
			byte[] arr = new byte[size];

			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(u, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		public UserTransferable from_bytes(byte[] arr) {
			UserTransferable u = new UserTransferable();

			int size = Marshal.SizeOf(u);
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.Copy(arr, 0, ptr, size);

			u = (UserTransferable)Marshal.PtrToStructure(ptr, u.GetType());
			Marshal.FreeHGlobal(ptr);

			return u;
		}
	}
}
