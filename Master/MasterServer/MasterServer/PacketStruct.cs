using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MasterServer
{
	public enum Command{
		SYNC,
		SESSION_SET_KEY,
		ENCRYPTED_COMMAND,
		START,
		CLOSE,
		AUTENTICATE
		//adicione aqui os comandos que você quiser, porém não use ou remova os comandos acima
		//isso pode resultar em crash, dsconexão do usuário, ou até mesmo encerramento do servidor.
	}

	//estrutura para enviar e receber informações de forma mais organizada
	public struct PacketStruct
	{
		public Command command;
		public string message;
	}

	public class PacketConversion{

		//Esta classe serve para converter os objetos PacketStruct para string, ou vice versa

		//converte uma string em um PacketStruct
		public static PacketStruct StringToPacketStruct(string packetString){
			//limpa os espaços em branco à direita
			packetString=packetString.TrimEnd ('\0');
			//quebra a string em "comando","mensagem"
			string[] txt = packetString.Split (',');

			//cria um novo PacketStruct
			PacketStruct packet = new PacketStruct (); 

			//Converte a string 0 em Command
			byte[] commandBytes = Convert.FromBase64String (txt [0].TrimEnd ('\0'));
			packet.command = (Command)int.Parse (Encoding.UTF8.GetString (commandBytes));
	
			//Converte a string 0 em uma string legivel 
			byte[] messageBytes = Convert.FromBase64String (txt [1].TrimEnd ('\0'));
			packet.message = Encoding.UTF8.GetString (messageBytes);

			return packet;
		}

		//Converte um PacketStruct em uma string
		public static string PacketStructToString(PacketStruct packet){
			string txt = "";
			//converte os valores para base64 e separa com uma virgula
			txt+=Convert.ToBase64String (Encoding.UTF8.GetBytes(((int)packet.command).ToString()));
			txt+=",";
			txt += Convert.ToBase64String (Encoding.UTF8.GetBytes (""+packet.message));
			return txt;
		}

	}

}

