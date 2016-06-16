﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Helper.LocalDb.Trigger;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.LocalDbTests
{
	[TestFixture]
	public class LocalDbTriggerTest
	{
		public LocalDbTriggerTest()
		{

		}

		private LocalDbReposetory<Users> MockRepro()
		{
			LocalDbReposetory<Users> users;
			using (var db = new DatabaseScope())
			{
				users = new LocalDbReposetory<Users>(new DbConfig());
			}
			return users;
		}

		[Test]
		public void InsertTriggerWithCancelAfterOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};
			repro.Triggers.NotForReplication.After.Insert += (sender, token) =>
			{
				token.Cancel("AFTER");
			};
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.Add(new Users());
			}, Throws.Exception.TypeOf<TriggerException>().With.Property("Reason").EqualTo("AFTER"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteTriggerWithCancelAfterOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};
			repro.Triggers.NotForReplication.After.Delete += (sender, token) =>
			{
				token.Cancel("AFTER");
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(repro.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.Remove(repro.FirstOrDefault());
			}, Throws.Exception.TypeOf<TriggerException>().With.Property("Reason").EqualTo("AFTER"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(1));
		}

		[Test]
		public void InsertTriggerWithCancelForOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
				token.Cancel("FOR");
			};
			repro.Triggers.NotForReplication.After.Insert += (sender, token) =>
			{
				Assert.Fail("This should not be called");
			};
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.Add(new Users());
			}, Throws.Exception.TypeOf<TriggerException>().With.Property("Reason").EqualTo("FOR"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteTriggerWithCancelForOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
				token.Cancel("FOR");
			};
			repro.Triggers.NotForReplication.After.Delete += (sender, token) =>
			{
				Assert.Fail("This should not be called");
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(repro.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.Remove(repro.FirstOrDefault());
			}, Throws.Exception.TypeOf<TriggerException>().With.Property("Reason").EqualTo("FOR"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(1));
		}

		[Test]
		public void InsertTriggerOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};
			repro.Triggers.NotForReplication.After.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(1));
		}

		[Test]
		public void DeleteTriggerOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};
			repro.Triggers.NotForReplication.After.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(repro.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			repro.Remove(repro.FirstOrDefault());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteIOTriggerOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};
			repro.Triggers.NotForReplication.After.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(repro.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			repro.Remove(repro.FirstOrDefault());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteIORemoveTriggerOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			var deleted = false;

			repro.Triggers.NotForReplication.For.Delete += (sender, token) =>
			{
				if (!deleted)
					Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};

			repro.Triggers.NotForReplication.After.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};

			repro.Triggers.NotForReplication.InsteadOf.Delete += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
				deleted = true;
				Assert.That(token.Table.Contains(token.Item), Is.True);
				token.Table.Remove(token.Item);
				Assert.That(token.Table.Contains(token.Item), Is.False);
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(repro.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			repro.Remove(repro.FirstOrDefault());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(0));
		}

		[Test]
		public void InsertIOTriggerOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			repro.Triggers.NotForReplication.For.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};
			repro.Triggers.NotForReplication.After.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};
			repro.Triggers.NotForReplication.InsteadOf.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};
			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(0));
		}

		[Test]
		public void InsertIOReaddTriggerOrder()
		{
			var repro = MockRepro();
			var orderFlag = false;
			var inserted = false;
			
			repro.Triggers.NotForReplication.For.Insert += (sender, token) =>
			{
				if (!inserted)
					Assert.That(orderFlag, Is.False);
				orderFlag = true;
			};

			repro.Triggers.NotForReplication.After.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
			};

			repro.Triggers.NotForReplication.InsteadOf.Insert += (sender, token) =>
			{
				Assert.That(orderFlag, Is.True);
				inserted = true;
				using (var tr = new TransactionScope())
				{
					using (new IdentityInsertScope())
					{
						token.Table.Add(token.Item);
					}
					tr.Complete();
				}
			};

			Assert.That(orderFlag, Is.False);
			repro.Add(new Users());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.Count, Is.EqualTo(1));
		}

		
	}
}