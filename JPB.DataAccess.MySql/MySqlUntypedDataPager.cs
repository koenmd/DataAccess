﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.MySql
{
	public class MySqlUntypedDataPager<T> : IDataPager<T>
	{
		private bool _cache;
		private long _currentPage;

		private Action<Action> _syncHelper;

		public MySqlUntypedDataPager()
		{
			CurrentPage = 0;
			PageSize = 10;

			FirstID = -1;
			LastID = -1;
			SyncHelper = action => action();
		}

		public bool Cache
		{
			get { return _cache; }
			set
			{
				if (value)
				{
					throw new Exception("To be supported ... sory");
				}

				_cache = value;
			}
		}

		public bool RaiseEvents { get; set; }
		public List<IDbCommand> AppendedComands { get; set; }

		public long FirstID { get; private set; }
		public long LastID { get; private set; }

		public long CurrentPage
		{
			get { return _currentPage; }
			set
			{
				if (value >= 0)
				{
					_currentPage = value;
				}
			}
		}

		public long MaxPage { get; private set; }

		public int PageSize { get; set; }

		public Type TargetType { get; set; }

		IEnumerable IDataPager.CurrentPageItems
		{
			get { return CurrentPageItems; }
		}

		public IDbCommand BaseQuery { get; set; }

		public virtual ICollection<T> CurrentPageItems { get; protected set; }

		public Action<Action> SyncHelper
		{
			get { return _syncHelper; }
			set
			{
				if (value != null)
				{
					_syncHelper = value;
				}
			}
		}

		public long TotalItemCount
		{
			get { throw new NotImplementedException(); }
		}

		public event Action NewPageLoading;
		public event Action NewPageLoaded;

		private void RaiseNewPageLoading()
		{
			var handler = NewPageLoading;
			if (handler != null)
			{
				handler();
			}
		}

		private void RaiseNewPageLoaded()
		{
			var handler = NewPageLoaded;
			if (handler != null)
			{
				handler();
			}
		}

		public virtual void LoadPage(DbAccessLayer dbAccess)
		{
			throw new NotImplementedException();
			//		RaiseNewPageLoading();
			//		SyncHelper(() => CurrentPageItems.Clear());

			//		var pk = TargetType.GetPK();

			//		var targetQuery = BaseQuery;
			//		if (targetQuery == null)
			//		{
			//			targetQuery = dbAccess.Database.CreateCommand(TargetType.GetClassInfo().TableName);
			//		}

			//		IDbCommand FirstIdCommand = targetQuery;
			//		if (AppendedComands.Any())
			//		{
			//			FirstIdCommand = this.AppendedComands.Aggregate(FirstIdCommand,
			//				(e, f) => dbAccess.ConcatCommands(dbAccess.Database, e, f));
			//		}

			//		if (FirstID == -1 || LastID == -1)
			//		{
			//			var firstOrDefault = dbAccess.RunPrimetivSelect(typeof(long),
			//				dbAccess.Create(dbAccess.Database,
			//					("SELECT " + pk + " FROM ( {0} ) ORDER BY " + pk + " LIMIT 1").CreateCommand(dbAccess.Database), FirstIdCommand)).FirstOrDefault();
			//			if (firstOrDefault != null)
			//				FirstID = (long)firstOrDefault;

			//			var lastId = dbAccess.RunPrimetivSelect(typeof(long),
			//				dbAccess.InsertCommands(dbAccess.Database,
			//				("SELECT " + pk + " FROM ( {0} ) ORDER BY " + pk + " DESC LIMIT 1").CreateCommand(dbAccess.Database), FirstIdCommand)).FirstOrDefault();
			//			if (lastId != null)
			//				LastID = (long)lastId;
			//		}

			//		var maxItems = dbAccess.RunPrimetivSelect(typeof(long),
			//DbAccessLayer.InsertCommands(dbAccess.Database,
			//("SELECT COUNT( * ) AS NR FROM {0}").CreateCommand(dbAccess.Database), FirstIdCommand)).FirstOrDefault();

			//		if (maxItems != null)
			//		{
			//			long parsedCount;
			//			long.TryParse(maxItems.ToString(), out parsedCount);
			//			MaxPage = ((long)parsedCount) / PageSize;
			//		}

			//		var realSelect = DbAccessLayer.InsertCommands(dbAccess.Database,
			//			("SELECT * FROM {0}").CreateCommand(dbAccess.Database), FirstIdCommand);

			//		var selectWhere = dbAccess.SelectNative(this.TargetType, realSelect, new
			//		{
			//			PagedRows = CurrentPage * PageSize,
			//			PageSize
			//		});

			//		//var selectWhere = dbAccess.SelectWhere(TargetType, " ORDER BY " + pk + " ASC LIMIT @PagedRows, @PageSize", new
			//		//{
			//		//    PagedRows = CurrentPage * PageSize,
			//		//    PageSize
			//		//});

			//		foreach (var item in selectWhere)
			//		{
			//			dynamic item1 = item;
			//			SyncHelper(() => CurrentPageItems.Add(item1));
			//		}

			//		RaiseNewPageLoaded();
		}

		public void Dispose()
		{
			BaseQuery.Dispose();
			CurrentPageItems.Clear();
		}
	}
}