using System.Collections;
using UnityEditor;

public class hwmEditorCoroutine
{
	private readonly IEnumerator m_Routine;

	public static hwmEditorCoroutine Start(IEnumerator routine)
	{
		hwmEditorCoroutine coroutine = new hwmEditorCoroutine(routine);
		coroutine.Start();
		return coroutine;
	}

	private hwmEditorCoroutine(IEnumerator routine)
	{
		m_Routine = routine;
	}

	private void Start()
	{
		EditorApplication.update += Update;
	}

	public void Stop()
	{
		EditorApplication.update -= Update;
	}

	private void Update()
	{
		if (!m_Routine.MoveNext())
		{
			Stop();
		}
	}
}