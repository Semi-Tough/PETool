using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PETool.IOCPNet {
	public class IOCPNet<T, TK>
		where T : IOCPSession<TK>, new()
		where TK : IOCPMsg, new() {
		private Socket socket;
		private readonly SocketAsyncEventArgs eventArgs;

		public IOCPNet() {
			eventArgs = new SocketAsyncEventArgs();
			eventArgs.Completed += IOCompleted;
		}

		#region Client
		private T session;

		public void StartAsClient(string ip, int port) {
			try {
				EndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				eventArgs.RemoteEndPoint = endPoint;
				IOCPTool.Log("Client Start...", LogColor.Green);
				StartConnect();
			}
			catch(Exception e) {
				IOCPTool.Error($"Start Client Error: {e.Message}");
			}
		}
		public void Connect() {
			StartConnect();
		}
		public bool SendMsg(TK msg) {
			if(session != null) {
				return session.SendMsg(msg);
			}
			else {
				IOCPTool.Wain("Connection is break cannot send msg.");
				return false;
			}
		}
		public bool SendMsg(byte[] bytes) {
			if(session != null) {
				return session.SendMsg(bytes);
			}
			else {
				IOCPTool.Wain("Connection is break cannot send msg.");
				return false;
			}
		}
		public void CloseClient() {
			if(session != null) {
				session.CloseSession();
				session = null;
			}
		}

		private void StartConnect() {
			bool suspend = socket.ConnectAsync(eventArgs);
			if(suspend == false) {
				ProcessConnect();
			}
		}
		private void ProcessConnect() {
			if(socket.Connected) {
				session = new T();
				session.InitSession(socket, null);
			}
			else {
				IOCPTool.Wain("Connection Fail...");
			}
		}
		#endregion

		#region Server
		private List<T> sessionList;
		private IOCPSessionPool<T, TK> sessionPool;
		private Semaphore acceptSemaphore;
		private int currentCount;

		public void StartAsServer(string ip, int port, int maxCount) {
			currentCount = 0;
			sessionList = new List<T>();
			sessionPool = new IOCPSessionPool<T, TK>(maxCount);
			acceptSemaphore = new Semaphore(maxCount, maxCount);

			for(int i = 0; i < maxCount; i++) {
				T clientSession = new T();
				sessionPool.Push(clientSession);
			}

			try {
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
				socket.Listen(maxCount);
				IOCPTool.Log("Server Start...", LogColor.Green);
				StartAccept();
			}
			catch(Exception e) {
				IOCPTool.Error($"StartServer: {e.Message}");
			}
		}
		public void BroadcastMsg(IOCPMsg msg) {
			byte[] bytes = IOCPTool.AddPackHead(IOCPTool.Serialize(msg));
			BroadcastMsg(bytes);
		}
		public void BroadcastMsg(byte[] bytes) {
			for(int i = 0; i < sessionList.Count; i++) {
				sessionList[i].SendMsg(bytes);
			}
		}
		public void CloseServer() {
			for(int i = 0; i < sessionList.Count;) {
				sessionList[i].CloseSession();
			}
			sessionList = null;
			if(socket != null) {
				socket.Close();
				socket = null;
			}
		}

		private void StartAccept() {
			eventArgs.AcceptSocket = null;
			acceptSemaphore.WaitOne();
			bool suspend = socket.AcceptAsync(eventArgs);
			if(suspend == false) {
				ProcessAccept();
			}
		}
		private void ProcessAccept() {
			Interlocked.Increment(ref currentCount);
			T clientSession = sessionPool.Pop();
			lock(sessionList) {
				sessionList.Add(clientSession);
			}
			clientSession.InitSession(eventArgs.AcceptSocket, () => {
				if(sessionList.Contains(clientSession)) {
					sessionPool.Push(clientSession);
					lock(sessionList) {
						sessionList.Remove(clientSession);
					}
					Interlocked.Decrement(ref currentCount);
					acceptSemaphore.Release();
				}
			});
			StartAccept();
		}
		#endregion

		private void IOCompleted(object sender, SocketAsyncEventArgs eventArgs) {
			switch(eventArgs.LastOperation) {
				case SocketAsyncOperation.Accept:
					ProcessAccept();
					break;
				case SocketAsyncOperation.Connect:
					ProcessConnect();
					break;
			}
		}
	}
}