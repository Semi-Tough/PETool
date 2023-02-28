using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace PETool.IOCPNet {
	public abstract class IOCPSession<T> where T : IOCPMsg, new() {
		private readonly SocketAsyncEventArgs sendArgs;
		private readonly SocketAsyncEventArgs rcvArgs;
		private readonly Queue<byte[]> cacheQueue;
		private List<byte> readList;
		private Socket socket;
		private Action closeCB;
		private bool isWrite;

		public SessionState sessionState;

		public IOCPSession() {
			sendArgs = new SocketAsyncEventArgs();
			rcvArgs = new SocketAsyncEventArgs();
			readList = new List<byte>();
			cacheQueue = new Queue<byte[]>();
			rcvArgs.SetBuffer(new byte[2048], 0, 2048);
			rcvArgs.Completed += IOCompleted;
			sendArgs.Completed += IOCompleted;
		}

		public void InitSession(Socket socket, Action closeCB) {
			this.socket = socket;
			this.closeCB = closeCB;
			sessionState = SessionState.Connected;
			OnConnected();
			StartAsyncRcv();
		}
		public bool SendMsg(T msg) {
			byte[] bytes = IOCPTool.AddPackHead(IOCPTool.Serialize(msg));
			return SendMsg(bytes);
		}
		public bool SendMsg(byte[] bytes) {
			if(sessionState != SessionState.Connected) {
				IOCPTool.Wain("Connection is break cannot send msg.");
				return false;
			}

			if(isWrite) {
				cacheQueue.Enqueue(bytes);
				return true;
			}

			isWrite = true;
			sendArgs.SetBuffer(bytes, 0, bytes.Length);
			bool suspend = socket.SendAsync(sendArgs);
			if(suspend == false) {
				ProcessSend();
			}
			return true;
		}
		public void CloseSession() {
			if(socket != null) {
				sessionState = SessionState.DisConnected;
				closeCB?.Invoke();
				OnDisconnected();
				readList.Clear();
				cacheQueue.Clear();
				isWrite = false;
				try {
					socket.Shutdown(SocketShutdown.Both);
				}
				catch(Exception e) {
					IOCPTool.Error($"Shutdown Socket Error: {e.Message}");
				}
				finally {
					socket.Close();
					socket = null;
				}
			}
		}

		private void StartAsyncRcv() {
			bool suspend = socket.ReceiveAsync(rcvArgs);
			if(suspend == false) {
				ProcessRcv();
			}
		}

		private void ProcessRcv() {
			if(sessionState != SessionState.Connected) return;
			if(rcvArgs.BytesTransferred > 0 && rcvArgs.SocketError == SocketError.Success) {
				byte[] bytes = new byte[rcvArgs.BytesTransferred];
				Buffer.BlockCopy(rcvArgs.Buffer, 0, bytes, 0, rcvArgs.BytesTransferred);
				readList.AddRange(bytes);
				ProcessByteList();
				StartAsyncRcv();
			}
			else {
				CloseSession();
			}
		}
		private void ProcessSend() {
			if(sendArgs.SocketError == SocketError.Success) {
				isWrite = false;
				if(cacheQueue.Count > 0) {
					byte[] bytes = cacheQueue.Dequeue();
					SendMsg(bytes);
				}
			}
			else {
				CloseSession();
			}
		}
		private void ProcessByteList() {
			while(true) {
				byte[] buffer = IOCPTool.SplitLogic(ref readList);
				if(buffer != null) {
					T msg = IOCPTool.DeSerialize<T>(buffer);
					OnReceiveMsg(msg);
					continue;
				}
				break;
			}
		}

		private void IOCompleted(object sender, SocketAsyncEventArgs eventArgs) {
			switch(eventArgs.LastOperation) {
				case SocketAsyncOperation.Receive:
					ProcessRcv();
					break;
				case SocketAsyncOperation.Send:
					ProcessSend();
					break;
			}
		}

		protected abstract void OnConnected();
		protected abstract void OnReceiveMsg(T msg);
		protected abstract void OnDisconnected();
	}
	public enum SessionState {
		None,
		Connected,
		DisConnected
	}
}