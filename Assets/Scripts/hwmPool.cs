using System.Collections.Generic;

public abstract class hwmPool<T>
{
	private Stack<T> m_Pool;

	public hwmPool(int initialSize = 4)
	{
		hwmDebug.Assert(initialSize > 0, "initialSize > 0");

		m_Pool = new Stack<T>();

		while (initialSize-- > 0)
		{
			Push(HandleCreateItem());
		}
	}

	public void Push(T obj)
	{
		HandlePushItem(ref obj);

		m_Pool.Push(obj);
	}

	public T Pop()
	{
		T obj = m_Pool.Count > 0
			? m_Pool.Pop()
			: HandleCreateItem();

		HandlePopItem(ref obj);
		return obj;
	}

	public void Dispose()
	{
		foreach (T obj in m_Pool)
		{
			HandleDisposeItem(obj);
		}
		m_Pool = null;
	}

	protected abstract T HandleCreateItem();

	protected abstract void HandleDisposeItem(T item);

	protected abstract void HandlePushItem(ref T item);

	protected abstract void HandlePopItem(ref T item);
}