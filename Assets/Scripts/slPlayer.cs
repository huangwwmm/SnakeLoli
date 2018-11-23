using UnityEngine;

public class slPlayer : MonoBehaviour 
{
	private Camera m_Camera;
	private slHUD m_HUD;

	public void Initialize()
	{
		m_Camera = transform.Find("Camera").GetComponent<Camera>();
		m_HUD = transform.Find("HUD").GetComponent<slHUD>();
	}

	public void Dispose()
	{
		m_HUD = null;
		m_Camera = null;
	}
}