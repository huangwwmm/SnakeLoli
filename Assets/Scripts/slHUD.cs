using UnityEngine;

public class slHUD : MonoBehaviour
{
	public hwmBaseInputVirtualJoystick MoveVirtualJoysitck;
	public hwmInputButtonSimple SpeedUpButton;

	public void SetCanSpeedUp(bool canSpeedUp)
	{
		SpeedUpButton.GetUIButton().interactable = canSpeedUp;
	}

	public void SetSpeedUpButtonDisplap(bool display)
	{
		SpeedUpButton.gameObject.SetActive(display);
	}

	public void SetDisplayMoveVirtualJoystick(bool display)
	{
		MoveVirtualJoysitck.gameObject.SetActive(display);
	}
}