using System;

namespace MasterServer
{	
	class MainClass
	{

		public static void Main (string[] args)
		{

			Settings settings = new Settings ();
			Settings.instance = settings;
			settings.LoadSettings ();

		//	Log.Print ("Iniciando...");
			MasterServer server = new MasterServer (11000);
			server.Start ();
			Console.ReadKey ();

//			SessionManager sm = new SessionManager ();
//			sm.GenerateSession ();


//			SessionManager sm2 = new SessionManager ();
//			sm2.GenerateSession ();
//			sm2.SetClientSession (sm.PubKey);
//
//			sm.SetClientSession (sm2.PubKey);
//
//
//			PacketStruct pack = new PacketStruct ();
//			pack.command = Command.SESSION_SET_KEY;
//			pack.message = "éééééééééé";
////
////			byte[] packB=sm2.EncryptPacketStructure (pack);
////
////			PacketStruct packetDescrypted = sm.DencryptPacket (packB);
//
//			Console.WriteLine (PacketConversion.PacketStructToString(pack)+" l: "+PacketConversion.PacketStructToString(pack).Length);
//			Console.WriteLine ("c: "+PacketConversion.StringToPacketStruct(PacketConversion.PacketStructToString(pack)).command+" m:"+PacketConversion.StringToPacketStruct(PacketConversion.PacketStructToString(pack)).message);


//
//
//			Console.WriteLine ("public key : "+sm.PubKey);
//
//			string encrypted = sm.Encrypt ("Hello");
//
//			Console.WriteLine ("\n'Hello' encrypted: "+sm.Encrypt("Hello"));
//			Console.WriteLine ("decrypted: " + sm2.Dencrypt (encrypted));
		}

	}
}