using System;

namespace PETool.AsyncNet {
	[Serializable]
	public abstract class AsyncMsg {
	};

	public class AsyncMsgPack {
		public const int HeadLength = 4;
		public readonly byte[] headBuffer;
		public int headIndex;

		public int bodyLength;
		public byte[] bodyBuffer;
		public int bodyIndex;

		public AsyncMsgPack() {
			headBuffer = new byte[4];
		}
		public void InitBodyBuffer() {
			bodyLength = BitConverter.ToInt32(headBuffer, 0);
			bodyBuffer = new byte[bodyLength];
		}
		public void ResetData() {
			headIndex = 0;
			bodyLength = 0;
			bodyIndex = 0;
			bodyBuffer = null;
		}
	}
}