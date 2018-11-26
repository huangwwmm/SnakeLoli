using System;
using UnityEngine;

public class hwmBaseInputButton : MonoBehaviour
{
	public hwmConstants.ButtonIndex Index;

	protected bool m_Press;

	protected void OnEnable()
	{
		hwmSystem.GetInstance().GetInput().GetButton(Index).OnGetValueFromUI += OnGetButton;
	}

	protected void OnDisable()
	{
		if (hwmSystem.GetInstance() != null) // system equal null when game application abort
		{
			hwmSystem.GetInstance().GetInput().GetButton(Index).OnGetValueFromUI -= OnGetButton;
		}
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