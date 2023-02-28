/****************************************************
    文件：AsyncTimer.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/02/07 23:34:55
    功能：AsyncTimer（主要用于大量并发任务的定时）
    简介：使用async/await异步语法驱动计时，运行在线程池中，支持多线程。
         定时回调可以在驱动线程中运行，也可指定Handle线程运行。
*****************************************************/

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PETool.PETimer {
	public class AsyncTimer : BaseTimer {
		private readonly bool setHandle;
		private const string tidLock = "AsyncTimer_tidLock";
		private readonly ConcurrentQueue<AsyncTaskPack> packQue;
		private readonly ConcurrentDictionary<int, AsyncTask> taskDic;

		public AsyncTimer(bool setHandle) {
			this.setHandle = setHandle;
			taskDic = new ConcurrentDictionary<int, AsyncTask>();
			if(setHandle) {
				packQue = new ConcurrentQueue<AsyncTaskPack>();
			}
		}
		public override int AddTask(uint delay, Action<int> taskCb, Action<int> cancelCb, int count = 1) {
			int tid = GenerateTid();
			AsyncTask task = new AsyncTask(tid, delay, taskCb, cancelCb, count);
			if(taskDic.TryAdd(tid, task)) {
				RunTaskInPool(task);
				return tid;
			}
			else {
				wainFunc?.Invoke($"key:{tid.ToString()} already exist");
				return-1;
			}
		}
		public override bool DeleteTask(int tid) {
			if(taskDic.TryRemove(tid, out AsyncTask task)) {
				if(setHandle && task.cancelCb != null) {
					packQue.Enqueue(new AsyncTaskPack(tid, task.cancelCb));
				}
				else {
					task.cancelCb?.Invoke(tid);
				}
				task.cts.Cancel();
				logFunc?.Invoke($"Remove tid:{tid.ToString()} in taskDic success.");
				return true;
			}
			else {
				wainFunc?.Invoke($"Remove task: {tid.ToString()} in taskDic failed.");
				return false;
			}
		}
		public override void Rest() {
			if(packQue != null) {
				wainFunc?.Invoke($"CallBack is not Empty.");
			}
			taskDic.Clear();
			globalTid = 0;
		}

		/// <summary>
		/// 外部线程处理回调
		/// </summary>
		public void HandleTask() {
			while(packQue != null) {
				if(packQue.TryDequeue(out AsyncTaskPack pack)) {
					pack.cb?.Invoke(pack.tid);
				}
				else {
					wainFunc?.Invoke($"Dequeue task:{pack.tid.ToString()} in packQue failed.");
				}
			}
		}

		private void RunTaskInPool(AsyncTask task) {
			Task.Run(async () => {
				if(task.count > 0) {
					while(task.count > 0) {
						--task.count;
						++task.loopIndex;
						int delay = (int)(task.delay + task.fixDelta);
						if(delay > 0) {
							await Task.Delay(delay, task.ct);
						}
						else {
							wainFunc?.Invoke($"tid:{task.tid.ToString()} delayTime error.");
						}
						TimeSpan ts = DateTime.UtcNow - task.startTime;
						task.fixDelta = (int)(task.delay * task.loopIndex - ts.TotalMilliseconds);
						CallBackTaskCb(task);

						if(task.count == 0) {
							FinishTask(task.tid);
						}
					}
				}
				else {
					while(true) {
						++task.loopIndex;
						int delay = (int)(task.delay + task.fixDelta);
						if(delay > 0) {
							await Task.Delay(delay, task.ct);
						}
						else {
							wainFunc?.Invoke($"tid:{task.tid.ToString()} delayTime error.");
						}
						TimeSpan ts = DateTime.UtcNow - task.startTime;
						task.fixDelta = (int)(task.delay * task.loopIndex - ts.TotalMilliseconds);
						CallBackTaskCb(task);
					}
				}
			}, task.ct);
		}
		private void FinishTask(int tid) {
			if(taskDic.TryRemove(tid, out AsyncTask task)) {
				logFunc?.Invoke($"Task tid:{tid.ToString()} is completion.");
			}
			else {
				wainFunc?.Invoke($"Remove task: {tid.ToString()} in taskDic failed.");
			}
		}
		private void CallBackTaskCb(AsyncTask task) {
			if(setHandle) {
				packQue.Enqueue(new AsyncTaskPack(task.tid, task.taskCb));
			}
			else {
				task.taskCb?.Invoke(task.tid);
			}
		}
		override protected int GenerateTid() {
			lock(tidLock) {
				while(true) {
					++globalTid;
					if(globalTid == int.MaxValue) {
						globalTid = 0;
					}
					if(!taskDic.ContainsKey(globalTid)) {
						return globalTid;
					}
				}
			}
		}
		private class AsyncTask {
			public int tid;
			public uint delay;
			public int count;
			public DateTime startTime;
			public ulong loopIndex;
			public int fixDelta;
			public Action<int> taskCb;
			public Action<int> cancelCb;
			public CancellationTokenSource cts;
			public CancellationToken ct;

			public AsyncTask(int tid, uint delay, Action<int> taskCb, Action<int> cancelCb, int count) {
				this.tid = tid;
				this.delay = delay;
				startTime = DateTime.UtcNow;
				this.taskCb = taskCb;
				this.cancelCb = cancelCb;
				this.count = count;
				loopIndex = 0;
				fixDelta = 0;
				cts = new CancellationTokenSource();
				ct = cts.Token;
			}
		}
		private class AsyncTaskPack {
			public int tid;
			public Action<int> cb;
			public AsyncTaskPack(int tid, Action<int> cb) {
				this.tid = tid;
				this.cb = cb;
			}
		}
	}
}