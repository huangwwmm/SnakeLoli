using System;
using System.Collections;
using System.Collections.Generic;

public class hwmFreeList<T> : IEnumerable, ICollection, IEnumerable<T>, ICollection<T>
{
	public int Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsSynchronized
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public object SyncRoot
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsReadOnly
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public void Add(T item)
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		throw new NotImplementedException();
	}

	public bool Contains(T item)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public IEnumerator GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public bool Remove(T item)
	{
		throw new NotImplementedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		throw new NotImplementedException();
	}
}