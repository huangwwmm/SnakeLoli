using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Note cant storage NULL
/// </summary>
public class hwmFreeList<T> : IEnumerable, ICollection, IList, IEnumerable<T>, ICollection<T>, IList<T>
{
	private const int DEFAULT_CAPACITY = 4;
	/// <summary>
	/// <see cref="Array.MaxArrayLength"/>
	/// </summary>
	private const int MAX_ARRAY_LENGTH = 0X7FEFFFFF;

	private static readonly T[] ms_EmptyItems = new T[0];

	private T[] m_Items;
	private object m_SyncRoot;
	/// <summary>
	/// avoid add/remove when foreach
	/// </summary>
	private int m_Version;

	public T this[int index]
	{
		get
		{
			// Following trick can reduce the range check by one
			if ((uint)index >= (uint)Count)
			{
				throw new ArgumentOutOfRangeException("index", string.Format("index:{0} m_Size:{1}", index, Count));
			}
			return m_Items[index];
		}
		set
		{
			if ((uint)index >= (uint)Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			m_Items[index] = value;
			m_Version++;
		}
	}

	public int Count { get; private set; }

	/// <summary>
	/// Gets and sets the capacity of this list.  The capacity is the size of the internal array used to hold items.  When set, the internal array of the list is reallocated to the given capacity.
	/// </summary>
	public int Capacity
	{
		get
		{
			return m_Items.Length;
		}
		set
		{
			if (value < Count)
			{
				throw new ArgumentOutOfRangeException("Capacity");
			}

			if (value != m_Items.Length)
			{
				if (value > 0)
				{
					T[] newItems = new T[value];
					if (Count > 0)
					{
						Array.Copy(m_Items, 0, newItems, 0, Count);
					}
					m_Items = newItems;
				}
				else
				{
					m_Items = ms_EmptyItems;
				}
			}
		}
	}

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (T)value;
		}
	}

	bool ICollection.IsSynchronized { get { return false; } }

	bool ICollection<T>.IsReadOnly { get { return false; } }

	bool IList.IsFixedSize { get { return false; } }

	bool IList.IsReadOnly { get { return false; } }

	object ICollection.SyncRoot
	{
		get
		{
			if (m_SyncRoot == null)
			{
				System.Threading.Interlocked.CompareExchange<Object>(ref m_SyncRoot, new Object(), null);
			}
			return m_SyncRoot;
		}
	}

	/// <summary>
	/// Constructs a List.
	/// The list is initially empty and has a capacity of zero.
	/// Upon adding the first element to the list the capacity is increased to 16, and then increased in multiples of two as required.
	/// </summary>
	public hwmFreeList()
	{
		m_Items = ms_EmptyItems;
		Count = 0;
	}

	/// <summary>
	/// Constructs a List with a given initial capacity. The list is initially empty, but will have room for the given number of elements before any reallocations are required.
	/// </summary>
	public hwmFreeList(int capacity)
	{
		hwmDebug.Assert(capacity >= 0, "capacity >= 0");

		m_Items = capacity == 0
			? ms_EmptyItems
			: new T[capacity];
		Count = 0;
	}

	public int Add(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}

		if (Count == m_Items.Length)
		{
			EnsureCapacity(Count + 1);
		}
		int index = Count++;
		m_Items[index] = item;
		m_Version++;
		return index;
	}

	public void Clear()
	{
		if (Count > 0)
		{
			// Don't need to doc this but we clear the elements so that the gc can reclaim the references.
			Array.Clear(m_Items, 0, Count);
			Count = 0;
		}
		m_Version++;
	}

	public bool Contains(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}

		EqualityComparer<T> c = EqualityComparer<T>.Default;
		for (int iItem = 0; iItem < Count; iItem++)
		{
			if (c.Equals(m_Items[iItem], item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		// Delegate rest of error checking to Array.Copy.
		Array.Copy(m_Items, 0, array, arrayIndex, Count);
	}

	public bool Remove(T item)
	{
		int index = IndexOf(item);
		if (index >= 0)
		{
			RemoveAt(index);
			return true;
		}

		return false;
	}

	public void RemoveAt(int index)
	{
		if ((uint)index >= (uint)Count)
		{
			throw new ArgumentOutOfRangeException("Capacity");
		}

		int lastIndex = --Count;
		m_Items[index] = m_Items[lastIndex];
		m_Items[lastIndex] = default(T);
		m_Version++;
	}

	public int IndexOf(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}

		return Array.IndexOf(m_Items, item, 0, Count);
	}

	public void Shrink(int reserve = 0)
	{
		T[] newItems = new T[Count + reserve];

		int index = 0;
		for (int iItem = 0; iItem < m_Items.Length; iItem++)
		{
			if (index >= Count)
			{
				throw new ArgumentOutOfRangeException("m_ValidItemCount");
			}
			newItems[index] = m_Items[iItem];
			index++;
		}

		m_Items = newItems;
		m_Version++;
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Returns an enumerator for this list with the given permission for removal of elements. If modifications made to the list while an enumeration is in progress, the MoveNext and GetObject methods of the enumerator will throw an exception.
	/// </summary>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		Array.Copy(m_Items, 0, array, 0, Count);
		return array;
	}

	/// <summary>
	/// Ensures that the capacity of this list is at least the given minimum value. If the currect capacity of the list is less than min, the capacity is increased to twice the current capacity or to min, whichever is larger.
	/// </summary>
	private void EnsureCapacity(int min)
	{
		if (m_Items.Length < min)
		{
			int newCapacity = m_Items.Length == 0 ? DEFAULT_CAPACITY : m_Items.Length * 2;
			// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
			// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
			if ((uint)newCapacity > MAX_ARRAY_LENGTH)
			{
				newCapacity = MAX_ARRAY_LENGTH;
			}
			if (newCapacity < min)
			{
				newCapacity = min;
			}
			Capacity = newCapacity;
		}
	}

	int IList.Add(object item)
	{
		return Add((T)item);
	}

	void ICollection<T>.Add(T item)
	{
		Add(item);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		Array.Copy(m_Items, 0, array, arrayIndex, Count);
	}

	bool IList.Contains(object item)
	{
		return Contains((T)item);
	}

	int IList.IndexOf(object item)
	{
		return IndexOf((T)item);
	}

	void IList.Insert(int index, object item)
	{
		Insert(index, (T)item);
	}

	void IList.Remove(object item)
	{
		Remove((T)item);
	}

	[Serializable]
	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		private hwmFreeList<T> m_List;
		private int m_Index;
		private readonly int m_Version;

		public T Current { get; private set; }

		object IEnumerator.Current { get { return Current; } }

		internal Enumerator(hwmFreeList<T> list)
		{
			m_List = list;
			m_Index = 0;
			m_Version = list.m_Version;
			Current = default(T);
		}

		public bool MoveNext()
		{
			if (m_Version != m_List.m_Version)
			{
				throw new InvalidOperationException("list version invalid");
			}
			else
			{
				// Q: why use localList?
				hwmFreeList<T> localList = m_List;

				while ((uint)m_Index < (uint)localList.Count)
				{
					Current = localList.m_Items[m_Index];
					m_Index++;
					return true;
				}

				m_Index = m_List.Count + 1;
				Current = default(T);
				return false;
			}
		}

		public void Dispose()
		{
			m_List = null;
			Current = default(T);
		}

		void IEnumerator.Reset()
		{
			if (m_Version != m_List.m_Version)
			{
				throw new InvalidOperationException("list version invalid");
			}

			m_Index = 0;
			Current = default(T);
		}
	}
}