﻿#region

using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Selection;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///     Defines the root for every Query
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryContainer database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public DatabaseObjectSelector Select
		{
			get { return new DatabaseObjectSelector(this); }
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public CountElementsObjectSelector Count
		{
			get { return new CountElementsObjectSelector(this); }
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Execute<T>(params object[] argumentsForFactory)
		{
			var cmd = ContainerObject
				.AccessLayer
				.CreateSelectQueryFactory(
					ContainerObject.AccessLayer.GetClassInfo(typeof(T)), argumentsForFactory);
			return new SelectQuery<T>(this.QueryCommand(cmd));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> SelectFactory<T>(params object[] argumentsForFactory)
		{
			return new DatabaseObjectSelector(this).Table<T>(argumentsForFactory);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Distinct<T>()
		{
			var cmd = DbAccessLayer.CreateSelect(ContainerObject.AccessLayer.GetClassInfo(typeof(T)), "DISTINCT");
			return new SelectQuery<T>(this.QueryText(cmd));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public UpdateQuery<T> UpdateDirect<T>(T obj)
		{
			return new UpdateQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateUpdate(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public UpdateQuery<T> Update<T>(T obj)
		{
			return new UpdateQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateUpdateSimple(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}

		/// <summary>
		///     Adds a Delete - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public DeleteQuery<T> Delete<T>(T obj)
		{
			return new DeleteQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateDelete(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public DeleteQuery<T> Delete<T>()
		{
			return new DeleteQuery<T>(this
				.QueryCommand(
					DbAccessLayer
						.CreateDelete(ContainerObject
							.AccessLayer.Database, ContainerObject.AccessLayer.GetClassInfo(typeof(T)))));
		}
	}
}