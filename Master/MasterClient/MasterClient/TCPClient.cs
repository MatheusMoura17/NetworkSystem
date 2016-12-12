using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

namespace MasterClient
{
	public class TCPClient
	{

		public int id;

		TcpClient _tcpClient;
		bool canStop=false;
		int timeoutCounter=0;

		public int TIMEOUT=30;
		public int BUFFER_SIZE=1000;
		public int SYNC_TIME=5000;

		public bool hasSecureConnection;

		private SessionManager session;
		private string host;
		private int port;
		
		public TCPClient (string host, int port)
		{
			this.host = host;
			this.port = port;
			session = new SessionManager ();
			session.onSessionGenerated += OnSessionGenerated ;
		}

		public void Start(){
			session.GenerateSession ();
		
		}

		private void OnSessionGenerated(){
			_tcpClient = new TcpClient ();
			_tcpClient.Connect (new IPEndPoint (IPAddress.Parse (host), port));

			Thread t3 = new Thread (Receiver);
			t3.IsBackground = true;
			t3.Start ();

			Thread t2 = new Thread (TimeoutCounter);
			t2.IsBackground = true;
			t2.Start ();
		}

		public void ReceivedStartFromServer(){

			Console.WriteLine (id+": Connected");

			Thread t1 = new Thread (SenderSync);
			t1.IsBackground = true;
			t1.Start ();
		}

		public void SenderSync(){
			PacketStruct packet = new PacketStruct ();
			packet.command = Command.SYNC;
			while (!canStop) {
				packet.command = Command.SYNC;
				SendSecureData (packet);
				packet.command = Command.TEST;
				SendSecureData (packet);
				Thread.Sleep (SYNC_TIME);
			}
		}

		public void SendData(PacketStruct packet){
			byte[] data=Encoding.ASCII.GetBytes(PacketConversion.PacketStructToString(packet));
			//Console.WriteLine ("Enviado: " + PacketConversion.PacketStructToString (packet));
			_tcpClient.GetStream ().Write (data, 0, data.Length);
		}

		public void SendSecureData(PacketStruct packet){
			string packetString = PacketConversion.PacketStructToString (packet);
			byte[] data=Encoding.ASCII.GetBytes(session.Encrypt(PacketConversion.PacketStructToString(packet)));
			_tcpClient.GetStream ().Write (data, 0, data.Length);
		}

		public void Receiver(){
			while (!canStop) {
				
				if (IsConnected) {
					try {
						byte[] buffer = new byte[BUFFER_SIZE];
						_tcpClient.GetStream ().Read (buffer, 0, BUFFER_SIZE);

						string receivedString=Encoding.ASCII.GetString(buffer);

						//Console.WriteLine("Recebido: "+receivedString);

						if(hasSecureConnection){
							PacketStruct receivedSecurePacket=PacketConversion.StringToPacketStruct(session.Dencrypt(receivedString));
						//	Console.WriteLine ("Received " + receivedSecurePacket.message);

							if(receivedSecurePacket.command==Command.START){
								ReceivedStartFromServer();
							}

							timeoutCounter = 0;
						}else{
							PacketStruct packet =PacketConversion.StringToPacketStruct(receivedString);

						//	Console.WriteLine ("Received " + packet.message);

							//Recebe a chave publica do servidor
							if(packet.command==Command.SESSION_SET_KEY){
								session.SetServerSession(packet.message);

								PacketStruct packetToSend=new PacketStruct();
								packetToSend.command=Command.SESSION_SET_KEY; //envia o public key para o server
								packetToSend.message=session.PubKey;

								hasSecureConnection=true;

								SendData(packetToSend);
							}

						}
							
					//	Console.WriteLine ("Received " + buffer.Length+" bytes from server");
					} catch (Exception e) {
					//	Console.WriteLine ("falha ao receber");
					}

				} else {
					Disconnect ();
				}
			}
		}
			
		public void TimeoutCounter(){
			while (!canStop) {
				if (timeoutCounter >= TIMEOUT) {
					Disconnect ();
				}
				Thread.Sleep (1000);
				timeoutCounter++;
			}
		}

		public void Disconnect(){
			canStop = true;
			_tcpClient.Close ();
			Console.WriteLine (id+": Disconnected");
		}


		//Esqueminha pego na WEB pra facilitar quando ocorre desconexões

		private bool IsConnected
		{
			get
			{
				try
				{
					if (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Connected)
					{
						/* pear to the documentation on Poll:
                * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                * -or- true if data is available for reading; 
                * -or- true if the connection has been closed, reset, or terminated; 
                * otherwise, returns false
                */

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

