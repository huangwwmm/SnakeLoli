using UnityEngine;
using UnityEngine.EventSystems;

public class snEventSystemBaseInput : BaseInput
{
	public override Vector2 mousePosition
	{
		get
		{
			return snSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
				? snSystem.GetInstance().GetInput().JoystickCursor.GetCusrorPosition_ScreenSpace()
				: (Vector2)Input.mousePosition;
		}
	}

	public override Vector2 mouseScrollDelta
	{
		get
		{
			return snSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
				? Vector2.zero
				: Input.mouseScrollDelta;
		}
	}

	/// <summary>
	/// use <see cref="snJoystickCursor"/>
	/// </summary>
	/// <returns>always false</returns>
	public override bool GetButtonDown(string buttonName)
	{
		return false;
	}

	public override bool GetMouseButton(int button)
	{
		return snSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
			? button == 0 // joystickCursor only support button0
				? snSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == snJoystickCursor.PressState.Hold
					|| snSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == snJoystickCursor.PressState.Hold
				: false
			: Input.GetMouseButton(button);
	}

	public override bool GetMouseButtonDown(int button)
	{
		return snSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
			? button == 0 // joystickCursor only support button0
				? snSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == snJoystickCursor.PressState.Down
				: false
			: Input.GetMouseButtonDown(button);
	}

	public override bool GetMouseButtonUp(int button)
	{
		return snSystem.GetInstance().GetInput().JoystickCursor.IsDisplay()
			? button == 0 // joystickCursor only support button0
				? snSystem.GetInstance().GetInput().JoystickCursor.GetPressState() == snJoystickCursor.PressState.Up
				: false
			: Input.GetMouseButtonUp(button);
	}

	// UNDONE add support for Mobile Device
	//public virtual int touchCount { get; }
	//public virtual Touch GetTouch(int index);
}