/****************************************************
    文件：PEArgs.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/03/07 14:57:48
    功能：运算参数
*****************************************************/

using System;

namespace PETool.PEMath {
	public struct PEArgs {
		public int value;
		public uint multiplier;
		public PEArgs(int value, uint multiplier) {
			this.value = value;
			this.multiplier = multiplier;
		}

		public static readonly PEArgs Zero = new PEArgs(0, 10000);
		public static readonly PEArgs PI = new PEArgs(31416, 10000);

		public static bool operator >(PEArgs a, PEArgs b) {
			if(a.multiplier == b.multiplier) {
				return a.value > b.value;
			}
			else {
				throw new Exception("multiplier is unequal");
			}
		}
		public static bool operator <(PEArgs a, PEArgs b) {
			if(a.multiplier == b.multiplier) {
				return a.value < b.value;
			}
			else {
				throw new Exception("multiplier is unequal");
			}
		}
		public static bool operator >=(PEArgs a, PEArgs b) {
			if(a.multiplier == b.multiplier) {
				return a.value >= b.value;
			}
			else {
				throw new Exception("multiplier is unequal");
			}
		}
		public static bool operator <=(PEArgs a, PEArgs b) {
			if(a.multiplier == b.multiplier) {
				return a.value <= b.value;
			}
			else {
				throw new Exception("multiplier is unequal");
			}
		}
		public static bool operator ==(PEArgs a, PEArgs b) {
			if(a.multiplier == b.multiplier) {
				return a.value == b.value;
			}
			else {
				throw new Exception("multiplier is unequal");
			}
		}
		public static bool operator !=(PEArgs a, PEArgs b) {
			if(a.multiplier == b.multiplier) {
				return a.value != b.value;
			}
			else {
				throw new Exception("multiplier is unequal");
			}
		}

		/// <summary>
		/// 转化为视图角度，不可再用于逻辑运算
		/// </summary>
		/// <returns>角度</returns>
		public int ConvertViewAngle() {
			float rad = ConvertToFloat();
			return(int)Math.Round(rad / Math.PI * 180);
		}

		/// <summary>
		/// 转化为视图弧度，不可再用于逻辑运算
		/// </summary>
		/// <returns>弧度</returns>
		public float ConvertToFloat() {
			return value * 1.0f / multiplier;
		}

		public bool Equals(PEArgs other) {
			return value == other.value && multiplier == other.multiplier;
		}
		public override bool Equals(object obj) {
			return obj is PEArgs other && Equals(other);
		}
		public override int GetHashCode() {
			unchecked {
				return(value * 397) ^ (int)multiplier;
			}
		}
		public override string ToString() {
			return$"value:{value.ToString()} multiplier:{multiplier.ToString()} ";
		}
	}
}