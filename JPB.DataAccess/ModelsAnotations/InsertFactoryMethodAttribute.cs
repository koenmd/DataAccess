﻿using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Marks a mehtod as an Factory method
	///     The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class InsertFactoryMethodAttribute : DbAccessTypeAttribute
	{
	}
}