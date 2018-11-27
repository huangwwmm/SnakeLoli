using System.Collections.Generic;

public class hwmPool<T>
{
	private Stack<T> m_Pool;

	public void Initialize(int initialSize = 4)
	{
		m_Pool = new Stack<T>();
		hwmDebug.Assert(initialSize >= 0, "initialSize > 0");
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

	protected virtual T HandleCreateItem()
	{
		return default(T);
	}

	protected virtual void HandleDisposeItem(T item) { }

	protected virtual void HandlePushItem(ref T item) { }

	protected virtual void HandlePopItem(ref T item) { }
}