﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.UnitTests.TestModels
{
    [ForModel("TestProcA")]
    public class TestProcAParams
    {

    }

    [ForModel("TestProcB")]
    public class TestProcBParams
    {
        [ForModel("number")]
        public int Number { get; set; }
    }
}
