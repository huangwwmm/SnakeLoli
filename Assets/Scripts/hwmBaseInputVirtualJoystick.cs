using UnityEngine;

public class hwmBaseInputVirtualJoystick : MonoBehaviour
{
	public hwmConstants.AxisIndex XAxis;
	public hwmConstants.AxisIndex YAxis;

	protected Vector2 m_Value;
	protected bool m_ValueAvailable = false;

	protected void OnEnable()
	{
		hwmSystem.GetInstance().GetInput().GetAxis(XAxis).OnGetValueFromUI += OnGetAxisX;
		hwmSystem.GetInstance().GetInput().GetAxis(YAxis).OnGetValueFromUI += OnGetAxisY;
	}

	protected void OnDisable()
	{
		if (hwmSystem.GetInstance() != null) // system equal null when game application abort
		{
			hwmSystem.GetInstance().GetInput().GetAxis(XAxis).OnGetValueFromUI -= OnGetAxisX;
			hwmSystem.GetInstance().GetInput().GetAxis(YAxis).OnGetValueFromUI -= OnGetAxisY;
		}
	}

	private void OnGetAxisX(hwmInput.UIEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}

		args.Handled = m_ValueAvailable;
		args.Value = m_Value.x;
	}

	private void OnGetAxisY(hwmInput.UIEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}

		args.Handled = m_ValueAvailable;
		args.Value = m_Value.y;
	}
}