using System.Collections.Generic;

namespace PETool.IOCPNet {
	public class IOCPSessionPool<T, TK>
		where T : IOCPSession<TK>, new()
		where TK : IOCPMsg, new() {
		private readonly Stack<T> sessions;
		public int Size {
			get {
				lock(sessions) {
					return sessions.Count;
				}
			}
		}

		public IOCPSessionPool(int capacity) {
			sessions = new Stack<T>(capacity);
		}

		public T Pop() {
			lock(sessions) {
				return sessions.Pop();
			}
		}
		public void Push(T session) {
			if(session == null) {
				IOCPTool.Error("Push Session To Pool Cannot be null.");
			}
			lock(sessions) {
				sessions.Push(session);
			}
		}
	}
}