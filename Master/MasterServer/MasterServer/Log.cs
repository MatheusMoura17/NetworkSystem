using System;
using System.IO;

namespace MasterServer
{
	public class Log
	{
		public static void Print(object msg){
			if(Settings.Read("debug").ToString()=="1")
				Console.WriteLine (DateTime.Now.ToString()+"\t"+msg);
		}

		public static void Error(object msg){
			if (Settings.Read ("show_error").ToString() == "1") {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine (DateTime.Now.ToString () + "\t" + msg);
				Console.ResetColor ();
			}
		}

	}
}

