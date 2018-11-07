using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class snEventSystem : EventSystem
{
	[Space(10)]
	public BaseInput InputOverride;

	protected override void Awake()
	{
		base.Awake();

		if (InputOverride != null)
		{
			StartCoroutine(OverrideEventSystemInputModule_Co());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
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
		while (currentInputModule == null)
		{
			// TODO add infinite loop protection
			yield return null;
		}
		inputOverrideFieldInfo.SetValue(currentInputModule, InputOverride);
	}
}