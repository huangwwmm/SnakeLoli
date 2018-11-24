using System;
using System.Collections;
using System.Collections.Generic;

public class hwmDeque<T> : IEnumerable, ICollection, IEnumerable<T>
{
	private const int DEFAULT_BLOCKSIZE = 8;
	private const int NOTSET_BLOCK_ITEMINDEX = -1;

	private Block m_FrontBlock;
	private Block m_BackBlock;
	private int m_FrontIndex;
	private int m_BackIndex;

	private object m_SyncRoot;

	private readonly int m_BlockSize;
	private int m_Version;

	public int Count { get; private set; }

	bool ICollection.IsSynchronized { get { return false; } }

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

	public hwmDeque(int blockSize = DEFAULT_BLOCKSIZE)
	{
		m_BlockSize = blockSize;

		Count = 0;
		m_Version = 0;
	}

	public void PushFront(T item)
	{
		if (m_FrontBlock == null)
		{
			InitializeBlock(item);
		}
		else
		{
			if (m_FrontIndex > 0)
			{
				m_FrontIndex--;
				m_FrontBlock.Items[m_FrontIndex] = item;
			}
			else
			{
				Block block = new Block();
				block.Items = new T[m_BlockSize];
				m_FrontIndex = m_BlockSize - 1;
				block.Items[m_FrontIndex] = item;

				m_FrontBlock.LastBlock = block;
				block.NextBlock = m_FrontBlock;

				m_FrontBlock = block;
			}
		}

		Count++;
		m_Version++;
	}

	public void PushBack(T item)
	{
		if (m_BackBlock == null)
		{
			InitializeBlock(item);
		}
		else
		{
			if (m_BackIndex < m_BlockSize - 1)
			{
				m_BackIndex++;
				m_BackBlock.Items[m_BackIndex] = item;
			}
			else
			{
				Block block = new Block();
				block.Items = new T[m_BlockSize];
				m_BackIndex = 0;
				block.Items[m_BackIndex] = item;

				m_BackBlock.NextBlock = block;
				block.LastBlock = m_BackBlock;

				m_BackBlock = block;
			}
		}

		Count++;
		m_Version++;
	}

	public T PeekFront()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Deque is empty");
		}

		return m_FrontBlock.Items[m_FrontIndex];
	}

	public T PeekBack()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Deque is empty");
		}

		return m_BackBlock.Items[m_BackIndex];
	}

	public T PopFront()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Deque is empty");
		}

		T item = m_FrontBlock.Items[m_FrontIndex];

		Count--;
		if (Count == 0)
		{
			DisposeBlock();
		}
		else
		{
			m_FrontBlock.Items[m_FrontIndex++] = default(T);
			if (m_FrontIndex == m_BlockSize)
			{
				m_FrontBlock.Items = null;
				m_FrontBlock = m_FrontBlock.NextBlock;
				m_FrontBlock.LastBlock = null;
				m_FrontIndex = 0;
			}
		}

		m_Version++;
		return item;
	}

	public T PopBack()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Deque is empty");
		}

		T item = m_BackBlock.Items[m_BackIndex];

		Count--;
		if (Count == 0)
		{
			DisposeBlock();
		}
		else
		{
			m_BackBlock.Items[m_BackIndex--] = default(T);
			if (m_BackIndex == -1)
			{
				m_BackBlock.Items = null;
				m_BackBlock = m_BackBlock.LastBlock;
				m_BackBlock.NextBlock = null;
				m_BackIndex = m_BlockSize - 1;
			}
		}

		m_Version++;
		return item;
	}

	public void CopyTo(Array array, int index)
	{
		Array.Copy(ToArray(), 0, array, index, Count);
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		int arrayIndex = 0;
		foreach (T item in this)
		{
			array[arrayIndex++] = item;
		}
		return array;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void Clear()
	{
		if (Count > 0)
		{
			Block currentBlock = m_FrontBlock;
			while (currentBlock != null)
			{
				currentBlock.Items = null;
				currentBlock = currentBlock.NextBlock;
			}
			Count = 0;
		}
		m_Version++;
	}

	private void InitializeBlock(T item)
	{
		m_FrontIndex = m_BackIndex = (int)(m_BlockSize * 0.5f);

		Block block = new Block();
		block.Items = new T[m_BlockSize];
		block.Items[m_FrontIndex] = item;

		m_FrontBlock = block;
		m_BackBlock = block;
	}

	private void DisposeBlock()
	{
		hwmDebug.Assert(m_FrontBlock == m_BackBlock, "m_FrontBlock == m_BackBlock");
		m_FrontBlock.Items = null;
		m_FrontBlock = null;
		m_BackBlock = null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private class Block
	{
		public Block LastBlock;
		public Block NextBlock;

		public T[] Items;
	}

	[Serializable]
	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		private hwmDeque<T> m_Deque;
		private readonly int m_Version;
		private Block m_CurrentBlock;
		private int m_Index;

		public T Current { get; private set; }

		object IEnumerator.Current { get { return Current; } }

		internal Enumerator(hwmDeque<T> deque)
		{
			m_Deque = deque;
			m_Version = m_Deque.m_Version;
			m_CurrentBlock = m_Deque.m_FrontBlock;
			m_Index = m_Deque.m_FrontIndex;
			Current = default(T);
		}

		public bool MoveNext()
		{
			if (m_Version != m_Deque.m_Version)
			{
				throw new InvalidOperationException("deque version invalid");
			}
			else
			{
				if (m_CurrentBlock != null
					&& (m_CurrentBlock.NextBlock != null
						|| m_Index <= m_Deque.m_BackIndex))
				{
					Current = m_CurrentBlock.Items[m_Index++];
					if (m_Index == m_Deque.m_BlockSize)
					{
						m_CurrentBlock = m_CurrentBlock.NextBlock;
						m_Index = 0;
					}
					return true;
				}
				else
				{
					m_CurrentBlock = null;
					m_Index = m_Deque.m_BlockSize;
					Current = default(T);
					return false;
				}
			}
		}

		public void Dispose()
		{
			m_Deque = null;
			Current = default(T);
		}

		void IEnumerator.Reset()
		{
			if (m_Version != m_Deque.m_Version)
			{
				throw new InvalidOperationException("deque version invalid");
			}

			m_CurrentBlock = m_Deque.m_FrontBlock;
			m_Index = m_Deque.m_FrontIndex;
			Current = default(T);
		}
	}
}