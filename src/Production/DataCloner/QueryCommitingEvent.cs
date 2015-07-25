using System;

namespace DataCloner
{
	public delegate void QueryCommitingEventHandler(object sender, QueryCommitingEventArgs e);
	public sealed class QueryCommitingEventArgs : EventArgs
	{
	    private string _query;

		public string Query { get{return _query;} }
		public bool Cancel { get; set; }

		public QueryCommitingEventArgs(string query)
		{
			_query = query;
		}
	}
}
