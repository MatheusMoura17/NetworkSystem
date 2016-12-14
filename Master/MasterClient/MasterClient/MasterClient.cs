using System;

namespace MasterClient
{
	public class MasterClient: TCPClient
	{
		public MasterClient ()
		{
			//Connect ();
		}

		public override void OnConnected ()
		{
			Console.WriteLine("Conectado, meu id: "+UserID);
		}

		public override void OnDisconnected ()
		{
			Console.WriteLine("Desconectado");
		}

		public override void OnReceiveMessage (PacketStruct packet)
		{
			Console.WriteLine("Mensagem recebida: "+packet.message);
		}
	}
}

