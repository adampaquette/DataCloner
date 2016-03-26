using System;

namespace DataCloner.Core.Framework
{
    /// <summary>
    /// Fast list access by index
    /// </summary>
    public class FastAccessList<T>
    {
        T[] _data = null;
        int _length = 0;

        public FastAccessList()
        {
        }

        public FastAccessList(T[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            _data = data;
            _length = data.Length;
        }

        public int Length
        {
            get { return _length; }
        }

        public T this[int index]
        {
            get { return _data[index]; }
        }

        public int Add(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (_data == null)
                _data = new T[128];

            if (_length == _data.Length)
            {
                var newData = new T[_data.Length * 2];
                Array.Copy(_data, newData, _data.Length);
                _data = newData;
            }

            var idx = _length;
            _length++;

            _data[idx] = obj;
            return idx;
        }

        public int TryAdd(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            int id;
            if ((id = FindT(obj)) == -1)
                id = Add(obj);
            return id;
        }

        public int FindT(T obj)
        {
            if (obj != null && _data != null)
            {
                for (int i = 0; i < _data.Length; i++)
                {
                    if (obj.Equals(_data[i]))
                        return i;
                }
            }
            return -1;
        }

        public void Trim()
        {
            if (_data != null)
            {
                for (int i = _data.Length - 1; i > 0; i--)
                {
                    if (_data[i] != null)
                    {
                        var length = i + 1;
                        var newData = new T[length];
                        Array.Copy(_data, newData, length);
                        _data = newData;
                        return;
                    }
                }
            }
        }
    }
}
