using System;
using System.Net.Sockets;

namespace MasterServer
{
	public class Client : ClientBase
	{

		/*
		 *  Send(Command commando, Object[] args) - envia uma mensagem para o cliente
		 * 	Disconnect() - Desconecta o cliente atual    
		 */

		public override void OnReceiveMessage (PacketStruct packet){
			//Log.Print ("Recebido: "+packet.command);
			//Console.WriteLine("Enviado? "+Send (Command.SYNC, new object[]{"1234lk98987yylkkhkjhkjhkjhkjhkhkjhkjyiyi56"}));
			if(packet.command==Command.CHAT){
				string[] input=packet.message.Split(',');
				masterServer.SendMessage(UserId,uint.Parse(input[0]),Command.CHAT,input[1]);
			}
				
				
		}

		public override void OnAcceptedConnection ()
		{
			Console.WriteLine ("Conexão aceita");
		}

		public override void OnDisconnected ()
		{
			Console.WriteLine ("cliente desconectado");
		}


	}
}

