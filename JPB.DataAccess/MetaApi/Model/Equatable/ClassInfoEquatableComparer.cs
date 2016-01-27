﻿using System.Collections.Generic;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model.Equatable
{
	internal class ClassInfoEquatableComparer
		: IEqualityComparer<IClassInfoCache>, 
		IComparer<IClassInfoCache>
	{
		public bool Equals(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			if (x.ClassName != y.ClassName)
				return false;
			if (x.Type == y.Type)
				return false;
			return true;
		}

		public int GetHashCode(IClassInfoCache obj)
		{
			return obj.Type.GetHashCode();
		}

		public int Compare(IClassInfoCache x, IClassInfoCache y)
		{
			if (x == null)
				return -1;
			if (y == null)
				return +1;
			return System.String.Compare(x.ClassName, y.ClassName, System.StringComparison.Ordinal);
		}
	}
}