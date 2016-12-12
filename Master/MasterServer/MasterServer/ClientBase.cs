using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace MasterServer
{

	public class ClientBase
	{

		private Thread _thread; //Thread deste cliente
		private TcpClient _tcpClient;
		private int timeoutConter=0;
		private bool canStop=false;
		private string iep;
		private Session session=new Session();
		private DateTime startedTime; //tempo em que o cliente se conectou
		private bool hasSecureChannel; //cliente e servidor compartilham informações com segurança

		//inicia a manipuação do cliente
		public void Init (TcpClient _tcpClient)
		{
			startedTime = DateTime.Now;
			this._tcpClient = _tcpClient;
			try{
				iep = _tcpClient.Client.RemoteEndPoint.ToString();
				Log.Print(iep+"\tConectado");
				//cria uma nova sessão
				session.onSessionGenerated+=OnSessionGenerated;
				session.GenerateSession();
			}catch(Exception e){}
		}

		//é chamado quando a sessão é gerada
		private void OnSessionGenerated(){
			StartThreads ();

			Log.Print(iep+"\tSessão gerada");
			PacketStruct packet = new PacketStruct();
			packet.command=Command.SESSION_SET_KEY;
			packet.message=session.PubKey;

			SendData(packet);
		}

		//inicia o receptor de mensagens e o timeout
		private void StartThreads(){
			_thread = new Thread (Receiver);
			_thread.IsBackground = true;
			_thread.Start ();

			Thread t = new Thread (TimeoutThread);
			t.IsBackground = true;
			t.Start ();
		}

		//envia mensagens inseguras para o cliente
		private void SendData(PacketStruct packet){
			try{
				if(IsConnected){
					string packetString = PacketConversion.PacketStructToString (packet);
					byte[] data = Encoding.ASCII.GetBytes(packetString);
					_tcpClient.GetStream ().Write (data, 0, data.Length);
				}
			}catch(Exception e){
				Log.Error ("Falha ao enviar mensagem");
			}
		}

		//envia mensagens criptografadas para o cliente
		private void SendSecureData(PacketStruct packet){
			try{
				if(IsConnected){
					string packetString=PacketConversion.PacketStructToString(packet);
					string packetStringEncrypted = session.Encrypt (packetString);
					byte[] data = Encoding.ASCII.GetBytes(packetStringEncrypted);
					_tcpClient.GetStream ().Write (data, 0, data.Length);
				}
			}catch(Exception e){
				Log.Error (iep+"\tFalha ao enviar mensagem cryptografada");
			}
		}

		private void Receiver(){
			while (!canStop) {
				if (IsConnected) {
					try{
						byte[] received=new byte[1000];

						_tcpClient.GetStream ().Read (received,0,received.Length);

						string receivedString=Encoding.ASCII.GetString(received);

						//canal seguro, chega apenas dados criptografados
						if(hasSecureChannel){
							string decryptedStringReceived=session.Dencrypt(receivedString);

							PacketStruct receivedSecurePacket= PacketConversion.StringToPacketStruct(decryptedStringReceived);
						
							//recebe um sinal de syncronização do cliente
							if(receivedSecurePacket.command==Command.SYNC){
								PacketStruct packetToSend=new PacketStruct();
								packetToSend.command=Command.SYNC;
								SendSecureData(packetToSend);
							
								//recebe um quit do cliente
							}else if(receivedSecurePacket.command==Command.CLOSE){
								OnReceiveMessage(receivedSecurePacket);
								Disconnect();
							}else{
								OnReceiveMessage(receivedSecurePacket);
							}


							timeoutConter = 0;

						//canal inseguro, recebe apenas dados não criptografados
						//é importante para receber a chave de criptografia
						}else{											

							PacketStruct receivedPacket=PacketConversion.StringToPacketStruct(receivedString);


							//Recebe a chave publica do cliente
							if(receivedPacket.command==Command.SESSION_SET_KEY){
								session.SetClientSession(receivedPacket.message);
								hasSecureChannel=true;
								OnAcceptedConnection();

								//pede para o cliente iniciar startar os modulos
								PacketStruct packetToSend=new PacketStruct();
								packetToSend.command=Command.START;
								SendSecureData(packetToSend);

							}
						}
							
					}catch(Exception e){
						//caso haja expepção em qualquer classe deste modulo o usuário é kickado,
						//isso evita ataques DoS
						Disconnect ();
					}
						
				}
				//se falhar na validação do 'IsConnected' o usuario é kickado
				else{
					Disconnect();
				}
			}
		}

		//Kicka o usuário caso nenhuma informação seja recebida em 30 segundos
		private void TimeoutThread(){
			while (!canStop) {
				//timeout counter
				Thread.Sleep (1000);
				//lê o arquivo de configuração e verifica o timeout
				Log.Print (iep + "\tTimeout: " + timeoutConter + "/" + Settings.Read ("user_connection_time_out"));
				//se a contagem de segundos for maior do que o timeout do arquivo o usuário é kickado
				if (timeoutConter >= Convert.ToInt32 (Settings.Read ("user_connection_time_out"))) {
					//desconectado por time out
					Disconnect ();
				}
				timeoutConter++;
			}
		}

		//desconecta o cliente a qualquer custo
		public void Disconnect(){
			//tempo em que o usuario ficou conectado
			TimeSpan time=DateTime.Now.Subtract(startedTime);
			Log.Print (iep + "\tdesconectado\ttempo de conexão:"+time.ToString()+"\tencryptado? "+hasSecureChannel);
			canStop=true;
			_tcpClient.Close();
			OnDisconnected ();
		}


		/********FUNÇÕES PARA O HERDEIRO*********/
		public bool Send(Command command, object[] args){
			PacketStruct packet= new PacketStruct ();
			packet.command = command;

			foreach (object o in args) {
				packet.message+= o.ToString();
			}
			int packetLength=PacketConversion.PacketStructToString(packet).Length;
			if(packetLength<=64){
				SendSecureData(packet);
				return true;
			}else{
				Log.Error("Mensagem muito grande pra ser enviada! "+packetLength+"/64bytes");
				return false;
			}
		}
		public virtual void OnReceiveMessage(PacketStruct packet){}
		public virtual void OnAcceptedConnection (){}
		public virtual void OnDisconnected (){}

		//retorna false apenas se o usuário se desconectar dando um close na conexão
		//caso o usuário perca conexão (falta de energia,internet, etc) o valor será retornado como true,
		//para resolver esse problema existe o TimeoutCounterThread();
		private bool IsConnected
		{
			get
			{
				try
				{
					if (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Connected)
					{
						// Detect if client disconnected
						if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
						{
							byte[] buff = new byte[1];
							if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
							{
								// Client disconnected
								return false;
							}
							else
							{
								return true;
							}
						}

						return true;
					}
					else
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
		}

	}
}

