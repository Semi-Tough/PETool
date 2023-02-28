using System;
using System.Collections.Generic;

namespace PETool.PEMsger {
	public class EventMap<T> where T : IEquatable<T> {
		private readonly Dictionary<T, List<Action<object[]>>> handlerDic
			= new Dictionary<T, List<Action<object[]>>>();

		private readonly Dictionary<object, List<T>> targetDic =
			new Dictionary<object, List<T>>();

		public void Add(T evt, Action<object[]> handler) {
			if(handler.Target == null) return;

			if(!handlerDic.ContainsKey(evt)) {
				handlerDic[evt] = new List<Action<object[]>>();
			}
			List<Action<object[]>> handlerList = handlerDic[evt];
			Action<object[]> action = handlerList.Find(action1 => action1.Equals(handler));
			if(action != null) return;
			handlerList.Add(handler);

			if(!targetDic.ContainsKey(handler.Target)) {
				targetDic[handler.Target] = new List<T>();
			}
			List<T> targetList = targetDic[handler.Target];
			targetList.Add(evt);
		}
		public void RemoveByEvent(T evt) {
			if(handlerDic.ContainsKey(evt)) {
				List<Action<object[]>> handlerList = handlerDic[evt];
				handlerList.ForEach(action => {
					if(action.Target == null || !targetDic.ContainsKey(action.Target)) return;
					List<T> targetList = targetDic[action.Target];
					targetList.RemoveAll(t => t != null && t.Equals(evt));
					if(targetList.Count == 0) {
						targetDic.Remove(action.Target);
					}
				});
			}
			handlerDic.Remove(evt);
		}
		public void RemoveByTarget(object target) {
			if(targetDic.ContainsKey(target)) {
				List<T> targetList = targetDic[target];
				targetList.ForEach(key => {
					if(handlerDic.ContainsKey(key)) {
						List<Action<object[]>> handlerList = handlerDic[key];
						handlerList.RemoveAll(action => action.Target == target);
						if(handlerList.Count == 0) {
							handlerDic.Remove(key);
						}
					}
				});
			}
			targetDic.Remove(target);
		}
		public List<Action<object[]>> GetAllHandler(T evt) {
			handlerDic.TryGetValue(evt, out List<Action<object[]>> list);
			return list;
		}
	}
}