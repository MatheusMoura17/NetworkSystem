using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MasterClient
{
	public enum Command{
		SYNC,
		SESSION_SET_KEY,
		ENCRYPTED_COMMAND,
		START,
		CLOSE,
		TEST
	}

	public struct PacketStruct
	{
		public Command command;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string message;
	}

	public class PacketConversion{

		//		public static byte[] GetBytes(PacketStruct packet){
		//			int size = Marshal.SizeOf(packet);
		//			byte[] arr = new byte[size];
		//
		//			IntPtr ptr = Marshal.AllocHGlobal(size);
		//			Marshal.StructureToPtr(packet, ptr, true);
		//			Marshal.Copy(ptr, arr, 0, size);
		//			Marshal.FreeHGlobal(ptr);
		//			return arr;
		//		}
		//
		//		public static PacketStruct GetPacketStruct(byte[] data){
		//			PacketStruct packet = new PacketStruct ();
		//
		//
		//			int size = Marshal.SizeOf(packet);
		//			IntPtr ptr = Marshal.AllocHGlobal(size);
		//
		//			Marshal.Copy(data, 0, ptr, size);
		//
		//			packet = (PacketStruct)Marshal.PtrToStructure(ptr, packet.GetType());
		//			Marshal.FreeHGlobal(ptr);
		//
		//			return packet;
		//		}

		public static PacketStruct StringToPacketStruct(string packetString){
			packetString=packetString.TrimEnd ('\0');
			if (packetString.Contains (",")) {
				string[] txt = packetString.Split (',');

				//Console.WriteLine ("txt[1]: '" + txt [1] + "'");

				PacketStruct packet = new PacketStruct ();
				byte[] commandBytes = Convert.FromBase64String (txt [0].TrimEnd ('\0'));
				packet.command = (Command)int.Parse (Encoding.UTF8.GetString (commandBytes));

				if (txt [1].TrimEnd ('\0') != "") {
					byte[] messageBytes = Convert.FromBase64String (txt [1].TrimEnd ('\0'));
					packet.message = Encoding.UTF8.GetString (messageBytes);
				}
					
				return packet;
			} else
				return new PacketStruct ();
		}

		public static string PacketStructToString(PacketStruct packet){
			string txt = "";
			txt+=Convert.ToBase64String (Encoding.UTF8.GetBytes(((int)packet.command).ToString()));
			txt+=",";
			if (packet.message!=null) {
				txt += Convert.ToBase64String (Encoding.UTF8.GetBytes (packet.message));
			}
			return txt;
		}

	}

}
