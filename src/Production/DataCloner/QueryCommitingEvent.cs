using System;
using System.Data;

namespace DataCloner.Core
{
	public delegate void QueryCommitingEventHandler(object sender, QueryCommitingEventArgs e);
	public sealed class QueryCommitingEventArgs : EventArgs
	{
	    private IDbCommand _command;

		public IDbCommand Command { get{return _command;} }
		public bool Cancel { get; set; }

		public QueryCommitingEventArgs(IDbCommand command)
		{
			_command = command;
		}
	}
}
