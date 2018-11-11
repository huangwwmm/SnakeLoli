using UnityEngine;
using UnityEngine.EventSystems;

public class hwmEventSystemBaseInput : BaseInput
{
	public override Vector2 mousePosition
	{
		get
		{
			return slSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
				? slSystem.GetInstance().GetInput().JoystickCursor.GetCusrorPosition_ScreenSpace()
				: (Vector2)Input.mousePosition;
		}
	}

	public override Vector2 mouseScrollDelta
	{
		get
		{
			return slSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
				? Vector2.zero
				: Input.mouseScrollDelta;
		}
	}

	/// <summary>
	/// use <see cref="hwmJoystickCursor"/>
	/// </summary>
	/// <returns>always false</returns>
	public override bool GetButtonDown(string buttonName)
	{
		return false;
	}

	public override bool GetMouseButton(int button)
	{
		return slSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
			? button == 0 // joystickCursor only support button0
				? slSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == hwmJoystickCursor.PressState.Hold
					|| slSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == hwmJoystickCursor.PressState.Hold
				: false
			: Input.GetMouseButton(button);
	}

	public override bool GetMouseButtonDown(int button)
	{
		return slSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
			? button == 0 // joystickCursor only support button0
				? slSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == hwmJoystickCursor.PressState.Down
				: false
			: Input.GetMouseButtonDown(button);
	}

	public override bool GetMouseButtonUp(int button)
	{
		return slSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
			? button == 0 // joystickCursor only support button0
				? slSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == hwmJoystickCursor.PressState.Up
				: false
			: Input.GetMouseButtonUp(button);
	}

	// UNDONE add support for Mobile Device
	//public virtual int touchCount { get; }
	//public virtual Touch GetTouch(int index);
}