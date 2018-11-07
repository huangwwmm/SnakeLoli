using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class hwmEventSystem : MonoBehaviour
{
	public BaseInput InputOverride;
	private EventSystem m_EventSystem;

	protected void Awake()
	{
		m_EventSystem = GetComponent<EventSystem>();
		if (InputOverride != null)
		{
			StartCoroutine(OverrideEventSystemInputModule_Co());
		}
	}

	protected void OnDestroy()
	{
		m_EventSystem = null;
	}

	protected void Update()
	{
	}

	/// <summary>
	/// Q: Why use Coroutine?
	/// A: I dont know when currentInputModule not null
	/// </summary>
	private IEnumerator OverrideEventSystemInputModule_Co()
	{
		Type baseInputModuleType = typeof(BaseInputModule);
		FieldInfo inputOverrideFieldInfo = baseInputModuleType.GetField("m_InputOverride", BindingFlags.NonPublic | BindingFlags.Instance);
		// TODO Assert inputOverrideFieldInfo != null
		while (m_EventSystem.currentInputModule == null)
		{
			// TODO add infinite loop protection
			yield return null;
		}
		inputOverrideFieldInfo.SetValue(m_EventSystem.currentInputModule, InputOverride);
	}
}