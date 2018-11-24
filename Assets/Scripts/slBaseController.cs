using UnityEngine;

public class slBaseController : MonoBehaviour 
{
	protected slSnake m_Snake;

	public void Initialize()
	{
		HandleInitialize();
	}

	public void SetControllerSnake(slSnake snake)
	{
		hwmDebug.Assert(m_Snake == null, "m_Snake == null");

		m_Snake = snake;
	}

	public void UnControllerSnake()
	{
		hwmDebug.Assert(m_Snake != null, "m_Snake != null");

		m_Snake = null;
	}

	public void Dispose()
	{
		HandleDispose();

		m_Snake = null;
	}

	protected virtual void HandleInitialize()
	{

	}

	protected virtual void HandleDispose()
	{

	}
}