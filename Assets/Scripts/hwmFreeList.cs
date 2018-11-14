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
	private static readonly bool[] ms_EmptyItemValids = new bool[0];

	private T[] m_Items;
	private bool[] m_ItemValids;
	private int m_Size;
	private int m_ValidItemCount;
	private Object m_SyncRoot;
	/// <summary>
	/// avoid add/remove when foreach
	/// </summary>
	private int m_Version;

	public T this[int index]
	{
		get
		{
			// Following trick can reduce the range check by one
			if ((uint)index >= (uint)m_Size)
			{
				throw new ArgumentOutOfRangeException("index", string.Format("index:{0} m_Size:{1}", index, m_Size));
			}
			return m_Items[index];
		}
		set
		{
			if ((uint)index >= (uint)m_Size)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			m_Items[index] = value;
			if (!m_ItemValids[index])
			{
				m_ItemValids[index] = true;
				m_ValidItemCount++;
			}
			m_Version++;
		}
	}

	public int Count { get { return m_Size; } }

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
			if (value < m_Size)
			{
				throw new ArgumentOutOfRangeException("Capacity");
			}

			if (value != m_Items.Length)
			{
				if (value > 0)
				{
					T[] newItems = new T[value];
					bool[] newItemValids = GenerateItemValids(value);
					if (m_Size > 0)
					{
						Array.Copy(m_Items, 0, newItems, 0, m_Size);
						Array.Copy(m_ItemValids, 0, newItemValids, 0, m_Size);
					}
					m_Items = newItems;
					m_ItemValids = newItemValids;
					m_ValidItemCount = 0;
					for (int iItem = 0; iItem < m_ItemValids.Length; iItem++)
					{
						if (m_ItemValids[iItem])
						{
							m_ValidItemCount++;
						}
					}
				}
				else
				{
					m_Items = ms_EmptyItems;
					m_ItemValids = ms_EmptyItemValids;
					m_ValidItemCount = 0;
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
		m_ItemValids = ms_EmptyItemValids;
		m_ValidItemCount = 0;
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

		m_ItemValids = GenerateItemValids(capacity);
		m_ValidItemCount = 0;
	}

	public int Add(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}

		for (int iItem = 0; iItem < m_ItemValids.Length; iItem++)
		{
			if (!m_ItemValids[iItem])
			{
				m_Size = Math.Max(m_Size, iItem + 1);
				m_Items[iItem] = item;
				m_ItemValids[iItem] = true;
				m_ValidItemCount++;
				m_Version++;
				return iItem;
			}
		}

		if (m_Size == m_Items.Length)
		{
			EnsureCapacity(m_Size + 1);
		}
		int index = m_Size++;
		m_Items[index] = item;
		m_ItemValids[index] = true;
		m_ValidItemCount++;
		m_Version++;
		return index;
	}

	public void Clear()
	{
		if (m_Size > 0)
		{
			// Don't need to doc this but we clear the elements so that the gc can reclaim the references.
			Array.Clear(m_Items, 0, m_Size);
			for (int iItem = 0; iItem < m_ItemValids.Length; iItem++)
			{
				m_ItemValids[iItem] = false;
			}
			m_ValidItemCount = 0;
			m_Size = 0;
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
		for (int iItem = 0; iItem < m_Size; iItem++)
		{
			if (m_ItemValids[iItem]
				&& c.Equals(m_Items[iItem], item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		// Delegate rest of error checking to Array.Copy.
		Array.Copy(m_Items, 0, array, arrayIndex, m_Size);
	}

	public bool Remove(T item)
	{
		int index = IndexOf(item);
		if (index >= 0
			&& m_ItemValids[index])
		{
			RemoveAt(index);
			return true;
		}

		return false;
	}

	public void RemoveAt(int index)
	{
		if ((uint)index >= (uint)m_Size)
		{
			throw new ArgumentOutOfRangeException("Capacity");
		}

		if (m_ItemValids[index])
		{
			m_Items[index] = default(T);
			m_ItemValids[index] = false;
			m_ValidItemCount--;
			m_Version++;
		}
	}

	public int IndexOf(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}

		return Array.IndexOf(m_Items, item, 0, m_Size);
	}

	public void Shrink(int reserve = 0)
	{
		T[] newItems = new T[m_ValidItemCount + reserve];
		bool[] newItemVailds = GenerateItemValids(m_ValidItemCount + reserve);

		int index = 0;
		for (int iItem = 0; iItem < m_Items.Length; iItem++)
		{
			if (m_ItemValids[iItem])
			{
				if (index >= m_ValidItemCount)
				{
					throw new ArgumentOutOfRangeException("m_ValidItemCount");
				}
				newItems[index] = m_Items[iItem];
				newItemVailds[index] = true;
				index++;
			}
		}

		m_Size = m_ValidItemCount;
		m_Items = newItems;
		m_ItemValids = newItemVailds;
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

	private bool[] GenerateItemValids(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}

		if (capacity == 0)
		{
			return ms_EmptyItemValids;
		}
		else
		{
			bool[] itemValids = new bool[capacity];

			for (int iItem = 0; iItem < capacity; iItem++)
			{
				itemValids[iItem] = false;
			}
			return itemValids;
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

	void ICollection.CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
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
		private int m_Version;
		private T m_Current;

		public T Current { get { return m_Current; } }

		object IEnumerator.Current { get { return m_Current; } }

		internal Enumerator(hwmFreeList<T> list)
		{
			m_List = list;
			m_Index = 0;
			m_Version = list.m_Version;
			m_Current = default(T);
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

				while ((uint)m_Index < (uint)localList.m_Size)
				{
					if (localList.m_ItemValids[m_Index])
					{
						m_Current = localList.m_Items[m_Index];
						m_Index++;
						return true;
					}
					else
					{
						m_Current = default(T);
						m_Index++;
					}
				}

				return MoveNextRare();
			}
		}

		public void Dispose()
		{
			m_List = null;
			m_Current = default(T);
		}

		private bool MoveNextRare()
		{
			m_Index = m_List.m_Size + 1;
			m_Current = default(T);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (m_Version != m_List.m_Version)
			{
				throw new InvalidOperationException("list version invalid");
			}

			m_Index = 0;
			m_Current = default(T);
		}
	}
}