﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.QueryProvider;

namespace JPB.DataAccess.QueryBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryBuilder : IEnumerable
    {
        internal readonly IDatabase Database;
        protected readonly Type _forType;

        internal int _autoParameterCounter;

        /// <summary>
        /// Defines the Way how the Data will be loaded
        /// </summary>
        public EnumerationMode EnumerationMode { get; set; }

        /// <summary>
        /// If enabled Variables that are only used for parameters will be Renamed if there Existing multiple times
        /// </summary>
        public bool AllowParamterRenaming { get; set; }

        public QueryBuilder(IDatabase database, Type forType)
            : this(database)
        {
            _forType = forType;
        }

        public QueryBuilder(IDatabase database)
        {
            this.Database = database;
            Parts = new List<QueryPart>();
        }

        internal List<QueryPart> Parts { get; set; }

        /// <summary>
        /// Will concat all QueryParts into a statement and will check for Spaces
        /// </summary>
        /// <returns></returns>
        public IDbCommand Compile()
        {
            var query = CompileFlat();
            return Database.CreateCommandWithParameterValues(query.Item1, query.Item2);
        }

        public IEnumerator GetEnumerator()
        {
            if (_forType == null)
                throw new ArgumentNullException("No type Supplied", new Exception());

            if (EnumerationMode == EnumerationMode.FullOnLoad)
                return new QueryEagerEnumerator(this, _forType);
            return new QueryLazyEnumerator(this, _forType);
        }

        /// <summary>
        /// Executes a query without result parsing
        /// </summary>
        public int Execute()
        {
            return this.Compile().ExecuteGenericCommand(Database);
        }

        /// <summary>
        /// Adds a Query part to the Local collection
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public QueryBuilder Add(QueryPart part)
        {
            if (AllowParamterRenaming)
            {
                foreach (var queryParameter in part.QueryParameters)
                {
                    var fod = this.Parts.SelectMany(s => s.QueryParameters).FirstOrDefault(s => s.Name == queryParameter.Name);

                    if (fod == null)
                        continue;

                    //parameter is existing ... renaming new Parameter to Auto gen and renaming all ref in the Query
                    var name = fod.Name;
                    var newName = GetParamaterAutoID().ToString().CheckParamter();
                    part.Prefix = part.Prefix.Replace(name, newName);
                    queryParameter.Name = newName;
                }   
            }
            Parts.Add(part);
            return this;
        }

        /// <summary>
        /// Compiles the Query into a String|IEnumerable of Paramameter
        /// </summary>
        /// <returns></returns>
        public Tuple<string, IEnumerable<IQueryParameter>> CompileFlat()
        {
            var sb = new StringBuilder();
            var queryParts = Parts.ToArray();
            QueryPart pref = null;
            var param = new List<IQueryParameter>();

            foreach (var queryPart in queryParts)
            {
                //take care of spaces
                //check if the last statement ends with a space or the next will start with one
                if (pref != null)
                {
                    if (!pref.Prefix.EndsWith(" ", true, CultureInfo.InvariantCulture) || !queryPart.Prefix.StartsWith(" ", true, CultureInfo.InvariantCulture))
                    {
                        queryPart.Prefix = " " + queryPart.Prefix;
                    }
                }
                sb.Append(queryPart.Prefix);
                param.AddRange(queryPart.QueryParameters);
                pref = queryPart;
            }

            return new Tuple<string, IEnumerable<IQueryParameter>>(sb.ToString(), param);
        }

        /// <summary>
        /// Converts the non Generic QueryBuilder into the Counterpart
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QueryBuilder<T> ForResult<T>()
        {
            return new QueryBuilder<T>(this.Database)
            {
                EnumerationMode = EnumerationMode,
                Parts = Parts
            };
        }

        /// <summary>
        /// Increment the counter +1 and return the value
        /// </summary>
        /// <returns></returns>
        public int GetParamaterAutoID()
        {
            return ++_autoParameterCounter;
        }

        public QueryBuilder SetAutoRenaming(bool value)
        {
            this.AllowParamterRenaming = value;
            return this;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryBuilder<T> : QueryBuilder, IEnumerable<T>
    {
        public QueryBuilder(IDatabase database)
            : base(database, typeof(T))
        {
        }

        public new IEnumerator<T> GetEnumerator()
        {
            if (_forType == null)
                throw new ArgumentNullException("No type Supplied", new Exception());

            if (EnumerationMode == EnumerationMode.FullOnLoad)
                return new QueryEagerEnumerator<T>(this, _forType);
            return new QueryLazyEnumerator<T>(this, _forType);
        }
    }
}
