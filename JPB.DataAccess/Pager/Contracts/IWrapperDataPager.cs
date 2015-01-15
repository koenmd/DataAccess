﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JPB.DataAccess.Pager.Contracts
{
    /// <summary>
    /// A wrapper interface to convert all incomming items from Load method into new type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    public interface IWrapperDataPager<T,TE> : IDataPager<T>
    {
        /// <summary>
        /// Function to convert all items from T to TE
        /// </summary>
        Func<T,TE> Converter { get; set; }

        /// <summary>
        /// new Collection of TE
        /// </summary>
        new ICollection<TE> CurrentPageItems { get; }
    }
}