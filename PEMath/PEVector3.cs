/****************************************************
    文件：PEVector3.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/03/06 21:20:02
    功能：定点数Vector3
*****************************************************/

#if UNITY_ENV
using UnityEngine;
#endif

namespace PETool.PEMath {
	public struct PEVector3 {
		public PEInt x;
		public PEInt y;
		public PEInt z;

		public PEVector3(PEInt x, PEInt y, PEInt z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}
#if UNITY_ENV
		public PEVector3(Vector3 v) {
			this.x = (PEInt)v.x;
			this.y = (PEInt)v.y;
			this.z = (PEInt)v.z;
		}
#endif

		public PEInt this[int index] {
			get {
				switch(index) {
					case 0:
						return x;
					case 1:
						return y;
					case 2:
						return z;
				}
				return 0;
			}
			set {
				switch(index) {
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					case 2:
						z = value;
						break;
				}
			}
		}


		public static PEVector3 Zero {
			get {
				return new PEVector3(0, 0, 0);
			}
		}
		public static PEVector3 One {
			get {
				return new PEVector3(1, 1, 1);
			}
		}
		public static PEVector3 Forward {
			get {
				return new PEVector3(0, 0, 1);
			}
		}
		public static PEVector3 Back {
			get {
				return new PEVector3(0, 0, -1);
			}
		}
		public static PEVector3 Right {
			get {
				return new PEVector3(1, 0, 0);
			}
		}
		public static PEVector3 Left {
			get {
				return new PEVector3(-1, 0, 0);
			}
		}
		public static PEVector3 Up {
			get {
				return new PEVector3(0, 1, 0);
			}
		}
		public static PEVector3 Down {
			get {
				return new PEVector3(0, -1, 0);
			}
		}

		public static PEVector3 operator -(PEVector3 v1) {
			return new PEVector3(-v1.x, -v1.y, -v1.z);
		}

		public static PEVector3 operator +(PEVector3 v1, PEVector3 v2) {
			return new PEVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		}
		public static PEVector3 operator -(PEVector3 v1, PEVector3 v2) {
			return new PEVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		}
		public static PEVector3 operator *(PEVector3 v1, PEInt val) {
			return new PEVector3(v1.x * val, v1.y * val, v1.z * val);
		}
		public static PEVector3 operator *(PEInt val, PEVector3 v1) {
			return new PEVector3(v1.x * val, v1.y * val, v1.z * val);
		}
		public static PEVector3 operator /(PEVector3 v1, PEInt val) {
			return new PEVector3(v1.x / val, v1.y / val, v1.z / val);
		}

		public static bool operator ==(PEVector3 v1, PEVector3 v2) {
			return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
		}
		public static bool operator !=(PEVector3 v1, PEVector3 v2) {
			return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
		}

		public long[] ConvertLongArray() {
			return new long[]{ x.ScaleValue, y.ScaleValue, z.ScaleValue };
		}
#if UNITY_ENV
		/// <summary>
		/// 转化为Vector3，不可再用于逻辑运算
		/// </summary>
		/// <returns>Vector3</returns>
		public Vector3 ConvertViewVector3() {
			return new Vector3(x.RawFloat, y.RawFloat, z.RawFloat);
		}
#endif

		public PEInt sqrMagnitude {
			get {
				return x * x + y * y + z * z;
			}
		}
		public static PEInt SqrMagnitude(PEVector3 v) {
			return v.x * v.x + v.y * v.y + v.z * v.z;
		}
		public PEInt magnitude {
			get {
				return PECalculate.Sqrt(sqrMagnitude);
			}
		}

		public PEVector3 normalized {
			get {
				if(magnitude > 0) {
					PEInt rate = PEInt.One / magnitude;
					return new PEVector3(x * rate, y * rate, z * rate);
				}
				else {
					return Zero;
				}
			}
		}
		public void Normalized() {
			if(magnitude <= 0) return;
			PEInt rate = PEInt.One / magnitude;
			x = x * rate;
			y = y * rate;
			z = z * rate;
		}
		public static PEVector3 Normalized(PEVector3 v) {
			if(v.magnitude > 0) {
				PEInt rate = PEInt.One / v.magnitude;
				return new PEVector3(v.x * rate, v.y * rate, v.z * rate);
			}
			else {
				return Zero;
			}
		}

		public static PEInt Dot(PEVector3 v1, PEVector3 v2) {
			return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
		}

		public static PEVector3 Cross(PEVector3 v1, PEVector3 v2) {
			return new PEVector3(
				v1.y * v2.z - v1.z * v2.y,
				v1.z * v2.x - v1.x * v2.z,
				v1.x * v2.y - v1.y * v2.x
			);
		}

		public static PEArgs Angle(PEVector3 from, PEVector3 to) {
			PEInt dot = Dot(from, to);
			PEInt mod = PECalculate.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
			if(mod == 0) return PEArgs.Zero;
			return PECalculate.ACos(dot / mod);
		}

		public bool Equals(PEVector3 other) {
			return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
		}
		public override bool Equals(object obj) {
			return obj is PEVector3 other && Equals(other);
		}
		public override int GetHashCode() {
			unchecked {
				int hashCode = x.GetHashCode();
				hashCode = (hashCode * 397) ^ y.GetHashCode();
				hashCode = (hashCode * 397) ^ z.GetHashCode();
				return hashCode;
			}
		}
		public override string ToString() {
			return$"x:{x.ToString()} y:{y.ToString()} z:{z.ToString()}";
		}
	}
}