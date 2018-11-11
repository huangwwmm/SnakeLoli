using UnityEngine;

public class hwmUIRoot : MonoBehaviour
{
	private bool m_Display = false;

	public bool IsDisplay()
	{
		return m_Display;
	}

	public void OnUIRootInitialize()
	{
		HandleUIRootInitialize();
		HandleUIRootLocalize();

		OnUIRootDisplay();
	}

	public void OnUIRootDestroy()
	{
		OnUIRootHide();

		HandleUIRootDestroy();
	}

	public void OnUIRootDisplay()
	{
		m_Display = true;
		HandleUIRootDisplay();
	}

	public void OnUIRootHide()
	{
		HandleUIRootHide();
		m_Display = false;
	}

	protected virtual void HandleUIRootInitialize()
	{
	}

	protected virtual void HandleUIRootDestroy()
	{
	}

	protected virtual void HandleUIRootDisplay()
	{
	}

	protected virtual void HandleUIRootHide()
	{
	}

	protected virtual void HandleUIRootLocalize()
	{
	}
}