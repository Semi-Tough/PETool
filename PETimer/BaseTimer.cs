using System;

namespace PETool.PETimer {
	public abstract class BaseTimer {
		public Action<string> logFunc;
		public Action<string> wainFunc;
		public Action<string> errorFunc;

		protected int globalTid = 0;
		private readonly DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public abstract int AddTask(uint delay, Action<int> taskCb, Action<int> cancelCb, int count = 1);
		public abstract bool DeleteTask(int tid);
		public abstract void Rest();
		protected abstract int GenerateTid();

		public double GetUtcMilliseconds() {
			TimeSpan ts = DateTime.UtcNow - startDateTime;
			return ts.TotalMilliseconds;
		}
	}
}