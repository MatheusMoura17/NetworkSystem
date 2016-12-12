using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MasterServer
{
	public class TCPServer
	{
		TcpListener listner;

		public TCPServer (int port)
		{
			//cria uma instancia do servidor na porta informada
			listner = new TcpListener (port);
		}

		//inicia o servidor
		public void Start(){
			try {
				//inicia o listner
				listner.Start ();
				Log.Print ("Servidor iniciado");

				//inicia o recebimento clientes
				Thread startThread = new Thread (StartListening);
				startThread.IsBackground = true;
				startThread.Start ();

			} catch (Exception e) {
				//caso ja haja um servidor nessa porta acontece essa Excepção
				Log.Print ("Falha ao iniciar servidor");
			}
		}


		//recebe novos clientes
		private void StartListening(){
			//loop para receber clientes
			while (true) {
				TcpClient tcpClient = listner.AcceptTcpClient ();  //aceita a conexão
				Client client = new Client (); //cria um novo cliente
				client.Init (tcpClient); //Inicia o cliente e passa o TcpClient do mesmo
			}
		}
	}
}

