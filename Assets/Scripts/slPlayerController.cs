﻿using UnityEngine;

public class slPlayerController : slBaseController
{
	public float CameraSmoothTime;

	private Camera m_Camera;
	private slHUD m_HUD;
	/// <summary>
	/// cache for update call
	/// </summary>
	private hwmInput m_Input;

	public override bool IsPlayer()
	{
		return true;
	}

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

		m_Input.JoystickCursor.SetAvailable(false);

		m_Input.GetAxis(hwmConstants.AxisIndex.MoveX).SetEnable(true);
		m_Input.GetAxis(hwmConstants.AxisIndex.MoveY).SetEnable(true);
		m_Input.GetButton(hwmConstants.ButtonIndex.Skill1).SetEnable(true);
		m_Input.GetButton(hwmConstants.ButtonIndex.Skill2).SetEnable(true);

		m_HUD.SetDisplayMoveVirtualJoystick(true);
		m_HUD.SetSpeedUpButtonDisplap(true);
	}

	protected override void HandleUnController()
	{
		m_HUD.SetSpeedUpButtonDisplap(false);
		m_HUD.SetDisplayMoveVirtualJoystick(false);

		m_Input.GetAxis(hwmConstants.AxisIndex.MoveX).SetEnable(false);
		m_Input.GetAxis(hwmConstants.AxisIndex.MoveY).SetEnable(false);
		m_Input.GetButton(hwmConstants.ButtonIndex.Skill1).SetEnable(false);
		m_Input.GetButton(hwmConstants.ButtonIndex.Skill2).SetEnable(false);

		m_Input.JoystickCursor.SetAvailable(true);
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

		Vector2 axis = new Vector2(m_Input.GetAxis(hwmConstants.AxisIndex.MoveX).GetValue()
			, m_Input.GetAxis(hwmConstants.AxisIndex.MoveY).GetValue());

		if (axis != Vector2.zero)
		{
			m_Snake.TargetMoveDirection = axis.normalized;
		}

		bool canSpeedUp = m_Snake.CanSpeedUp();
		m_HUD.SetCanSpeedUp(canSpeedUp);
		hwmInput.Button speedUpButton = m_Input.GetButton(hwmConstants.ButtonIndex.Skill1);
		speedUpButton.SetEnable(canSpeedUp);
		m_Snake.TryChangeSpeedState(speedUpButton.IsPress() ? slSnake.SpeedState.SpeedUp : slSnake.SpeedState.Normal);

		if (m_Input.GetButton(hwmConstants.ButtonIndex.Skill2).GetState() == hwmInput.Button.State.Up)
		{
			m_Snake.TestSkill();
		}
	}
}