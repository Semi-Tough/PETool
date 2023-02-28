/****************************************************
    文件：FrameTimer.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/02/07 23:36:52
    功能：FrameTimer（主要用于逻辑帧数的定时）
    简介：只可在单线程当中运行，只能由外部循环驱动计数。
         定时回调在驱动线程中运行
*****************************************************/

using System;
using System.Collections.Generic;

namespace PETool.PETimer {
	public class FrameTimer : BaseTimer {
		private ulong nowFrame;
		private const string tidLock = "TickTimer_tidLock";


		private List<FrameTask> tidAddList;
		private List<int> tidRemoveList;
		private readonly Dictionary<int, FrameTask> taskDic;

		public FrameTimer(ulong frameId = 0) {
			nowFrame = frameId;
			tidAddList = new List<FrameTask>();
			tidRemoveList = new List<int>();
			taskDic = new Dictionary<int, FrameTask>();
		}
		public override int AddTask(uint delay, Action<int> taskCb, Action<int> cancelCb, int count = 1) {
			int tid = GenerateTid();
			ulong destFrame = nowFrame + delay;
			FrameTask task = new FrameTask(tid, delay, destFrame, taskCb, cancelCb, count);
			if(taskDic.ContainsKey(tid)) {
				wainFunc?.Invoke($"key:{tid.ToString()} already exist.");
				return-1;
			}
			else {
				tidAddList.Add(task);
				// taskDic.Add(tid, task);
				return tid;
			}
		}
		public override bool DeleteTask(int tid) {
			if(taskDic.TryGetValue(tid, out FrameTask task)) {
				if(taskDic.Remove(tid)) {
					task.cancelCb?.Invoke(tid);
					logFunc?.Invoke($"Remove tid:{tid.ToString()} in taskDic success.");
					return true;
				}
				else {
					wainFunc?.Invoke($"Remove task: {tid.ToString()} in taskDic failed.");
					return false;
				}
			}
			else {
				wainFunc?.Invoke($" task: {tid.ToString()} is not exist.");
				return false;
			}
		}
		public override void Rest() {
			taskDic.Clear();
			tidRemoveList.Clear();
			globalTid = 0;
		}

		/// <summary>
		/// 外部线程驱动
		/// </summary>
		public void UpdateTask() {
			++nowFrame;
			for(int i = 0; i < tidAddList.Count; i++) {
				taskDic.Add(tidAddList[i].tid, tidAddList[i]);
			}
			tidAddList.Clear();

			foreach(KeyValuePair<int, FrameTask> item in taskDic) {
				FrameTask task = item.Value;
				if(task.destFrame <= nowFrame) {
					task.taskCb?.Invoke(task.tid);
					task.destFrame += task.delay;
					--task.count;
					if(task.count == 0) {
						tidRemoveList.Add(task.tid);
					}
				}
			}


			for(int i = 0; i < tidRemoveList.Count; i++) {
				if(taskDic.Remove(tidRemoveList[i])) {
					logFunc?.Invoke($"Task tid:{tidRemoveList[i].ToString()} is completion.");
				}
				else {
					wainFunc?.Invoke($"Remove task: {tidRemoveList[i].ToString()} in taskDic failed.");
				}
			}
			tidRemoveList.Clear();
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
		private class FrameTask {
			public int tid;
			public uint delay;
			public int count;
			public ulong destFrame;
			public Action<int> taskCb;
			public Action<int> cancelCb;

			public FrameTask(int tid, uint delay, ulong destFrame, Action<int> taskCb, Action<int> cancelCb, int count) {
				this.tid = tid;
				this.delay = delay;
				this.destFrame = destFrame;
				this.taskCb = taskCb;
				this.cancelCb = cancelCb;
				this.count = count;
			}
		}
	}
}