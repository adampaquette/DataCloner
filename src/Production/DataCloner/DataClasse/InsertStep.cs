﻿using System;
using System.Collections.Generic;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataClasse
{
	public class InsertStep : IExecutionStep
    {
        public Int32 StepId { get; set; }
		public Int32 Depth { get; set; }
	    public List<SqlVariable> Variables { get; set; }
        public ITableSchema TableSchema { get; set; }
        public ITableIdentifier SourceTable { get; set; }
        public ITableIdentifier DestinationTable { get; set; }
        public object[] DataRow { get; set; }

        public InsertStep()
        {
            Variables = new List<SqlVariable>();
        }
    }
}
