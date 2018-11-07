using UnityEngine;

public class snSystem : MonoBehaviour
{
	private static snSystem ms_Instance;

	private float m_RealtimeSinceStartup;

	public static snSystem GetInstance()
	{
		return ms_Instance;
	}

	public float GetRealtimeSinceStartup()
	{
		return m_RealtimeSinceStartup;
	}

	protected void Awake()
	{
		ms_Instance = this;
	}

	protected void OnDestroy()
	{
		ms_Instance = null;
	}

	protected void Update()
	{
		m_RealtimeSinceStartup = Time.realtimeSinceStartup;
	}
}