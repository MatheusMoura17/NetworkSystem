using System;
using System.Net;
using System.Threading;

namespace MasterClient
{
	class MainClass
	{

		public static int client=0;
		public static Random rnd = new Random();

		public static void Main (string[] args)
		{

//			Console.Write (">");
//			int clients = int.Parse(Console.ReadLine ());
//
//			for (int i = 0; i < clients; i++) {
//				Thread t = new Thread (StartThread);
//				t.IsBackground = true;
//				t.Start();
//				int time=rnd.Next (100, 1500);
//				//Thread.Sleep(time);
//				//Start ();
//			}
			//Console.ReadKey ();

//			PacketStruct pack = new PacketStruct ();
//			pack.message = "Olá mundo";
//			pack.command = Command.NEGOTIATE;
//			pack.isEncrypted = true;
//
//			byte[] data = PacketConversion.GetBytes (pack);
//
//			PacketStruct newPacket = PacketConversion.GetPacketStruct (data);
//
//			Console.WriteLine (newPacket.message+" "+data.Length);
			StartThread();
		}


		public static void StartThread(){

			client++;
			Console.WriteLine (client+": lauched");
			MasterClient tcpClient = new MasterClient ();
			//TCPClient tcpClient = new TCPClient (Dns.GetHostAddresses("matheusmoura.ddns.net")[0].ToString(), 11000);
			tcpClient.Connect ("127.0.0.1", 11000);
			while (true) {
				Console.Write ("ID: ");
				ulong id = uint.Parse(Console.ReadLine ());
				Console.Write ("Mensagem: ");
				string message = Console.ReadLine ();
				PacketStruct pack=new PacketStruct ();
				pack.command = Command.CHAT;
				pack.message = id+"," + message;

				tcpClient.SendSecureData (pack);
			}
		}
	}
}
