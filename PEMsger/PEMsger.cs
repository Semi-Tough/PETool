using System;
using System.Collections.Generic;

namespace PETool.PEMsger {
	public class PEMsger<T> where T : IEquatable<T> {
		private const string EvtLock = "PEEventLock";
		private readonly EventMap<T> evtMap = new EventMap<T>();

		public void AddMsg(T evt, Action<object[]> handler) {
			lock(EvtLock) {
				evtMap.Add(evt, handler);
			}
		}
		public void RemoveByEvent(T evt) {
			lock(EvtLock) {
				evtMap.RemoveByEvent(evt);
			}
		}
		public void RemoveByTarget(object target) {
			lock(EvtLock) {
				evtMap.RemoveByTarget(target);
			}
		}
		public void InvokeMsg(T evt, params object[] args) {
			lock(EvtLock) {
				List<Action<object[]>> evtList = evtMap.GetAllHandler(evt);
				evtList.ForEach(action => {
					action.Invoke(args);
				});
			}
		}
	}
}