﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.MetaApi.Contract
{
	public interface IMethodInfoCache<TAtt, TArg> : 
		IComparable<IMethodInfoCache<TAtt, TArg>>,
		IEquatable<IMethodInfoCache<TAtt, TArg>>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		Func<object, object[], object> Delegate { get; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		MethodBase MethodInfo { get; }

		/// <summary>
		///     The name of the method
		/// </summary>
		string MethodName { get; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		HashSet<TAtt> AttributeInfoCaches { get; }

		/// <summary>
		/// Arguments for this Method
		/// </summary>
		HashSet<TArg> Arguments { get; }

		/// <summary>
		/// When set to true, an IL Wrapper is used inside the Invoke method
		/// </summary>
		bool UseILWrapper { get; set; }

		IMethodInfoCache<TAtt, TArg> Init(MethodBase info);
		IMethodInfoCache<TAtt, TArg> Init(MethodBase mehtodInfo, Type sourceType);

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		object Invoke(object target, params object[] param);
	}
}