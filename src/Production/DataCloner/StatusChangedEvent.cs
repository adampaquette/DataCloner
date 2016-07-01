using System;

namespace DataCloner.Core
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
    public sealed class StatusChangedEventArgs : EventArgs
    {
        public Status Status { get;  }
        public int CurrentIndex { get ; }
        public int MaxIndex { get; }
        public RowIdentifier SourceRow { get; }
        public int Level { get; }

        public StatusChangedEventArgs(Status status, int currentIndex, int maxIndex, RowIdentifier sourceRow, int level)
        {
            Status = status;
            CurrentIndex = currentIndex;
            MaxIndex = maxIndex;
            SourceRow = sourceRow;
            Level = level;
        }
    }

    public enum Status
    {
        BuildingCache,
        Cloning,
        FetchingDerivatives
    }
}
