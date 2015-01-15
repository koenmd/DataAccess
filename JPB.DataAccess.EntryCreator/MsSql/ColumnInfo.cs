using System;
using System.Data;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntityCreator.MsSql
{
    public class ColumnInfo
    {
        private SqlDbType _targetType2;

        [SelectFactoryMehtod()]
        public static IQueryFactoryResult SelectColumns(string tableName)
        {
            return new QueryFactoryResult("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND TABLE_CATALOG = @database", new[]
            {
                new QueryParameter("@tableName", tableName),
                new QueryParameter("@database", MsSqlCreator.Manager.Database.DatabaseName)
            });
        }

        [ForModel("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [ForModel("ORDINAL_POSITION")]
        public int PositionFromTop { get; set; }

        [ForModel("IS_NULLABLE")]
        [ValueConverter(typeof(NoYesConverter))]
        public bool Nullable { get; set; }

        public Type TargetType { get; set; }

        [ForModel("DATA_TYPE")]
        public SqlDbType TargetType2
        {
            get { return _targetType2; }
            set
            {
                TargetType = DbTypeToCsType.GetClrType(value);
                _targetType2 = value;
            }
        }
    }
}