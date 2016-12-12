using System;
using System.Collections.Generic;
using System.IO;

namespace MasterServer
{
	public class Settings
	{

		public static Settings instance;
		Dictionary<string,string> dictionary;

		public void LoadSettings(){
			dictionary = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines("settings.txt"))
			{
				if ((!string.IsNullOrEmpty(line)) &&
					(!line.StartsWith(";")) &&
					(!line.StartsWith("#")) &&
					(!line.StartsWith("'")) &&
					(line.Contains("=")))
				{
					int index = line.IndexOf('=');
					string key = line.Substring(0, index).Trim();
					string value = line.Substring(index + 1).Trim();

					if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
						(value.StartsWith("'") && value.EndsWith("'")))
					{
						value = value.Substring(1, value.Length - 2);
					}
					dictionary.Add(key, value);
				}
			}
		}

		public static object Read(string key){
			try{
				return instance.dictionary[key];
			}catch(Exception e){
				Log.Print ("key :" + key + " não definida");
					return null;
			}
		}
			
	}
}

