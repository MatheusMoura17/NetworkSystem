using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MasterServer
{
	public delegate void OnSessionGeneratedDelegate();

	//Classe responsavel por controlar as sessões de criptografia e descriptografia entre servidor e cliente.

	public class Session
	{
		private RSACryptoServiceProvider RSA;
		private RSACryptoServiceProvider clientRSA;
		//serve para chamar uma função quando a chave publica e privada for gerada,
		//tive alguns problemas quando tentei conectar varios usuarios ao mesmo tempo
		//devido ao delay causado na geração das chaves a solução foi criar um thread pra isso
		//e chamar uma função quando a sessão for gerada
		public OnSessionGeneratedDelegate onSessionGenerated;


		private string publicKey;
		private string privateKey;

		//inicia um thread em segundo plano
		public void GenerateSession(){
			Thread t=new Thread(GenerateSessionThread);
			t.IsBackground = true;
			t.Start ();
		}

		//gera uma chave publica(cliente) e privada(host) para descriptografar e criptografar
		//os pacotes
		private void GenerateSessionThread(){
			RSA=new RSACryptoServiceProvider(512);

			publicKey  = RSA.ToXmlString (false);
			privateKey = RSA.ToXmlString (true);

			onSessionGenerated.Invoke (); 
		}

		//cria um RSA e seta a chave publica enviada pelo cliente para futuras leituras
		public void SetClientSession(string clientPubKey){
			clientRSA = new RSACryptoServiceProvider();
			clientRSA.FromXmlString(clientPubKey);
		}

		//criptografa uma string usando a chave privada do servidor
		public string Encrypt(string text){
			byte[] cipherBytes = clientRSA.Encrypt (Encoding.UTF8.GetBytes (text),false);
			return Convert.ToBase64String (cipherBytes);
		}

		//descriptografa uma string enviada pelo cliente.
		public string Dencrypt(string cipherText){
			cipherText = cipherText.TrimEnd ('\0');
			byte[] cipherBytes = Convert.FromBase64String (cipherText);
			byte[] decrypted=RSA.Decrypt (cipherBytes,false);
			return Encoding.UTF8.GetString(decrypted);
		}

		//retorna a chave publica ques gerada servidor
		public string PubKey {
			get {
				return publicKey;
			}
		}
			
	}
}

