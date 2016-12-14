using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace MasterServer
{
	public class MasterServer
	{
		private TcpListener listner;
		private List<Client> clientList=new List<Client>();
		private ulong connectionCounter;

		public MasterServer (int port)
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

				//inicia o testador
				Thread startThreadtest= new Thread (ThreadTest);
				startThreadtest.IsBackground = true;
				startThreadtest.Start ();


				//inicia o recebimento clientes
				Thread startThread = new Thread (StartListening);
				startThread.IsBackground = true;
				startThread.Start ();

			} catch (Exception e) {
				//caso ja haja um servidor nessa porta acontece essa Excepção
				Log.Print ("Falha ao iniciar servidor");
			}
		}

		public void SendMessage(ulong fromID, ulong targetID, Command command, string args){
			try{
			Client fromClient = GetClientFromID (fromID);
			Client targetClient = GetClientFromID (targetID);

			targetClient.Send (command,args);
			}catch(Exception e){}
		}

		//checa se um usuario especifico está online
		public Client GetClientFromID(ulong clientID){
			lock (this) {
				foreach (Client c in clientList)
					if (c.UserId == clientID) {
						return c;
					}
				return null;
				}
		}

		//checa se um usuario especifico está online
		public bool UserIsOnline(ulong clientID){
			lock (this) {
				foreach (Client c in clientList)
					if (c.UserId == clientID && c.isReady) {
						return true;
					}
				return false;
			}
		}

		//remove um usuario especifico
		public void RemoveClient(ulong clientID){
			lock (this) {
				foreach (Client c in clientList)
					if (c.UserId == clientID) {
						clientList.Remove (c);
						break;
					}
			}
		}

		private void ThreadTest(){
			while (true) {
				Console.Clear ();
				Console.WriteLine ("USER_ID \t CONEXÃO\n");
				if (clientList.Count != 0) {
					for (int i = 0; i < clientList.Count; i++)
						Console.WriteLine (clientList[i].UserId+"\t"+clientList[i].ConnectedTime);
				} else
					Console.WriteLine ("Nenhum usuário conectado");

				Thread.Sleep (1000);
			}
		}

		//recebe novos clientes
		private void StartListening(){
			//loop para receber clientes
			while (true) {
				TcpClient tcpClient = listner.AcceptTcpClient ();  //aceita a conexão
				Client client = new Client (); //cria um novo cliente
				client.UserId=connectionCounter;
				client.masterServer=this; //passa o objeto do masterServer para o client criado
				client.Init (tcpClient); //Inicia o cliente e passa o TcpClient do mesmo
				clientList.Add(client); //Adiciona o cliente na lista
				connectionCounter++;
			}
		}
	}
}

