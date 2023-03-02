/****************************************************
    文件：PEInt.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/03/06 19:56:42
    功能：定点数PEInt
*****************************************************/

using System;

namespace PETool.PEMath {
	public struct PEInt {
		private const int BitMoveCount = 10;
		private const long MultiplyFactor = 1 << BitMoveCount;

		public long ScaleValue { get; }
		public float RawFloat {
			get {
				return ScaleValue * 1.0f / MultiplyFactor;
			}
		}
		public int RawInt {
			get {
				if(ScaleValue >= 0) {
					return(int)(ScaleValue >> BitMoveCount);
				}
				else {
					return-(int)(-ScaleValue >> BitMoveCount);
				}
			}
		}

		private PEInt(long val) {
			ScaleValue = val;
		}
		public PEInt(int val) {
			ScaleValue = val << BitMoveCount;
		}
		public PEInt(float val) {
			ScaleValue = (long)Math.Round(val * MultiplyFactor);
		}

		public static readonly PEInt Zero = new PEInt(0);
		public static readonly PEInt One = new PEInt(1);

		public static PEInt operator -(PEInt val) {
			return new PEInt(-val.ScaleValue);
		}

		public static PEInt operator +(PEInt a, PEInt b) {
			return new PEInt(a.ScaleValue + b.ScaleValue);
		}
		public static PEInt operator -(PEInt a, PEInt b) {
			return new PEInt(a.ScaleValue - b.ScaleValue);
		}
		public static PEInt operator *(PEInt a, PEInt b) {
			long val = (a.ScaleValue * b.ScaleValue) >> BitMoveCount;
			return new PEInt(val);
		}
		public static PEInt operator /(PEInt a, PEInt b) {
			if(b.ScaleValue == 0) {
				throw new DivideByZeroException();
			}
			long val = (a.ScaleValue << BitMoveCount) / b.ScaleValue;
			return new PEInt(val);
		}

		public static bool operator ==(PEInt a, PEInt b) {
			return a.ScaleValue == b.ScaleValue;
		}
		public static bool operator !=(PEInt a, PEInt b) {
			return a.ScaleValue != b.ScaleValue;
		}

		public static bool operator >(PEInt a, PEInt b) {
			return a.ScaleValue > b.ScaleValue;
		}
		public static bool operator <(PEInt a, PEInt b) {
			return a.ScaleValue < b.ScaleValue;
		}

		public static bool operator >=(PEInt a, PEInt b) {
			return a.ScaleValue >= b.ScaleValue;
		}
		public static bool operator <=(PEInt a, PEInt b) {
			return a.ScaleValue <= b.ScaleValue;
		}

		public static PEInt operator >> (PEInt val, int moveCount) {
			if(val.ScaleValue >= 0) {
				return new PEInt(val.ScaleValue >> moveCount);
			}
			else {
				return new PEInt(-(-val.ScaleValue >> moveCount));
			}
		}
		public static PEInt operator <<(PEInt val, int moveCount) {
			return new PEInt(val.ScaleValue << moveCount);
		}

		public static explicit operator PEInt(float f) {
			return new PEInt(f);
		}
		public static implicit operator PEInt(int f) {
			return new PEInt(f);
		}

		public override string ToString() {
			return RawFloat.ToString();
		}
		public bool Equals(PEInt other) {
			return ScaleValue == other.ScaleValue;
		}
		public override bool Equals(object obj) {
			return obj is PEInt other && Equals(other);
		}
		public override int GetHashCode() {
			return ScaleValue.GetHashCode();
		}
	}
}