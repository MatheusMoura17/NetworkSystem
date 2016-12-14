using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MasterClient
{

	public delegate void OnSessionGeneratedDelegate();

	public class Session
	{
		private RSACryptoServiceProvider RSA;
		private RSACryptoServiceProvider serverRSA;
		public OnSessionGeneratedDelegate onSessionGenerated;

		private string publicKey;
		private string privateKey;
	//	private DateTime expiration;

		public void GenerateSession(){
			Thread t=new Thread(GenerateSessionThread);
			t.IsBackground = true;
			t.Start ();
		}

		private void GenerateSessionThread(){
			RSA = new RSACryptoServiceProvider (512);

			publicKey  = RSA.ToXmlString (false);
			privateKey = RSA.ToXmlString (true);

			onSessionGenerated.Invoke ();
		}

		public void SetServerSession(string serverPubKey){
			serverRSA = new RSACryptoServiceProvider();
			serverRSA.FromXmlString(serverPubKey);
		}

//		public void Renew(){
//			RSA=new RSACryptoServiceProvider(1024);
//
//			publicKey  = RSA.ToXmlString (false);
//			privateKey = RSA.ToXmlString (tru	e);
//
//			expiration = DateTime.Now.AddMinutes (5);
//		}

		public string Encrypt(string text){
			byte[] cipherBytes = serverRSA.Encrypt (Encoding.UTF8.GetBytes (text),false);
			return Convert.ToBase64String (cipherBytes);
		}

		public string Dencrypt(string cipherText){
			cipherText = cipherText.TrimEnd ('\0');
			byte[] cipherBytes = Convert.FromBase64String (cipherText);
			byte[] decrypted=RSA.Decrypt (cipherBytes,false);
			return Encoding.UTF8.GetString(decrypted);
		}

		public string PubKey {
			get {
				return publicKey;
			}
		}

//		public DateTime Expiration{
//			get{
//				return expiration;
//			}
//		}
			
	}
}

