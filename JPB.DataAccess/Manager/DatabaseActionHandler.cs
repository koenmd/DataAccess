﻿using JPB.DataAccess.DbEventArgs;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A database operation has to be done
	/// </summary>
	public delegate void DatabaseActionHandler(object sender, DatabaseActionEvent e);	
	
	/// <summary>
	///     Indicates the finish of an Event handler (for testing)
	/// </summary>
	public delegate void OnDatabaseActionHandler(object sender, DatabaseActionEvent e, DatabaseActionHandler eventHandler);
}