using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace PETool.PELogger {
	public static class ExtensionMethod {
		public static void Log(this object o, string msg) {
			PELogger.Log(msg);
		}
		public static void Log(this object o, object obj) {
			PELogger.Log(obj);
		}
		public static void ColorLog(this object o, string msg, LogColor color) {
			PELogger.ColorLog(msg, color);
		}
		public static void ColorLog(this object o, object obj, LogColor color) {
			PELogger.ColorLog(obj, color);
		}
		public static void Wain(this object o, string msg) {
			PELogger.Wain(msg);
		}
		public static void Wain(this object o, object obj) {
			PELogger.Wain(obj);
		}
		public static void Error(this object o, string msg) {
			PELogger.Error(msg);
		}
		public static void Error(this object o, object obj) {
			PELogger.Error(obj);
		}
		public static void Trace(this object o, string msg) {
			PELogger.Trace(msg);
		}
		public static void Trace(this object o, object obj) {
			PELogger.Trace(obj);
		}
	}

	public static class PELogger {
		private class ConsoleLogger : ILogger {
			public void Log(string msg, LogColor color = LogColor.None) {
				WriteConsoleLog(msg, color);
			}
			public void Error(string msg, LogColor color = LogColor.Red) {
				WriteConsoleLog(msg, color);
			}
			public void Wain(string msg, LogColor color = LogColor.Yellow) {
				WriteConsoleLog(msg, color);
			}
			private void WriteConsoleLog(string msg, LogColor color) {
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
		private class UnityLogger : ILogger {
			private Type type = Type.GetType("UnityEngine.Debug,UnityEngine");
			public void Log(string msg, LogColor color = LogColor.None) {
				if(color != LogColor.None) {
					msg = WriteUnityLog(msg, color);
				}
				type.GetMethod("PELogger", new[]{ typeof(object) })?.Invoke(null, new object[]{ msg });
			}
			public void Error(string msg, LogColor color = LogColor.Red) {
				type.GetMethod("LogError", new[]{ typeof(object) })?.Invoke(null, new object[]{ msg });
				WriteUnityLog(msg, color);
			}
			public void Wain(string msg, LogColor color = LogColor.Yellow) {
				type.GetMethod("LogWarning", new[]{ typeof(object) })?.Invoke(null, new object[]{ msg });
				WriteUnityLog(msg, color);
			}
			private string WriteUnityLog(string msg, LogColor color) {
				switch(color) {
					case LogColor.Red:
						msg = $"<color=#FF0000>{msg}</color>";
						break;
					case LogColor.Green:
						msg = $"<color=#00FF00>{msg}</color>";
						break;
					case LogColor.Blue:
						msg = $"<color=#0000FF>{msg}</color>";
						break;
					case LogColor.Cyan:
						msg = $"<color=#00FFFF>{msg}</color>";
						break;
					case LogColor.Magenta:
						msg = $"<color=#FF00FF>{msg}</color>";
						break;
					case LogColor.Yellow:
						msg = $"<color=#FFFF00>{msg}</color>";
						break;
				}
				return msg;
			}
		}

		public static LogConfig logCfg;
		private static ILogger logger;
		private static StreamWriter streamWriter;
		private const string LogLock = "PELoggerLock";
		public static void InitSetting(LogConfig cfg = null) {
			logCfg = cfg ?? new LogConfig();

			if(logCfg.loggerType == LoggerType.Console) {
				logger = new ConsoleLogger();
			}
			else {
				logger = new UnityLogger();
			}

			if(logCfg.enableSave) {
				if(logCfg.enableCover) {
					string path = logCfg.SavePath + logCfg.SaveName;
					try {
						if(Directory.Exists(logCfg.SavePath)) {
							if(File.Exists(path)) {
								File.Delete(path);
							}
						}
						else {
							Directory.CreateDirectory(logCfg.SavePath);
						}
						streamWriter = File.AppendText(path);
						streamWriter.AutoFlush = true;
					}
					catch {
						streamWriter = null;
					}
				}
				else {
					string prefix = DateTime.Now.ToString("yyyyMMdd@HH-mm-ss");
					string path = logCfg.SavePath + prefix + logCfg.SaveName;
					try {
						if(Directory.Exists(cfg?.SavePath) == false) {
							Directory.CreateDirectory(logCfg.SavePath);
						}
						streamWriter = File.AppendText(path);
						streamWriter.AutoFlush = true;
					}
					catch {
						streamWriter = null;
					}
				}
			}
		}

		public static void Log(string msg) {
			if(logCfg.enableLog == false) {
				return;
			}
			msg = DecorateLog($"{msg}");

			lock(LogLock) {
				logger.Log(msg);
				if(logCfg.enableSave) {
					WriteToFile($"[L]{msg}");
				}
			}
		}
		public static void Log(object obj) {
			if(logCfg.enableLog == false) {
				return;
			}
			string msg = DecorateLog($"{obj}");

			lock(LogLock) {
				logger.Log(msg);
				if(logCfg.enableSave) {
					WriteToFile($"[L]{msg}");
				}
			}
		}
		public static void ColorLog(string msg, LogColor color) {
			if(logCfg.enableLog == false) {
				return;
			}
			msg = DecorateLog($"{msg}");
			lock(LogLock) {
				logger.Log(msg, color);
				if(logCfg.enableSave) {
					WriteToFile($"[L]{msg}");
				}
			}
		}
		public static void ColorLog(object obj, LogColor color) {
			if(logCfg.enableLog == false) {
				return;
			}
			string msg = DecorateLog($"{obj}");
			lock(LogLock) {
				logger.Log(msg, color);
				if(logCfg.enableSave) {
					WriteToFile($"[L]{msg}");
				}
			}
		}
		public static void Wain(string msg) {
			if(logCfg.enableLog == false) {
				return;
			}
			msg = DecorateLog($"{msg}");
			lock(LogLock) {
				logger.Wain(msg);
				if(logCfg.enableSave) {
					WriteToFile($"[W]{msg}");
				}
			}
		}
		public static void Wain(object obj) {
			if(logCfg.enableLog == false) {
				return;
			}
			string msg = DecorateLog($"{obj}");
			lock(LogLock) {
				logger.Wain(msg);
				if(logCfg.enableSave) {
					WriteToFile($"[W]{msg}");
				}
			}
		}
		public static void Error(string msg) {
			if(logCfg.enableLog == false) {
				return;
			}
			msg = DecorateLog($"{msg}", logCfg.enableTrace);
			lock(LogLock) {
				logger.Error(msg);
				if(logCfg.enableSave) {
					WriteToFile($"[E]{msg}");
				}
			}
		}
		public static void Error(object obj) {
			if(logCfg.enableLog == false) {
				return;
			}
			string msg = DecorateLog($"{obj}", logCfg.enableTrace);
			lock(LogLock) {
				logger.Error(msg);
				if(logCfg.enableSave) {
					WriteToFile($"[E]{msg}");
				}
			}
		}
		public static void Trace(string msg) {
			if(logCfg.enableLog == false) {
				return;
			}
			msg = DecorateLog($"{msg}", logCfg.enableTrace);
			lock(LogLock) {
				logger.Log(msg, LogColor.Magenta);
				if(logCfg.enableSave) {
					WriteToFile($"[T]{msg}");
				}
			}
		}
		public static void Trace(object obj) {
			if(logCfg.enableLog == false) {
				return;
			}
			string msg = DecorateLog($"{obj}", logCfg.enableTrace);
			lock(LogLock) {
				logger.Log(msg, LogColor.Magenta);
				if(logCfg.enableSave) {
					WriteToFile($"[T]{msg}");
				}
			}
		}


		public static string DecorateLog(string msg, bool isTrac = false) {
			StringBuilder sb = new StringBuilder(logCfg.logPrefix, 100);
			if(logCfg.enableTime) {
				sb.Append(GetTime());
			}
			if(logCfg.enableThreadId) {
				sb.Append(GetThreadId());
			}

			sb.Append($" {logCfg.logSeparate} {msg}");

			if(isTrac) {
				sb.Append(GetStackTrace());
			}
			return sb.ToString();
		}
		private static string GetTime() {
			return$"  {DateTime.Now:hh:mm:ss--fff}";
		}
		private static string GetThreadId() {
			return$"  ThreadID:{Thread.CurrentThread.ManagedThreadId}";
		}
		private static string GetStackTrace() {
			StackTrace st = new StackTrace(3, true);
			StringBuilder trackInfo = new StringBuilder(100);

			for(int i = 0; i < st.FrameCount; i++) {
				StackFrame sf = st.GetFrame(i);
				trackInfo.Append($"\n{sf.GetFileName()}::{sf.GetMethod()} line:{sf.GetFileLineNumber()}");
			}

			return$"\nStackTrace: {trackInfo}";
		}
		private static void WriteToFile(string msg) {
			if(streamWriter != null) {
				try {
					streamWriter.WriteLine($"{msg}");
				}
				catch {
					streamWriter = null;
				}
			}
		}
	}
}