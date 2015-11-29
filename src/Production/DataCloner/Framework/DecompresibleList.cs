using System;

namespace DataCloner.Framework
{
    /// <summary>
    /// Fast list access by index for decompression purpose
    /// </summary>
    public class DecompresibleList
    {
        object[] _data = null;
        int _length = 0;

        public DecompresibleList()
        {            
        }

        public int Length
        {
            get { return _length; }
        }

        public object this[int index]
        {
            get { return _data[index]; }
        }

        public int Add(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (_data == null)
                _data = new object[128];

            if (_length == _data.Length)
            {
                var newData = new object[_data.Length * 2];
                Array.Copy(_data, newData, newData.Length);
                _data = newData;
            }
            
            var idx = _length;
            _length++;

            _data[idx] = obj;
            return idx;
        }

        public int TryAdd(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            int id;
            if ((id = FindObject(obj)) == -1)
                id = Add(obj);
            return id;
        }

        public int FindObject(object obj)
        {
            if (obj != null && _data != null)
            {
                for (int i = 0; i < _data.Length; i++)
                {
                    if ((object)obj == (object)_data[i])
                        return i;
                }
            }
            return -1;
        }

        public void Trim()
        {
            if (_data != null)
            {
                for (int i = _data.Length-1; i > 0; i--)
                {
                    if (_data[i] != null)
                    {
                        var length = i + 1;
                        var newData = new object[length];
                        Array.Copy(_data, newData, length);
                        _data = newData;
                        return;
                    }
                }
            }
        }
    }
}
