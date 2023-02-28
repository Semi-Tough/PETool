using System;
using System.Net.Sockets;

namespace PETool.AsyncNet {
	public abstract class AsyncSession<T> where T : AsyncMsg, new() {
		private Socket socket;
		private Action closeCB;
		public AsyncSessionState sessionState;


		public void InitSession(Socket socket, Action closeCB) {
			this.closeCB = closeCB;
			try {
				this.socket = socket;
				AsyncMsgPack msgPack = new AsyncMsgPack();
				this.socket.BeginReceive(
					msgPack.headBuffer,
					0,
					AsyncMsgPack.HeadLength,
					SocketFlags.None,
					RcvHeadData,
					msgPack
				);
			}
			catch(Exception e) {
				AsyncTool.Error($"InitSession: {e.Message}");
			}
			sessionState = AsyncSessionState.Connected;
			OnConnected();
		}
		public bool SendMsg(AsyncMsg msg) {
			byte[] bytes = AsyncTool.AddPackageHead(AsyncTool.Serialize(msg));
			return SendMsg(bytes);
		}
		public bool SendMsg(byte[] bytes) {
			bool result = false;
			if(sessionState != AsyncSessionState.Connected) {
				AsyncTool.Wain("Session is DisConnected,can not send message.");
			}
			else {
				try {
					NetworkStream stream = new NetworkStream(socket);
					if(stream.CanWrite) {
						stream.BeginWrite(
							bytes,
							0,
							bytes.Length,
							SendCB,
							stream
						);
					}
					result = true;
				}
				catch(Exception e) {
					AsyncTool.Error($"SendMsg: {e.Message}");
				}
			}
			return result;
		}
		public void CloseSession() {
			sessionState = AsyncSessionState.DisConnected;
			closeCB?.Invoke();
			OnDisConnected();
			try {
				if(socket == null) return;
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				socket = null;
			}
			catch(Exception e) {
				AsyncTool.Error($"CloseSession: {e.Message}");
			}
		}

		private void RcvHeadData(IAsyncResult result) {
			if(sessionState != AsyncSessionState.Connected) return;
			if(socket == null || socket.Connected == false) {
				CloseSession();
				return;
			}
			try {
				AsyncMsgPack msgPack = (AsyncMsgPack)result.AsyncState;
				int length = socket.EndReceive(result);
				if(length == 0) {
					CloseSession();
				}
				else {
					msgPack.headIndex += length;
					if(msgPack.headIndex < AsyncMsgPack.HeadLength) {
						socket.BeginReceive(
							msgPack.headBuffer,
							msgPack.headIndex,
							AsyncMsgPack.HeadLength - msgPack.bodyIndex,
							SocketFlags.None,
							RcvHeadData,
							msgPack
						);
					}
					else {
						msgPack.InitBodyBuffer();
						socket.BeginReceive(
							msgPack.bodyBuffer,
							msgPack.bodyIndex,
							msgPack.bodyLength,
							SocketFlags.None,
							RcvBodyData,
							msgPack
						);
					}
				}
			}
			catch(Exception e) {
				AsyncTool.Error($"RcvHeadData: {e.Message}");
				CloseSession();
			}
		}
		private void RcvBodyData(IAsyncResult result) {
			if(sessionState != AsyncSessionState.Connected) return;
			if(socket == null || socket.Connected == false) {
				CloseSession();
				return;
			}
			try {
				AsyncMsgPack msgPack = (AsyncMsgPack)result.AsyncState;
				int length = socket.EndReceive(result);
				if(length == 0) {
					CloseSession();
				}
				else {
					msgPack.bodyIndex += length;
					if(msgPack.bodyIndex < msgPack.bodyLength) {
						socket.BeginReceive(
							msgPack.bodyBuffer,
							msgPack.bodyIndex,
							msgPack.bodyLength - msgPack.bodyIndex,
							SocketFlags.None,
							RcvBodyData,
							msgPack
						);
					}
					else {
						T msg = AsyncTool.DeSerialize<T>(msgPack.bodyBuffer);
						OnReceiveMsg(msg);

						msgPack.ResetData();
						socket.BeginReceive(
							msgPack.headBuffer,
							msgPack.headIndex,
							AsyncMsgPack.HeadLength,
							SocketFlags.None,
							RcvHeadData,
							msgPack
						);
					}
				}
			}
			catch(Exception e) {
				AsyncTool.Error($"RcvBodyData: {e.Message}");
				CloseSession();
			}
		}
		private void SendCB(IAsyncResult result) {
			NetworkStream stream = (NetworkStream)result.AsyncState;
			try {
				stream.EndWrite(result);
				stream.Flush();
				stream.Close();
			}
			catch(Exception e) {
				AsyncTool.Error($"SendCB: {e.Message}");
			}
		}


		protected abstract void OnConnected();
		protected abstract void OnReceiveMsg(T msg);
		protected abstract void OnDisConnected();
	}
	public enum AsyncSessionState {
		None,
		Connected,
		DisConnected
	}
}