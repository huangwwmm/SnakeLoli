﻿using UnityEngine;
using UnityEngine.UI;

public class slSkillRemainsFoodContamination : slSkill
{
	public Properties MyProperties;
	public Text CDText;

	private float m_CD;
	private bool m_EnableSkill;
	private float m_SkillRemainTime;

	public override void Active(slSnake snake, hwmConstants.ButtonIndex buttonIndex)
	{
		base.Active(snake, buttonIndex);

		m_CD = MyProperties.CD;
		m_EnableSkill = false;
		CDText.color = Color.white;
	}

	public override void DoUpdate()
	{
		if (m_EnableSkill)
		{
			m_SkillRemainTime -= Time.deltaTime;
			if (m_SkillRemainTime < 0)
			{
				m_Snake.EnableRemainsFoodContamination(false, 0);
				m_EnableSkill = false;
				m_CD = MyProperties.CD;
				CDText.color = Color.white;
			}
			else
			{
				CDText.text = Mathf.CeilToInt(m_SkillRemainTime).ToString();
			}
		}
		else
		{
			m_CD -= Time.deltaTime;
			bool canUse = m_CD <= 0;
			m_InputButton.GetUIButton().interactable = canUse;
			hwmInput.Button skillInput = hwmSystem.GetInstance().GetInput().GetButton(m_ButtonIndex);
			skillInput.SetEnable(canUse);
			CDText.text = canUse || m_CD < 0
				? ""
				: Mathf.CeilToInt(m_CD).ToString();

			if (skillInput.GetState() == hwmInput.Button.State.Up)
			{
				m_SkillRemainTime = MyProperties.Duration;
				m_InputButton.GetUIButton().interactable = false;
				skillInput.SetEnable(false);
				CDText.color = Color.red;
				m_EnableSkill = true;
				m_Snake.EnableRemainsFoodContamination(true, MyProperties.Power);
			}
		}
	}

	[System.Serializable]
	public class Properties
	{
		public float CD;
		public float Duration;
		public float Power;
	}
}