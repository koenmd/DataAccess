﻿/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
	/// <summary>
	///     Marker interface for an Query that was created due the invoke of a Factory mehtod
	/// </summary>
	public interface IQueryFactoryResult
	{
		/// <summary>
		///     The SQL Query
		/// </summary>
		string Query { get; }

		/// <summary>
		///     Sql Query Parameter
		/// </summary>
		IEnumerable<IQueryParameter> Parameters { get; }
	}
}