using UnityEngine;

public class slPlayer : MonoBehaviour 
{
	private Camera m_Camera;

	public void Initialize()
	{
		m_Camera = transform.Find("Camera").GetComponent<Camera>();
	}

	public void Dispose()
	{
		m_Camera = null;
	}
}