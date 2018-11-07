using UnityEngine;

public class snInput : MonoBehaviour
{
	private static snInput ms_Instance;

	public snEventSystem EventSystem;
	public snJoystickCursor JoystickCursor;

	public static snInput GetInstance()
	{
		return ms_Instance;
	}

	protected void Awake()
	{
		ms_Instance = this;
	}

	protected void OnDestroy()
	{
		ms_Instance = null;
	}
}