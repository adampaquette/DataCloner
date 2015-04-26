using System;

namespace DataCloner
{
	public delegate void QueryCommitingEventHandler(object sender, QueryCommitingEventArgs e);
	public sealed class QueryCommitingEventArgs : EventArgs
	{
		public string Query { get; }
		public bool Cancel { get; set; }

		public QueryCommitingEventArgs(string query)
		{
			Query = query;
		}
	}
}
