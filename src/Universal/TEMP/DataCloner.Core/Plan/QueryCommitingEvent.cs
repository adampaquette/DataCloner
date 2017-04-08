using System;
using System.Data;

namespace DataCloner.Core.Plan
{
	public delegate void QueryCommitingEventHandler(object sender, QueryCommitingEventArgs e);
	public sealed class QueryCommitingEventArgs : EventArgs
	{
        public IDbCommand Command { get; }
		public bool Cancel { get; set; }

		public QueryCommitingEventArgs(IDbCommand command)
		{
			Command = command;
		}
	}
}
