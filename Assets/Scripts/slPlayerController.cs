using UnityEngine;

public class slPlayerController : slBaseController
{
	public float CameraSmoothTime;

	private slCamera m_Camera;
	private slHUD m_HUD;
	/// <summary>
	/// cache for update call
	/// </summary>
	private hwmInput m_Input;

	private slSkill[] m_Skills;

	/// <summary>
	/// UNDONE will move to player data
	/// </summary>
	private slSkill.SkillType[] m_UsedSkill = new slSkill.SkillType[]
	{
		slSkill.SkillType.SpeedUp,
		slSkill.SkillType.Gluttony,
		slSkill.SkillType.Stealth,
		slSkill.SkillType.Radar,
		slSkill.SkillType.RemainsFoodContamination
	};

	public slCamera GetCamera()
	{
		return m_Camera;
	}

	public override bool IsPlayer()
	{
		return true;
	}

	protected override void HandleInitialize()
	{
		m_Camera = transform.Find("Camera").GetComponent<slCamera>();
		m_HUD = transform.Find("HUD").GetComponent<slHUD>();
		m_Input = hwmSystem.GetInstance().GetInput();

		m_Input.GetButton(hwmConstants.ButtonIndex.Menu).SetEnable(true); // TEMP
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

		m_HUD.OnSetControllerSnake();

		m_Skills = new slSkill[m_UsedSkill.Length];
		for (int iSkill = 0; iSkill < m_Skills.Length; iSkill++)
		{
			slSkill.SkillType skillType = m_UsedSkill[iSkill];
			m_Skills[iSkill] = m_HUD.PopSkill(skillType, iSkill);
			m_Skills[iSkill].Active(m_Snake, slConstants.SKILL_BUTTONINDEXS[iSkill]);
		}
	}

	protected override void HandleUnController()
	{
		for (int iSkill = 0; iSkill < m_Skills.Length; iSkill++)
		{
			slSkill iterSkill = m_Skills[iSkill];
			iterSkill.Deactive();
			m_HUD.PushSkill(iterSkill);
		}
		m_Skills = null;

		m_HUD.OnUnControllerSnake();

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

		for (int iSkill = 0; iSkill < m_Skills.Length; iSkill++)
		{
			m_Skills[iSkill].DoUpdate();
		}

		// TEMP
		if (m_Input.GetButton(hwmConstants.ButtonIndex.Menu).GetState() == hwmInput.Button.State.Up)
		{
			Application.Quit();
		}

		IsHitPredict();
	}
}