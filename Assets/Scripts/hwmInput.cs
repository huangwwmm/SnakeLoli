using System;
using UnityEngine;

public class hwmInput : MonoBehaviour
{
	public hwmEventSystem EventSystem;
	public hwmJoystickCursor JoystickCursor;

	private Axis[] m_Axiss;
	private Button[] m_Buttons;

	private UIEventArgs m_UIEventArgs;

	#region Get&Set
	public void SetAllAxisEnable(bool enable)
	{
		for (int iAxis = 0; iAxis < m_Axiss.Length; iAxis++)
		{
			m_Axiss[iAxis].SetEnable(enable);
		}
	}

	public void SetAllButtonEnable(bool enable)
	{
		for (int iButton = 0; iButton < m_Buttons.Length; iButton++)
		{
			m_Buttons[iButton].SetEnable(enable);
		}
	}

	public Axis GetAxis(hwmConstants.AxisIndex index)
	{
		return m_Axiss[(int)index];
	}

	public Button GetButton(hwmConstants.ButtonIndex index)
	{
		return m_Buttons[(int)index];
	}
	#endregion End Get&Set

	protected void Awake()
	{
		m_UIEventArgs = new UIEventArgs();

		hwmInputConfig inpuConfig = hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.System, "InputConfig") as hwmInputConfig;
		m_Axiss = inpuConfig.Axiss;
		Array.Sort(m_Axiss, SortAxisComparison);
		for (int iAxis = 0; iAxis < m_Axiss.Length; iAxis++)
		{
			hwmDebug.Assert(iAxis == (int)m_Axiss[iAxis].Index, "iAxis == (int)Axiss[iAxis].Index");
		}

		m_Buttons = inpuConfig.Buttons;
		Array.Sort(m_Buttons, SortButtonComparison);
		for (int iButton = 0; iButton < m_Buttons.Length; iButton++)
		{
			hwmDebug.Assert(iButton == (int)m_Buttons[iButton].Index, "iButton == (int)Buttons[iButton].Index");
		}

	}

	protected void OnDestroy()
	{
		m_UIEventArgs = null;
	}

	protected void Update()
	{
		for (int iAxis = 0; iAxis < m_Axiss.Length; iAxis++)
		{
			Axis iterAxis = m_Axiss[iAxis];
			if (!iterAxis.IsEnable())
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
				m_UIEventArgs.Handled = false;
				iterAxis.OnGetValueFromUI(m_UIEventArgs);
				if (m_UIEventArgs.Handled)
				{
					value = m_UIEventArgs.Value;
				}
			}
			iterAxis.SetValue(value);
		}

		for (int iButton = 0; iButton < m_Buttons.Length; iButton++)
		{
			Button iterButton = m_Buttons[iButton];
			if (!iterButton.IsEnable())
			{
				continue;
			}

			bool press = false;
			for (int iKeyCode = 0; iKeyCode < iterButton.KeyCodes.Length; iKeyCode++)
			{
				if (Input.GetKey(iterButton.KeyCodes[iKeyCode]))
				{
					press = true;
					break;
				}
			}

			if (!press && iterButton.OnGetValueFromUI != null)
			{
				m_UIEventArgs.Handled = false;
				iterButton.OnGetValueFromUI(m_UIEventArgs);
				if (m_UIEventArgs.Handled)
				{
					press = m_UIEventArgs.Value > 0;
				}
			}

			iterButton.SetPress(press);
		}
	}

	private int SortAxisComparison(Axis x, Axis y)
	{
		return x.Index - y.Index;
	}

	private int SortButtonComparison(Button x, Button y)
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
		public Action<UIEventArgs> OnGetValueFromUI;

		private bool m_Enable = false;
		private float m_Value = 0.0f;

		public bool IsEnable()
		{
			return m_Enable;
		}

		public void SetEnable(bool enable)
		{
			m_Enable = enable;
			if (!enable)
			{
				m_Value = 0;
			}
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

	public class UIEventArgs
	{
		public bool Handled;
		public float Value;
	}

	[System.Serializable]
	public class Button
	{
		public hwmConstants.ButtonIndex Index;
		public KeyCode[] KeyCodes;
		public Action<UIEventArgs> OnGetValueFromUI;

		private bool m_Enable = false;
		private State m_State = State.Idle;

		public bool IsEnable()
		{
			return m_Enable;
		}

		public void SetEnable(bool enable)
		{
			m_Enable = enable;
			if (!enable)
			{
				m_State = State.Idle;
			}
		}

		public void SetPress(bool press)
		{
			m_State = press
				? m_State == State.Idle || m_State == State.Up
					? State.Down
					: State.HoldDown
				: m_State == State.HoldDown || m_State == State.Down
					? State.Up
					: State.Idle;
		}

		public State GetState()
		{
			return m_State;
		}

		public bool IsPress()
		{
			return m_State == State.Down || m_State == State.HoldDown;
		}

		public enum State
		{
			Idle,
			HoldDown,
			Up,
			Down,
		}
	}
}