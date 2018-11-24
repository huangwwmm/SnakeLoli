using UnityEngine;

public class slPlayerController : slBaseController
{
	private Camera m_Camera;
	private slHUD m_HUD;

	protected override void HandleInitialize()
	{
		m_Camera = transform.Find("Camera").GetComponent<Camera>();
		m_HUD = transform.Find("HUD").GetComponent<slHUD>();
	}

	protected override void HandleDispose()
	{
		m_HUD = null;
		m_Camera = null;
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
		m_Camera.transform.localPosition = cameraPosition;

		Vector2 move = Vector2.zero;
		if (Input.GetKey(KeyCode.A))
		{
			move.x = -1;
		}
		if (Input.GetKey(KeyCode.D))
		{
			move.x = 1;
		}
		if (Input.GetKey(KeyCode.W))
		{
			move.y = 1;
		}
		if (Input.GetKey(KeyCode.S))
		{
			move.y = -1;
		}
		if (move != Vector2.zero)
			m_Snake.MoveDirection = move;
	}
}