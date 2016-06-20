using System;
using System.Linq;
using System.Transactions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Helper.LocalDb.Trigger;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.LocalDbTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class LocalDbTriggerTestInReplication
	{
		public LocalDbTriggerTestInReplication()
		{

		}

		public class DbScope : IDisposable
		{
			public LocalDbReposetory<Users> users;
			public DatabaseScope database;
			public DbScope()
			{
				database = new DatabaseScope();
				users = new LocalDbReposetory<Users>(new DbConfig());
			}

			public void Dispose()
			{
				database.Dispose();
			}
		}

		private DbScope MockRepro()
		{
			return new DbScope();
		}

		[Test]
		public void InsertTriggerWithCancelAfterOrder()
		{
			var orderFlag = false;
			using (var repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Insert += (sender, token) =>
				{
					token.Cancel("AFTER");
				};

				repro.users.Triggers.NotForReplication.For.Insert += (sender, token) =>
				{
					Assert.Fail("Replication misused");
				};
				repro.users.Triggers.NotForReplication.After.Insert += (sender, token) =>
				{
					Assert.Fail("Replication misused");
				};

				repro.database.SetupDone += (sender, args) =>
				{
					Assert.That(orderFlag, Is.False);
					Assert.That(() =>
					{
						repro.users.Add(new Users());
					}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("AFTER"));
					Assert.That(orderFlag, Is.True);
					Assert.That(repro.users.Count, Is.EqualTo(0));
				};
			}
		}

		//[Test]
		//public void DeleteTriggerWithCancelAfterOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};
		//	repro.Triggers.WithReplication.After.Delete += (sender, token) =>
		//	{
		//		token.Cancel("AFTER");
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//	Assert.That(orderFlag, Is.False);
		//	Assert.That(() =>
		//	{
		//		repro.Remove(repro.FirstOrDefault());
		//	}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("AFTER"));
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//}

		//[Test]
		//public void InsertTriggerWithCancelForOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//		token.Cancel("FOR");
		//	};
		//	repro.Triggers.WithReplication.After.Insert += (sender, token) =>
		//	{
		//		Assert.Fail("This should not be called");
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	Assert.That(() =>
		//	{
		//		repro.Add(new Users());
		//	}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("FOR"));
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(0));
		//}

		//[Test]
		//public void DeleteTriggerWithCancelForOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//		token.Cancel("FOR");
		//	};
		//	repro.Triggers.WithReplication.After.Delete += (sender, token) =>
		//	{
		//		Assert.Fail("This should not be called");
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//	Assert.That(orderFlag, Is.False);
		//	Assert.That(() =>
		//	{
		//		repro.Remove(repro.FirstOrDefault());
		//	}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("FOR"));
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//}

		//[Test]
		//public void InsertTriggerOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};
		//	repro.Triggers.WithReplication.After.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//}

		//[Test]
		//public void DeleteTriggerOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};
		//	repro.Triggers.WithReplication.After.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//	Assert.That(orderFlag, Is.False);
		//	repro.Remove(repro.FirstOrDefault());
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(0));
		//}

		//[Test]
		//public void DeleteIOTriggerOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};
		//	repro.Triggers.WithReplication.After.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//	Assert.That(orderFlag, Is.False);
		//	repro.Remove(repro.FirstOrDefault());
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(0));
		//}

		//[Test]
		//public void DeleteIORemoveTriggerOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	var deleted = false;

		//	repro.Triggers.WithReplication.For.Delete += (sender, token) =>
		//	{
		//		if (!deleted)
		//			Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};

		//	repro.Triggers.WithReplication.After.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};

		//	repro.Triggers.WithReplication.InsteadOf.Delete += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//		deleted = true;
		//		Assert.That(token.Table.Contains(token.Item), Is.True);
		//		token.Table.Remove(token.Item);
		//		Assert.That(token.Table.Contains(token.Item), Is.False);
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//	Assert.That(orderFlag, Is.False);
		//	repro.Remove(repro.FirstOrDefault());
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(0));
		//}

		//[Test]
		//public void InsertIOTriggerOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	repro.Triggers.WithReplication.For.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};
		//	repro.Triggers.WithReplication.After.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};
		//	repro.Triggers.WithReplication.InsteadOf.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};
		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(0));
		//}

		//[Test]
		//public void InsertIOReaddTriggerOrder()
		//{
		//	var repro = MockRepro();
		//	var orderFlag = false;
		//	var inserted = false;

		//	repro.Triggers.WithReplication.For.Insert += (sender, token) =>
		//	{
		//		if (!inserted)
		//			Assert.That(orderFlag, Is.False);
		//		orderFlag = true;
		//	};

		//	repro.Triggers.WithReplication.After.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//	};

		//	repro.Triggers.WithReplication.InsteadOf.Insert += (sender, token) =>
		//	{
		//		Assert.That(orderFlag, Is.True);
		//		inserted = true;
		//		using (var tr = new TransactionScope())
		//		{
		//			using (new IdentityInsertScope())
		//			{
		//				token.Table.Add(token.Item);
		//			}
		//			tr.Complete();
		//		}
		//	};

		//	Assert.That(orderFlag, Is.False);
		//	repro.Add(new Users());
		//	Assert.That(orderFlag, Is.True);
		//	Assert.That(repro.Count, Is.EqualTo(1));
		//}


	}
}