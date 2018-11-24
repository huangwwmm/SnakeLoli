using UnityEngine;

public class slPlayerController : slBaseController
{
	public float CameraSmoothTime;

	private Camera m_Camera;
	private slHUD m_HUD;
	/// <summary>
	/// cache for update call
	/// </summary>
	private hwmInput m_Input;
	private Vector3 m_CameraVelocity = Vector3.zero;

	protected override void HandleInitialize()
	{
		m_Camera = transform.Find("Camera").GetComponent<Camera>();
		m_HUD = transform.Find("HUD").GetComponent<slHUD>();
		m_Input = hwmSystem.GetInstance().GetInput();
	}

	protected override void HandleDispose()
	{
		m_Input = null;
		m_HUD = null;
		m_Camera = null;
	}

	protected override void HandleSetController()
	{
		Vector3 cameraPosition = m_Camera.transform.localPosition;
		Vector3 snakeHaedPosition = m_Snake.GetHeadPosition();
		cameraPosition.x = snakeHaedPosition.x;
		cameraPosition.y = snakeHaedPosition.y;
		m_Camera.transform.localPosition = cameraPosition;
	}

	protected void Update()
	{
		if (m_Snake == null)
		{
			return;
		}

		Vector3 cameraPosition = m_Camera.transform.localPosition;
		Vector3 snakeHaedPosition = m_Snake.GetHeadPosition();
		cameraPosition.x = snakeHaedPosition.x;
		cameraPosition.y = snakeHaedPosition.y;
		m_Camera.transform.localPosition = Vector3.SmoothDamp(m_Camera.transform.localPosition, cameraPosition, ref m_CameraVelocity, CameraSmoothTime);

		Vector2 axis = new Vector2(m_Input.GetAxisValue(hwmConstants.AxisIndex.MoveX)
			, m_Input.GetAxisValue(hwmConstants.AxisIndex.MoveY));

		if (axis != Vector2.zero)
		{
			m_Snake.MoveDirection = axis;
		}
	}
}