using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    public class Table : IDictionary<string, int>
    {
        Dictionary<string, int> _table = new Dictionary<string, int>();
        bool _canAdd;
        int _startIndex;

        public Table(IEnumerable<string> init = null, bool canAdd = false, int startIndex = 0)
        {
            _canAdd = canAdd;
            _startIndex = startIndex;
            if (init != null)
            foreach(var str in init)
            {
                Add(str);
            }
        }

        public void Add(string identifier)
        {
            _table.Add(identifier, _startIndex + _table.Count);
        }


        #region IDictionary
        public int this[string key]
        {
            get
            {
                return ((IDictionary<string, int>)_table)[key];
            }

            set
            {
                ((IDictionary<string, int>)_table)[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IDictionary<string, int>)_table).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<string, int>)_table).IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return ((IDictionary<string, int>)_table).Keys;
            }
        }

        public ICollection<int> Values
        {
            get
            {
                return ((IDictionary<string, int>)_table).Values;
            }
        }

        public void Add(KeyValuePair<string, int> item)
        {
            ((IDictionary<string, int>)_table).Add(item);
        }

        public void Add(string key, int value)
        {
            ((IDictionary<string, int>)_table).Add(key, value);
        }

        public void Clear()
        {
            ((IDictionary<string, int>)_table).Clear();
        }

        public bool Contains(KeyValuePair<string, int> item)
        {
            return ((IDictionary<string, int>)_table).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, int>)_table).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
        {
            ((IDictionary<string, int>)_table).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return ((IDictionary<string, int>)_table).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, int> item)
        {
            return ((IDictionary<string, int>)_table).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, int>)_table).Remove(key);
        }

        public bool TryGetValue(string key, out int value)
        {
            return ((IDictionary<string, int>)_table).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, int>)_table).GetEnumerator();
        } 
        #endregion
    }
}
