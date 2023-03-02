using System;
using System.Collections.Generic;

namespace PETool.PEQueue {
	public class PEQueue<T> where T : IComparable<T> {
		private readonly List<T> list;
		public int Count { get { return list.Count; } }
		public int Capacity { get { return list.Capacity; } }
		public bool IsEmpty { get { return list.Count == 0; } }

		public PEQueue(int capacity = 4) {
			list = new List<T>(capacity);
		}


		public int IndexOf(T t) {
			return list.IndexOf(t);
		}
		public T RemoveAt(int index) {
			if(list.Count <= index) return default(T);
			T value = list[index];
			int endIndex = list.Count - 1;
			list[index] = list[endIndex];
			list.RemoveAt(endIndex);
			--endIndex;

			if(index < endIndex) {
				int parentIndex = (index - 1) / 2;
				if(parentIndex > 0 && list[index].CompareTo(list[parentIndex]) < 0) {
					HeapifyUp(index);
				}
				else {
					HeapifyDown(index, endIndex);
				}
			}

			return value;
		}
		public T Remove(T t) {
			int index = IndexOf(t);
			return index == -1 ? default(T) : RemoveAt(index);
		}
		public T Dequeue() {
			if(list.Count == 0) return default(T);

			T value = list[0];
			int endIndex = list.Count - 1;
			list[0] = list[endIndex];
			list.RemoveAt(endIndex);
			--endIndex;
			HeapifyDown(0, endIndex);
			return value;
		}

		public void Enqueue(T t) {
			list.Add(t);
			HeapifyUp(list.Count - 1);
		}
		public T Peep() {
			return list.Count > 0 ? list[0] : default(T);
		}
		public bool Contains(T t) {
			return list.Contains(t);
		}
		public List<T> ToList() {
			return list;
		}
		public T[] ToArray() {
			return list.ToArray();
		}
		public void Clear() {
			list.Clear();
		}

		private void HeapifyDown(int topIndex, int endIndex) {
			while(true) {
				int minIndex = topIndex;
				//Left
				int childIndex = topIndex * 2 + 1;
				if(childIndex <= endIndex && list[childIndex].CompareTo(list[topIndex]) < 0) {
					minIndex = childIndex;
				}

				//Right
				childIndex = topIndex * 2 + 2;
				if(childIndex <= endIndex && list[childIndex].CompareTo(list[minIndex]) < 0) {
					minIndex = childIndex;
				}

				if(topIndex == minIndex) break;
				Swap(topIndex, minIndex);
				topIndex = minIndex;
			}
		}
		private void HeapifyUp(int index) {
			int parentIndex = (index - 1) / 2;
			while(index > 0 && list[index].CompareTo(list[parentIndex]) < 0) {
				Swap(index, parentIndex);
				index = parentIndex;
				parentIndex = (index - 1) / 2;
			}
		}
		private void Swap(int a, int b) {
			(list[a], list[b]) = (list[b], list[a]);
		}
	}
}