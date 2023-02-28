using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PETool.AsyncNet {
	public class AsyncNet<T, TK>
		where T : AsyncSession<TK>, new()
		where TK : AsyncMsg, new() {
		private Socket socket;

		#region Server
		private List<T> sessionList;

		public void StartAsServer(string ip, int port, int maxCount) {
			sessionList = new List<T>();
			try {
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
				socket.Listen(maxCount);
				AsyncTool.Log("Server Start...", LogColor.Green);
				socket.BeginAccept(ClientConnectCB, null);
			}
			catch(Exception e) {
				AsyncTool.Error($"StartServer: {e.Message}");
			}
		}
		public void BroadcastMsg(AsyncMsg msg) {
			byte[] bytes = AsyncTool.AddPackageHead(AsyncTool.Serialize(msg));
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

		private void ClientConnectCB(IAsyncResult result) {
			T clientSession = new T();
			try {
				if(socket == null) return;
				Socket clientSocket = socket.EndAccept(result);
				if(clientSocket.Connected) {
					lock(sessionList) {
						sessionList.Add(clientSession);
					}
					clientSession.InitSession(clientSocket, () => {
						if(sessionList.Contains(clientSession)) {
							lock(sessionList) {
								sessionList.Remove(clientSession);
							}
						}
					});
				}
				socket.BeginAccept(ClientConnectCB, null);
			}
			catch(Exception e) {
				AsyncTool.Error($"ClientConnectCB: {e.Message}");
			}
		}
		#endregion

		 #region Client
		public T session;

		public void StartAsClient(string ip, int port) {
			try {
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ServerConnectCB, null);
				AsyncTool.Log("Client Start...", LogColor.Green);
			}
			catch(Exception e) {
				AsyncTool.Error($"StartServer: {e.Message}");
			}
		}
		public void CloseClient() {
			if(session != null) {
				session.CloseSession();
				session = null;
			}
			socket = null;
		}
		private void ServerConnectCB(IAsyncResult result) {
			try {
				if(socket == null) return;
				socket.EndConnect(result);
				if(socket.Connected) {
					session = new T();
					session.InitSession(socket, null);
				}
			}
			catch(Exception e) {
				AsyncTool.Error($"ServerConnectCB: {e.Message}");
			}
		}
		#endregion
	}
}