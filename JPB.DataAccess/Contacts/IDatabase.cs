/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Contacts
{
	/// <summary>
	///     Database wrapper interface
	/// </summary>
	public interface IDatabase : IDisposable
	{
		/// <summary>
		///     Defines the Target database we are conneting to
		/// </summary>
		DbAccessType TargetDatabase { get; }

		/// <summary>
		///     NotImp
		/// </summary>
		bool IsAttached { get; }

		/// <summary>
		///     Get the Current Connection string
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		///     If local instance get the file
		/// </summary>
		string DatabaseFile { get; }

		/// <summary>
		///     Get the Database name that we are connected to
		/// </summary>
		string DatabaseName { get; }

		/// <summary>
		///     Get the Server we are Connected to
		/// </summary>
		string ServerName { get; }

		/// <summary>
		///     Get the last Executed QueryCommand wrapped by a Debugger
		/// </summary>
		QueryDebugger LastExecutedQuery { get; }

		/// <summary>
		///     Get Database specific Datapager
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IDataPager<T> CreatePager<T>();

		/// <summary>
		///     Get database specific converter Datapager
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TE"></typeparam>
		/// <returns></returns>
		IWrapperDataPager<T, TE> CreatePager<T, TE>();

		/// <summary>
		///     Required
		///     Is used to attach a Strategy that handles certain kinds of Databases
		/// </summary>
		void Attach(IDatabaseStrategy strategy);

		void Detach();

		/// <summary>
		///     Required
		///     Is used to create an new Connection based on the Strategy and
		///     keep it
		/// </summary>
		/// <returns></returns>
		IDbConnection GetConnection();

		/// <summary>
		///     Required
		///     Is used to create an new Transaction based on the Strategy
		/// </summary>
		/// <returns></returns>
		IDbTransaction GetTransaction();

		/// <summary>
		///     Required
		///     When a new Connection is requested this function is used
		/// </summary>
		void Connect(IsolationLevel? levl = null);

		void TransactionRollback();

		/// <summary>
		///     Required
		///     Closing a open Connection
		/// </summary>
		void CloseConnection();

		/// <summary>
		///     Required
		///     Closing all Connections that maybe open
		/// </summary>
		void CloseAllConnection();

		int ExecuteNonQuery(string strSql, params object[] obj);
		int ExecuteNonQuery(IDbCommand cmd);

		/// <summary>
		///     Required
		///     Return the last inserted id based on the Strategy
		/// </summary>
		/// <returns></returns>
		object GetlastInsertedID();

		IDataReader GetDataReader(string strSql, params object[] obj);

		object GetSkalar(IDbCommand cmd);
		object GetSkalar(string strSql, params object[] obj);

		DataTable GetDataTable(string name, string strSql);
		DataSet GetDataSet(string strSql);

		/// <summary>
		///     Required
		///     Creates a Command based on the Strategy
		/// </summary>
		/// <returns></returns>
		IDbCommand CreateCommand(string strSql, params IDataParameter[] fields);

		/// <summary>
		///     Required
		///     Creates a Parameter based on the Strategy
		/// </summary>
		/// <returns></returns>
		IDataParameter CreateParameter(string strName, object value);

		/// <summary>
		///     Required
		///     Execute a QueryCommand and map the result that is created with the func
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<T> GetEntitiesList<T>(string strQuery, Func<IDataRecord, T> func, bool bHandleConnection);

		/// <summary>
		///     Required
		///     Execute a QueryCommand and map the result that is created with the func
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<T> GetEntitiesList<T>(IDbCommand cmd, Func<IDataRecord, T> func);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		void Run(Action<IDatabase> action);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		T Run<T>(Func<IDatabase, T> func);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		void RunInTransaction(Action<IDatabase> action);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		void RunInTransaction(Action<IDatabase> action, IsolationLevel transaction);

		/// <summary>
		///     Required
		///     Opens a Connection or reuse an existing one and then execute the action
		/// </summary>
		T RunInTransaction<T>(Func<IDatabase, T> func);

		IDatabase Clone();
		IDbCommand GetlastInsertedIdCommand();

		/// <summary>
		///     Formarts a Command to a executable QueryCommand
		/// </summary>
		/// <returns></returns>
		string FormartCommandToQuery(IDbCommand comm);

		/// <summary>
		///     Converts the Generic SourceDbType to the Specific represntation
		/// </summary>
		/// <returns></returns>
		string ConvertParameter(DbType type);
	}
}