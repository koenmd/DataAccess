﻿#region

using System.Collections.Generic;

#endregion

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///     Provides access to the interal query storage and enumeration Function
	/// </summary>
	/// <typeparam name="Stack"></typeparam>
	public interface IQueryBuilder<Stack>
		where Stack : IQueryElement
	{
		/// <summary>
		///     The interal value holder
		/// </summary>
		IQueryContainer ContainerObject { get; }

		/// <summary>
		///     Enumerates the current query for a type <typeparamref name="E" />
		/// </summary>
		/// <typeparam name="E"></typeparam>
		/// <returns></returns>
		IEnumerable<E> ForResult<E>();

		/// <summary>
		///     Wraps this query type to an new QueryElement
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IQueryBuilder<T> ChangeType<T>() where T : IQueryElement;
	}
}