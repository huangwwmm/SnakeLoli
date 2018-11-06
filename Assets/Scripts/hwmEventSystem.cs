using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class hwmEventSystem : MonoBehaviour
{
	public BaseInput InputOverride;
	private EventSystem m_EventSystem;

	public bool b;

	protected void Awake()
	{
		m_EventSystem = GetComponent<EventSystem>();

		if (InputOverride != null)
		{
		}
	}

	protected void OnDestroy()
	{
		m_EventSystem = null;
	}

	protected void Update()
	{
		b = m_EventSystem.IsPointerOverGameObject();
	}
}