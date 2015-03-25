﻿using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace UnitTestProject1
{
    public class UsersMeta
    {
        public const string UserTable = "Users";
        public const string SelectStatement = "SELECT * FROM " + UserTable;
        public const string UserIDCol = "User_ID";
        public const string UserNameCol = "UserName";
    }

    [ForModel(UsersMeta.UserTable)]
    public class Users_Col : INotifyPropertyChanged
    {
        private long _userId;
        private string _userName;
        public event PropertyChangedEventHandler PropertyChanged;

        [JPB.DataAccess.UnitTests.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public long User_ID
        {
            get { return _userId; }
            set
            {
                if (value == _userId) return;
                _userId = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged();
            }
        }
    }

    public class Users
    {
        public long User_ID { get; set; }
        public string UserName { get; set; }
    }

    [ForModel(UsersMeta.UserTable)]
    public class Users_PK
    {
        [PrimaryKey]
        public long User_ID { get; set; }
        public string UserName { get; set; }
    }

    [ForModel(UsersMeta.UserTable)]
    public class Users_PK_UFM
    {
        [PrimaryKey]
        public long User_ID { get; set; }
        [ForModel(UsersMeta.UserNameCol)]
        public string UserName { get; set; }
    }

    [ForModel(UsersMeta.UserTable)]
    public class Users_PK_IDFM
    {
        [PrimaryKey]
        [ForModel(UsersMeta.UserIDCol)]
        public long UserId { get; set; }
        public string UserName { get; set; }
    }

    [ForModel(UsersMeta.UserTable)]
    public class Users_PK_IDFM_CTORSEL
    {
        public Users_PK_IDFM_CTORSEL(IDataRecord rec)
        {
            UserName = (string)rec[UsersMeta.UserNameCol];
            UserId = (long)rec[UsersMeta.UserIDCol];
        }

        [PrimaryKey]
        [ForModel(UsersMeta.UserIDCol)]
        public long UserId { get; set; }
        public string UserName { get; set; }
    }

    [ForModel(UsersMeta.UserTable)]
    [SelectFactory(UsersMeta.SelectStatement)]
    public class Users_PK_IDFM_CLASSEL
    {
        [PrimaryKey]
        [ForModel(UsersMeta.UserIDCol)]
        public long UserId { get; set; }
        public string UserName { get; set; }
    }

    [ForModel(UsersMeta.UserTable)]
    [SelectFactory(UsersMeta.SelectStatement)]
    public class Users_PK_IDFM_FUNCSELECT
    {
        [PrimaryKey]
        [ForModel(UsersMeta.UserIDCol)]
        public long UserId { get; set; }
        public string UserName { get; set; }

        [SelectFactoryMehtod]
        public static string GetSelectStatement()
        {
            return UsersMeta.SelectStatement;
        }
    }

    [ForModel(UsersMeta.UserTable)]
    [SelectFactory(UsersMeta.SelectStatement)]
    public class Users_PK_IDFM_FUNCSELECTFAC
    {
        [PrimaryKey]
        [ForModel(UsersMeta.UserIDCol)]
        public long UserId { get; set; }
        public string UserName { get; set; }

        [SelectFactoryMehtod]
        public static IQueryFactoryResult GetSelectStatement()
        {
            return new QueryFactoryResult(UsersMeta.SelectStatement);
        }
    }

    [ForModel(UsersMeta.UserTable)]
    [SelectFactory(UsersMeta.SelectStatement)]
    public class Users_PK_IDFM_FUNCSELECTFACWITHPARAM
    {
        [PrimaryKey]
        [ForModel(UsersMeta.UserIDCol)]
        public long UserId { get; set; }
        public string UserName { get; set; }

        [SelectFactoryMehtod]
        public static IQueryFactoryResult GetSelectStatement(int whereID)
        {
            return new QueryFactoryResult(UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new QueryParameter("paramA", whereID));
        }
    }
}