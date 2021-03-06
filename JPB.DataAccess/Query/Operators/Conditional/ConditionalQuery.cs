﻿#region

using System;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	///     Creates an Conditional Query that allows you to filter the Previus query
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class ConditionalQuery<TPoco> : QueryBuilderX, IConditionalQuery<TPoco>
	{
		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public readonly CondtionBuilderState State;

		/// <summary>
		///     Creates a new Instance based on the previus query
		/// </summary>
		/// <param name="queryText"></param>
		/// <param name="state"></param>
		public ConditionalQuery(IQueryBuilder queryText, CondtionBuilderState state) : base(queryText)
		{
			State = state;
		}

		/// <summary>
		///     Creates a new Instance based on the previus query
		/// </summary>
		/// <param name="queryText"></param>
		public ConditionalQuery(ConditionalQuery<TPoco> queryText) : base(queryText)
		{
			State = queryText.State;
		}

		/// <summary>
		///     Opens a new Logical combined Query
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Parenthesis
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("("), State.ToInBreaket(true)); }
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public string CurrentIdentifier
		{
			get { return State.Identifier; }
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Alias(string alias)
		{
			return new ConditionalQuery<TPoco>(this, State.ToAlias(alias));
		}

		/// <summary>
		///		Selects the current PrimaryKey
		/// </summary>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> PrimaryKey()
		{
			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return Column(tCache.PrimaryKeyProperty.DbName);
		}

		/// <summary>
		///		Selects the ForginKey to the table.
		/// </summary>
		/// <exception cref="InvalidOperationException">If there are 0 or more then 1 forginKeys</exception>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> ForginKey<TFkPoco>()
		{
			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			var tProp = tCache.Propertys.Values
			                  .Single(e =>
				                  e.ForginKeyDeclarationAttribute != null &&
				                  e.ForginKeyDeclarationAttribute.Attribute.ForeignType == typeof(TFkPoco));
			return Column(tProp.DbName);
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column(string columnName)
		{
			return new ConditionalColumnQuery<TPoco>(this.QueryText(columnName), State);
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column<TA>(
			Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLamdba();
			var propName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			if (CurrentIdentifier != null)
			{
				return Column(CurrentIdentifier + "." + propName.DbName);
			}
			return Column(propName.DbName);
		}
	}
}