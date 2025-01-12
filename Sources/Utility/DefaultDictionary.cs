using System;
using System.Collections.Generic;

namespace Dot42.Utility
{
	/// <summary>
	/// Dictionary with default values.
	/// </summary>
	public sealed class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> dict;
		private readonly Func<TKey, TValue> defaultProvider;
		
		public DefaultDictionary(TValue defaultValue, IDictionary<TKey, TValue> dictionary = null)
			: this(key => defaultValue, dictionary)
		{
		}
		
		public DefaultDictionary(Func<TKey, TValue> defaultProvider = null, IDictionary<TKey, TValue> dictionary = null)
		{
			this.dict = dictionary ?? new Dictionary<TKey, TValue>();
			this.defaultProvider = defaultProvider ?? (key => default(TValue));
		}
		
		public TValue this[TKey key] {
			get {
				TValue val;
				if (dict.TryGetValue(key, out val))
					return val;
				else
					return dict[key] = defaultProvider(key);
			}
			set {
				dict[key] = value;
			}
		}
		
		public ICollection<TKey> Keys {
			get { return dict.Keys; }
		}
		
		public ICollection<TValue> Values {
			get { return dict.Values; }
		}
		
		public int Count {
			get { return dict.Count; }
		}
		
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get { return false; }
		}
		
		public bool ContainsKey(TKey key)
		{
			return dict.ContainsKey(key);
		}
		
		public void Add(TKey key, TValue value)
		{
			dict.Add(key, value);
		}
		
		public bool Remove(TKey key)
		{
			return dict.Remove(key);
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			return dict.TryGetValue(key, out value);
		}
		
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			dict.Add(item);
		}
		
		public void Clear()
		{
			dict.Clear();
		}
		
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return dict.Contains(item);
		}
		
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			dict.CopyTo(array, arrayIndex);
		}
		
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return dict.Remove(item);
		}
		
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return dict.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return dict.GetEnumerator();
		}
	}
}