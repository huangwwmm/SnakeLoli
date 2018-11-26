using System;
using UnityEngine;

public class hwmBaseVirtualJoystick : MonoBehaviour
{
	public hwmConstants.AxisIndex XAxis;
	public hwmConstants.AxisIndex YAxis;

	protected Vector2 m_Value;
	protected bool m_ValueAvailable = false;

	protected void OnEnable()
	{
		hwmSystem.GetInstance().GetInput().Axiss[(int)XAxis].OnGetValueFromUI += OnGetAxisX;
		hwmSystem.GetInstance().GetInput().Axiss[(int)YAxis].OnGetValueFromUI += OnGetAxisY;
	}

	protected void OnDisable()
	{
		if (hwmSystem.GetInstance() != null) // system equal null when game application abort
		{
			hwmSystem.GetInstance().GetInput().Axiss[(int)XAxis].OnGetValueFromUI -= OnGetAxisX;
			hwmSystem.GetInstance().GetInput().Axiss[(int)YAxis].OnGetValueFromUI -= OnGetAxisY;
		}
	}

	private void OnGetAxisX(hwmInput.AxisUIEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}

		args.Handled = m_ValueAvailable;
		args.Value = m_Value.x;
	}

	private void OnGetAxisY(hwmInput.AxisUIEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}

		args.Handled = m_ValueAvailable;
		args.Value = m_Value.y;
	}
}