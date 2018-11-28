using System;
using UnityEngine;

public class hwmBaseInputButton : MonoBehaviour
{
	private hwmConstants.ButtonIndex m_Index = hwmConstants.ButtonIndex.Notset;

	protected bool m_Press;

	public void EnableButton(hwmConstants.ButtonIndex index)
	{
		hwmDebug.Assert(m_Index == hwmConstants.ButtonIndex.Notset, "m_Index == hwmConstants.ButtonIndex.Notset");
		hwmSystem.GetInstance().GetInput().GetButton(index).OnGetValueFromUI += OnGetButton;
		m_Index = index;
	}

	public void DisableButton()
	{
		hwmDebug.Assert(m_Index != hwmConstants.ButtonIndex.Notset, "m_Index != hwmConstants.ButtonIndex.Notset");
		hwmSystem.GetInstance().GetInput().GetButton(m_Index).OnGetValueFromUI -= OnGetButton;
		m_Index = hwmConstants.ButtonIndex.Notset;
	}

	private void OnGetButton(hwmInput.UIEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}

		args.Handled = true;
		args.Value = m_Press ? 1 : 0;
	}
}