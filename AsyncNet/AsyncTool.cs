using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PETool.AsyncNet {
	public static class AsyncTool {
		public static byte[] AddPackageHead(byte[] bytes) {
			int length = bytes.Length;
			byte[] package = new byte[length + 4];
			byte[] head = BitConverter.GetBytes(length);
			head.CopyTo(package, 0);
			bytes.CopyTo(package, 4);
			return package;
		}
		public static byte[] Serialize<T>(T msg) where T : AsyncMsg {
			byte[] date = null;
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();

			try {
				formatter.Serialize(stream, msg);
				stream.Seek(0, SeekOrigin.Begin);
				date = stream.ToArray();
			}
			catch(SerializationException e) {
				Error($"Serialize: {e.Message}");
			}
			finally {
				stream.Close();
			}
			return date;
		}
		public static T DeSerialize<T>(byte[] bytes) where T : AsyncMsg {
			T msg = null;
			MemoryStream stream = new MemoryStream(bytes);
			BinaryFormatter formatter = new BinaryFormatter();

			try {
				msg = (T)formatter.Deserialize(stream);
			}
			catch(SerializationException e) {
				Error($"DeSerialize: {e.Message}");
			}
			finally {
				stream.Close();
			}
			return msg;
		}


		public static readonly Action<string> LogFunc = null;
		public static readonly Action<string> WainFunc = null;
		public static readonly Action<string> ErrorFunc = null;


		public static void Log(string msg, LogColor color = LogColor.None) {
			if(LogFunc != null) {
				LogFunc.Invoke(msg);
			}
			else {
				ConsoleLog(msg, color);
			}
		}
		public static void Wain(string msg) {
			if(WainFunc != null) {
				WainFunc.Invoke(msg);
			}
			else {
				ConsoleLog(msg, LogColor.Yellow);
			}
		}
		public static void Error(string msg) {
			if(ErrorFunc != null) {
				ErrorFunc.Invoke(msg);
			}
			else {
				ConsoleLog(msg, LogColor.Red);
			}
		}
		private static void ConsoleLog(string msg, LogColor color) {
			switch(color) {
				case LogColor.Red:
					Console.ForegroundColor = ConsoleColor.DarkRed;
					break;
				case LogColor.Green:
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case LogColor.Blue:
					Console.ForegroundColor = ConsoleColor.Blue;
					break;
				case LogColor.Magenta:
					Console.ForegroundColor = ConsoleColor.Magenta;
					break;
				case LogColor.Yellow:
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					break;
				case LogColor.Cyan:
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
			}
			Console.WriteLine(msg);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
	public enum LogColor {
		None,
		Red,
		Green,
		Blue,
		Cyan,
		Magenta,
		Yellow
	}
}