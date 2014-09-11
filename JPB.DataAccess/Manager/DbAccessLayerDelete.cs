﻿using System;
using System.Data;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
    partial class DbAccessLayer
    {
        public void Delete<T>(T entry)
        {
            Delete<T>(entry, Database);
        }

        internal static IDbCommand CreateDelete<T>(T entry, IDatabase db)
        {
            Type type = typeof(T);
            string proppk = type.GetPK();
            string query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @1";
            return CreateCommandWithParameterValues(query, db, new object[] { entry.GetPK<T, long>() });
        }

        internal static void _Delete<T>(T entry, IDatabase db)
        {
            db.Run(s =>
            {
                s.ExecuteNonQuery(CreateDelete(entry, s));
            });
        }

        public static void Delete<T>(T entry, IDatabase db)
        {
            CheckInstanceForAttriute<T, DeleteFactoryMethodAttribute>(entry, db, CreateDelete);
        }
    }
}