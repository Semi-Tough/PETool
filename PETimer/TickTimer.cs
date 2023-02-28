/****************************************************
    文件：TickTimer.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/02/07 23:28:52
    功能：TickTimer（主要用于高频高精度的毫秒级定时）
    简介：可使用外部循环驱动计时，也可使用单独线程驱动计时，支持多线程。
         定时回调可以在驱动线程中运行，也可指定Handle线程运行。
*****************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace PETool.PETimer {
	public class TickTimer : BaseTimer {
		private readonly bool setHandle;
		private readonly Thread timerThread;
		private const string tidLock = "TickTimer_tidLock";
		private readonly ConcurrentQueue<TickTaskPack> packQue;
		private readonly ConcurrentDictionary<int, TickTask> taskDic;


		public TickTimer(int interval = 0, bool setHandle = true) {
			this.setHandle = setHandle;
			taskDic = new ConcurrentDictionary<int, TickTask>();
			if(setHandle) {
				packQue = new ConcurrentQueue<TickTaskPack>();
			}
			if(interval > 0) {
				void StartTick() {
					try {
						while(true) {
							UpdateTask();
							Thread.Sleep(interval);
						}
					}
					catch(ThreadAbortException e) {
						errorFunc?.Invoke($"Tick Thread Abort: {e}.");
					}
				}
				timerThread = new Thread(StartTick);
				timerThread.Start();
			}
		}
		public override int AddTask(uint delay, Action<int> taskCb, Action<int> cancelCb, int count = 1) {
			int tid = GenerateTid();
			double startTime = GetUtcMilliseconds();
			double destTime = startTime + delay;
			TickTask task = new TickTask(tid, delay, startTime, destTime, taskCb, cancelCb, count);

			if(taskDic.TryAdd(tid, task)) {
				return tid;
			}
			else {
				wainFunc?.Invoke($"key:{tid.ToString()} already exist.");
				return-1;
			}
		}
		public override bool DeleteTask(int tid) {
			if(taskDic.TryRemove(tid, out TickTask task)) {
				if(setHandle && task.cancelCb != null) {
					packQue.Enqueue(new TickTaskPack(tid, task.cancelCb));
				}
				else {
					task.cancelCb?.Invoke(tid);
				}
				logFunc?.Invoke($"Remove tid:{tid.ToString()} in taskDic success.");
				return true;
			}
			else {
				wainFunc?.Invoke($"Remove task: {tid.ToString()} in taskDic failed.");
				return false;
			}
		}
		public override void Rest() {
			if(packQue != null && !packQue.IsEmpty) {
				wainFunc?.Invoke($"CallBack is not Empty.");
			}
			taskDic.Clear();
			globalTid = 0;
			timerThread?.Abort();
		}

		/// <summary>
		/// 外部线程驱动
		/// </summary>
		public void UpdateTask() {
			double nowTime = GetUtcMilliseconds();
			foreach(KeyValuePair<int, TickTask> item in taskDic) {
				TickTask task = item.Value;

				if(nowTime < task.destTime) {
					continue;
				}

				++task.loopIndex;
				if(task.count > 0) {
					--task.count;
					task.destTime = task.startTime + task.delay * (task.loopIndex + 1);
					CallTaskCb(task.tid, task.taskCb);
					if(task.count == 0) {
						FinishTask(task.tid);
					}
				}
				else {
					task.destTime = task.startTime + task.delay * (task.loopIndex + 1);
					CallTaskCb(task.tid, task.taskCb);
				}
			}
		}
		/// <summary>
		/// 外部线程处理回调
		/// </summary>
		public void HandleTask() {
			while(packQue != null) {
				if(packQue.TryDequeue(out TickTaskPack pack)) {
					pack.cb?.Invoke(pack.tid);
				}
				else {
					wainFunc?.Invoke($"Dequeue task:{pack.tid.ToString()} in packQue failed.");
				}
			}
		}

		private void FinishTask(int tid) {
			if(taskDic.TryRemove(tid, out TickTask task)) {
				logFunc?.Invoke($"Task tid:{tid.ToString()} is completion.");
			}
			else {
				wainFunc?.Invoke($"Remove task: {tid.ToString()} in taskDic failed.");
			}
		}
		private void CallTaskCb(int tid, Action<int> taskCb) {
			if(setHandle) {
				packQue.Enqueue(new TickTaskPack(tid, taskCb));
			}
			else {
				taskCb?.Invoke(tid);
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
		private class TickTask {
			public int tid;
			public uint delay;
			public int count;
			public double destTime;
			public double startTime;
			public ulong loopIndex;
			public Action<int> taskCb;
			public Action<int> cancelCb;

			public TickTask(int tid, uint delay, double startTime, double destTime, Action<int> taskCb, Action<int> cancelCb, int count) {
				this.tid = tid;
				this.delay = delay;
				this.startTime = startTime;
				this.destTime = destTime;
				this.taskCb = taskCb;
				this.cancelCb = cancelCb;
				this.count = count;
			}
		}
		private class TickTaskPack {
			public int tid;
			public Action<int> cb;
			public TickTaskPack(int tid, Action<int> cb) {
				this.tid = tid;
				this.cb = cb;
			}
		}
	}
}