using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// Maintains a local collection of entitys simulating a basic DB Bevavior by setting PrimaryKeys in an General way. 
	/// Starting with 0 incriment by 1
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class LocalDbReposetoryBase : ICollection
	{
		private readonly DbConfig _config;
		protected internal readonly object LockRoot = new object();
		protected internal readonly DbClassInfoCache TypeInfo;
		protected internal readonly DbClassInfoCache TypeKeyInfo;
		protected internal readonly DbAccessLayer Db;
		protected internal readonly IDictionary<object, object> Base;
		private readonly LocalDbManager _databaseDatabase;
		protected internal readonly ILocalPrimaryKeyValueProvider KeyGenerator;
		protected internal readonly HashSet<ILocalDbConstraint> Constraints;
		private Transaction _currentTransaction;
		private IdentityInsertScope _currentIdentityInsertScope;
		private readonly List<TransactionalItem> _transactionalItems = new List<TransactionalItem>();
		private bool _isMigrating;
		private readonly bool _keepOriginalObject;

		internal class TransactionalItem
		{
			internal object Item { get; set; }
			internal CollectionStates State { get; set; }

			internal TransactionalItem(object item, CollectionStates state)
			{
				Item = item;
				State = state;
			}
		}

		///  <summary>
		///  Creates a new Instance that is bound to &lt;paramref name="type"/&gt; and uses &lt;paramref name="keyGenerator"/&gt; for generation of PrimaryKeys
		/// 	Must created inside an DatabaseScope
		///  </summary>
		///  <param name="type">The type of an Valid Poco</param>
		///  <param name="keyGenerator">The Strategy to generate an uniqe PrimaryKey that matches the PrimaryKey Property</param>
		///  <param name="config">The Config store to use</param>
		/// <param name="useOrignalObjectInMemory">If enabled the given object referance will be used (Top performance). 
		/// if Disabled each object has to be define an Valid Ado.Net constructor to allow a copy (Can be slow)</param>
		/// <param name="constraints">Additonal Constrains to ensure database like Data Integrity</param>
		protected LocalDbReposetoryBase(Type type,
			ILocalPrimaryKeyValueProvider keyGenerator,
			DbConfig config,
			bool useOrignalObjectInMemory,
			params ILocalDbConstraint[] constraints)
		{
			_keepOriginalObject = useOrignalObjectInMemory;
			_config = config;
			Constraints = new HashSet<ILocalDbConstraint>(constraints);
			_databaseDatabase = LocalDbManager.Scope;
			if (_databaseDatabase == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate" +
				                                " multibe tables in the same Application");
			}

			TypeInfo = _config.GetOrCreateClassInfoCache(type);
			if (TypeInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. " +
				                                              "Type: '{0}'", type.Name));
			}

			TypeKeyInfo = _config.GetOrCreateClassInfoCache(TypeInfo.PrimaryKeyProperty.PropertyType);

			if (TypeKeyInfo == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. " +
				                                              "Type: '{0}'", type.Name));
			}

			if (!TypeKeyInfo.Type.IsValueType)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey that is of " +
				                                              "type of any value type cannot be used. Type: '{0}'", type.Name));
			}

			if (keyGenerator != null)
			{
				KeyGenerator = keyGenerator;
			}
			else
			{
				ILocalPrimaryKeyValueProvider defaultKeyGen;
				if (LocalDbManager.DefaultPkProvider.TryGetValue(TypeKeyInfo.Type, out defaultKeyGen))
				{
					KeyGenerator = defaultKeyGen.Clone() as ILocalPrimaryKeyValueProvider;
				}
				else
				{
					throw new NotSupportedException(
						string.Format("You must specify ether an Primary key that is of one of this types " +
									  "({1}) " +
									  "or invoke the ctor with an proper keyGenerator. " +
						              "Type: '{0}'",
							type.Name,
							LocalDbManager
								.DefaultPkProvider
								.Keys
								.Select(f => f.Name)
								.Aggregate((e, f) => e + "," + f)));
				}
			}
			Base = new ConcurrentDictionary<object, object>();
			_databaseDatabase.AddTable(this);
			_databaseDatabase.SetupDone += DatabaseDatabaseOnSetupDone;
		}

		private void DatabaseDatabaseOnSetupDone(object sender, EventArgs eventArgs)
		{
			foreach (var dbPropertyInfoCach in TypeInfo.Propertys)
			{
				if (dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute != null && dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType != null)
				{
					_databaseDatabase.AddMapping(TypeInfo.Type, dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType);
				}
			}

			ReposetoryCreated = true;
			IsMigrating = false;
		}

		/// <summary>
		/// Returns an value that indicates a proper DatabaseScope usage. 
		/// If true the creation was successfull and all tables for the this table are mapped
		/// The Reposetory cannot operate if the reposetory is not created!
		/// </summary>
		public bool ReposetoryCreated { get; private set; }

		private void CheckCreatedElseThrow()
		{
			if (!ReposetoryCreated && !IsMigrating)
			{
				throw new InvalidOperationException("The database must be completly created until this Table is operational");
			}
		}

		/// <summary>
		/// Creates a new, only local Reposetory by using one of the Predefined KeyGenerators
		/// </summary>
		protected LocalDbReposetoryBase(Type type)
			: this(type, null, new DbConfig(), true)
		{

		}

		/// <summary>
		/// Creates a new, database as fallback using batabase
		/// </summary>
		/// <param name="db"></param>
		/// <param name="type"></param>
		protected LocalDbReposetoryBase(DbAccessLayer db, Type type)
			: this(type)
		{
			Db = db;
		}

		public IEnumerator GetEnumerator()
		{
			CheckCreatedElseThrow();
			if (Db != null)
				return Db.Select(TypeInfo.Type).GetEnumerator();

			return Base.Values.GetEnumerator();
		}

		private object SetNextId(object item)
		{
			var idVal = GetId(item);
			if (IdentityInsertScope.Current != null && this._currentIdentityInsertScope == null)
			{
				this._currentIdentityInsertScope = IdentityInsertScope.Current;
			}

			if (this._currentIdentityInsertScope != null)
			{
				if (idVal.Equals(KeyGenerator.GetUninitilized()) && !this._currentIdentityInsertScope.RewriteDefaultValues)
				{
					return idVal;
				}
				if (!idVal.Equals(KeyGenerator.GetUninitilized()))
				{
					return idVal;
				}
				lock (LockRoot)
				{
					object newId = KeyGenerator.GetNextValue();
					TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, Convert.ChangeType(newId, TypeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			if (idVal.Equals(KeyGenerator.GetUninitilized()))
			{
				lock (LockRoot)
				{
					object newId = KeyGenerator.GetNextValue();
					TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, Convert.ChangeType(newId, TypeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			var exception =
				new InvalidOperationException(string.Format("Cannot insert explicit value for identity column in table '{0}' " +
				                                            "when no IdentityInsertScope exists.", this.TypeInfo.Name));
			throw exception;
		}

		private object GetId(object item)
		{
			var key = TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
			if (key == null)
			{
				var exception = new InvalidOperationException(string.Format("The PrimaryKey value '{0}' is null.", key));
				throw exception;
			}
			if (key.GetType() != this.TypeKeyInfo.Type)
			{
				var exception = new InvalidOperationException(string.Format("The PrimaryKey value '{0}' is invalid.", key));
				throw exception;
			}
			return key;
		}
		
		private ConstraintException CheckEnforceConstraints(object refItem)
		{
			var refTables = _databaseDatabase.GetMappings(TypeInfo.Type);

			foreach (var localDbReposetory in refTables)
			{
				var fkPropForTypeX =
					TypeInfo.Propertys.FirstOrDefault(
						s =>
							s.Value.ForginKeyDeclarationAttribute != null &&
							s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == localDbReposetory.TypeInfo.TableName)
						.Value;

				if (fkPropForTypeX == null)
					continue;

				var fkValueForTableX = fkPropForTypeX.Getter.Invoke(refItem);
				if (fkValueForTableX != null && !localDbReposetory.ContainsId(fkValueForTableX))
				{
					return new ForginKeyConstraintException(
						"ForginKey",
						TypeInfo.TableName,
						localDbReposetory.TypeInfo.TableName,
						fkValueForTableX,
						TypeInfo.PrimaryKeyProperty.PropertyName,
						fkPropForTypeX.PropertyName);
				}
			}

			foreach (var item in Constraints)
			{
				if (!item.CheckConstraint(refItem))
				{
					return new ConstraintException(string.Format("The Constraint '{0}' has detected an invalid object", item.Name));
				}
			}

			return null;
		}

		public virtual bool ContainsId(object fkValueForTableX)
		{
			var local = Base.ContainsKey(fkValueForTableX);
			if (!local)
			{
				//try upcasting
				local = Base.ContainsKey(Convert.ChangeType(fkValueForTableX, TypeInfo.PrimaryKeyProperty.PropertyType));
			}

			if (!local && Db != null)
			{
				return Db.Select(TypeInfo.Type, fkValueForTableX) != null;
			}
			return local;
		}

		private ConstraintException AttachTransactionIfSet(object changedItem, CollectionStates action, bool throwInstant = false)
		{
			if (Transaction.Current != null)
			{
				if (this._currentTransaction == null)
				{
					this._currentTransaction = Transaction.Current;
					this._currentTransaction.TransactionCompleted += _currentTransaction_TransactionCompleted;
				}

				var hasElement = this._transactionalItems.FirstOrDefault(s => s.Item == changedItem);
				if (hasElement != null)
				{
					if (hasElement.State == CollectionStates.Added)
					{
						if (action == CollectionStates.Removed)
						{
							this._transactionalItems.Remove(hasElement);
						}
					}

					if (hasElement.State == CollectionStates.Removed)
					{
						if (action == CollectionStates.Added)
						{
							hasElement.State = CollectionStates.Unchanged;
						}
					}
				}
				else
				{
					this._transactionalItems.Add(new TransactionalItem(changedItem, action));
				}

				return null;
			}
			var ex = this.CheckEnforceConstraints(changedItem);

			if (throwInstant && ex != null)
			{
				throw ex;
			}
			return ex;
		}

		private void _currentTransaction_TransactionCompleted(object sender, TransactionEventArgs e)
		{
			lock (this.LockRoot)
			{
				if (e.Transaction.TransactionInformation.Status == TransactionStatus.Aborted)
				{
					this._currentTransaction_Rollback();
				}
				else
				{
					this._currentTransaction_TransactionCompleted();
				}
			}
		}

		private void _currentTransaction_Rollback()
		{
			foreach (var transactionalItem in _transactionalItems)
			{
				switch (transactionalItem.State)
				{
					case CollectionStates.Unknown:
					case CollectionStates.Unchanged:
					case CollectionStates.Changed:
						break;
					case CollectionStates.Added:
						Base.Remove(transactionalItem.Item);
						break;
					case CollectionStates.Removed:
						Base.Add(GetId(transactionalItem.Item), transactionalItem.Item);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void _currentTransaction_TransactionCompleted()
		{
			lock (this.LockRoot)
			{
				try
				{
					foreach (var transactionalItem in _transactionalItems)
					{
						var checkEnforceConstraints = this.CheckEnforceConstraints(transactionalItem.Item);
						if (checkEnforceConstraints != null)
						{
							try
							{
								throw checkEnforceConstraints;
							}
							finally
							{
								_currentTransaction_Rollback();
							}
						}
					}
				}
				finally
				{
					_transactionalItems.Clear();
				}
			}
		}

		/// <summary>
		/// Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		public virtual void Add(object item)
		{
			var elementToAdd = item;
			CheckCreatedElseThrow();
			if (Db != null)
			{
				Db.Insert(elementToAdd);
			}
			else
			{
				if (!Contains(elementToAdd))
				{
					AttachTransactionIfSet(elementToAdd, 
						CollectionStates.Added, 
						true);
					var id = SetNextId(elementToAdd);
					if (!_keepOriginalObject)
					{
						bool fullyLoaded;
						elementToAdd = DbAccessLayer.CreateInstance(
							TypeInfo, 
							new ObjectDataRecord(item, _config, 0), 
							out fullyLoaded, 
							DbAccessType.Unknown);
						if (!fullyLoaded)
						{
							throw new InvalidOperationException(string.Format("The given type did not provide a Full ado.net constructor " +
							                                                  "and the setting of the propertys did not succeed. " +
							                                                  "Type: '{0}'", item.GetType()));
						}
					}

					Base.Add(id, elementToAdd);
				}
			}
		}

		/// <summary>
		/// Removes all items from this Table
		/// </summary>
		public virtual void Clear()
		{
			CheckCreatedElseThrow();
			lock (this.LockRoot)
			{
				foreach (var item in this.Base)
				{
					Remove(item);
				}
			}
		}

		public virtual bool Contains(object item)
		{
			CheckCreatedElseThrow();
			var pk = GetId(item);
			var local = Base.Contains(new KeyValuePair<object, object>(pk, item));
			if (!local && Db != null)
			{
				return Db.Select(TypeInfo.Type, pk) != null;
			}

			return local;
		}

		/// <summary>
		/// Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Contains(long item)
		{
			return ContainsId(item);
		}

		/// <summary>
		/// Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Contains(int item)
		{
			return ContainsId(item);
		}

		public virtual bool Remove(object item)
		{
			CheckCreatedElseThrow();
			var id = GetId(item);
			bool success;
			lock (this.LockRoot)
			{
				success = Base.Remove(id);
				var hasInvalidOp = AttachTransactionIfSet(item, CollectionStates.Removed);
				if (hasInvalidOp != null)
				{
					Base.Add(id, item);
					throw hasInvalidOp;
				}
			}

			if (!success && Db != null)
			{
				Db.Delete(item);
				success = true;
			}
			return success;
		}

		/// <summary>
		/// Returns an object with the given Primarykey
		/// </summary>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public object this[object primaryKey]
		{
			get
			{
				object value;
				if (Base.TryGetValue(primaryKey, out value))
					return value;
				return null;
			}
		}

		/// <summary>
		/// Thread save
		/// </summary>
		/// <returns></returns>
		public object[] ToArray()
		{
			lock (SyncRoot)
			{
				return Base.Values.ToArray();
			}
		}

		/// <summary>
		/// Thread save
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public virtual void CopyTo(Array array, int index)
		{
			lock (SyncRoot)
			{
				var values = Base.Values.ToArray();
				values.CopyTo(array, index);
			}
		}

		public virtual int Count
		{
			get { return Base.Count; }
		}

		public virtual object SyncRoot
		{
			get { return this.LockRoot; }
		}

		public virtual bool IsSynchronized
		{
			get { return Monitor.IsEntered(LockRoot); }
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public LocalDbManager Database
		{
			get { return _databaseDatabase; }
		}

		internal bool IsMigrating
		{
			get { return _isMigrating; }
			set { _isMigrating = value; }
		}
	}
}