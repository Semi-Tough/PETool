/****************************************************
    文件：PECalculate.cs
    作者：Semi-Tough
    邮箱：1693416984@qq.com
    日期：2023/03/07 12:19:36
    功能：定点数常用数学运算
*****************************************************/

using System;

namespace PETool.PEMath {
	public static class PECalculate {
		public static PEInt Sqrt(PEInt val, int iteratorCount = 8) {
			if(val == PEInt.Zero) return 0;
			if(val < PEInt.Zero) throw new Exception();
			PEInt result = val;
			PEInt history;
			int count = 0;
			do {
				history = result;
				result = (result + val / result) >> 1;
				count++;
			}
			while(history != result && count < iteratorCount);
			return result;
		}

		public static PEArgs ACos(PEInt val) {
			PEInt rate = (val * AcosTable.HalfIndexCount) + AcosTable.HalfIndexCount;
			rate = Clamp(rate, PEInt.Zero, AcosTable.IndexCount);
			return new PEArgs(AcosTable.Table[rate.RawInt], AcosTable.Multiplier);
		}

		public static PEInt Clamp(PEInt val, PEInt min, PEInt max) {
			if(val < min) {
				val = min;
			}
			if(val > max) {
				val = max;
			}
			return val;
		}
	}
}