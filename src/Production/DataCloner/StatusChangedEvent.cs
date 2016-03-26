﻿using System;
using DataCloner.Core.Internal;

namespace DataCloner.Core
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
    public sealed class StatusChangedEventArgs : EventArgs
    {
        private Status _status;
        private int _currentIndex;
        private int _maxIndex;
        private RowIdentifier _sourceRow;
        private int _level;

        public Status Status { get { return _status; } }
        public int CurrentIndex { get { return _currentIndex; } }
        public int MaxIndex { get { return _maxIndex; } }
        public RowIdentifier SourceRow { get { return _sourceRow; } }
        public int Level { get { return _level; } }

        public StatusChangedEventArgs(Status status, int currentIndex, int maxIndex, RowIdentifier sourceRow, int level)
        {
            _status = status;
            _currentIndex = currentIndex;
            _maxIndex = maxIndex;
            _sourceRow = sourceRow;
            _level = level;
        }
    }

    public enum Status
    {
        BuildingCache,
        Cloning,
        FetchingDerivatives
    }
}
