using System;
using UnityEngine;

public class hwmInput : MonoBehaviour
{
	public hwmEventSystem EventSystem;
	public hwmJoystickCursor JoystickCursor;

	public Axis[] Axiss;

	private AxisUIEventArgs m_AxisUIEventArgs;

	public void SetAllAxisEnable(bool enable)
	{
		for (int iAxis = 0; iAxis < Axiss.Length; iAxis++)
		{
			Axiss[iAxis].SetEnable(enable);
		}
	}

	public float GetAxisValue(hwmConstants.AxisIndex index)
	{
		Axis axis = Axiss[(int)index];
		return axis.GetEnable() ? axis.GetValue() : 0;
	}

	protected void Awake()
	{
		Array.Sort(Axiss, SortAxisComparison);
		for (int iAxis = 0; iAxis < Axiss.Length; iAxis++)
		{
			hwmDebug.Assert(iAxis == (int)Axiss[iAxis].Index, "iAxis == (int)Axiss[iAxis].Index");
		}
		m_AxisUIEventArgs = new AxisUIEventArgs();
	}

	protected void OnDestroy()
	{
		m_AxisUIEventArgs = null;
	}

	protected void Update()
	{
		for (int iAxis = 0; iAxis < Axiss.Length; iAxis++)
		{
			Axis iterAxis = Axiss[iAxis];
			if (!iterAxis.GetEnable())
			{
				continue;
			}

			float value = Input.GetAxis(iterAxis.AxesName);
			if (value == 0)
			{
				value = Input.GetKey(iterAxis.MinValueKeyCode)
					? -1
					: Input.GetKey(iterAxis.MaxValueKeyCode)
						? 1
						: 0;
			}
			if (value == 0 && iterAxis.OnGetValueFromUI != null)
			{
				m_AxisUIEventArgs.Handled = false;
				iterAxis.OnGetValueFromUI(m_AxisUIEventArgs);
				if (m_AxisUIEventArgs.Handled)
				{
					value = m_AxisUIEventArgs.Value;
				}
			}
			iterAxis.SetValue(value);
		}
	}

	private int SortAxisComparison(Axis x, Axis y)
	{
		return x.Index - y.Index;
	}

	[System.Serializable]
	public class Axis
	{
		public hwmConstants.AxisIndex Index;
		public KeyCode MinValueKeyCode;
		public KeyCode MaxValueKeyCode;
		public string AxesName = "Horizontal";
		public Action<AxisUIEventArgs> OnGetValueFromUI;

		private bool m_Enable = false;
		private float m_Value = 0.0f;

		public bool GetEnable()
		{
			return m_Enable;
		}

		public void SetEnable(bool enable)
		{
			m_Enable = enable;
		}

		public void SetValue(float value)
		{
			m_Value = value;
		}

		public float GetValue()
		{
			return m_Value;
		}
	}

	public class AxisUIEventArgs
	{
		public bool Handled;
		public float Value;
	}
}