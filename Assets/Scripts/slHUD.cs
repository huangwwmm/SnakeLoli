using System;
using System.Linq;
using UnityEngine;

public class slHUD : MonoBehaviour
{
	public hwmBaseInputVirtualJoystick MoveVirtualJoysitck;

	public Transform[] SkillAnchors;
	public Transform SkillCacheRoot;

	/// <summary>
	/// sorted by skill type
	/// </summary>
	private slSkill[] m_AllSkill;
	/// <summary>
	/// sorted by anchor index
	/// </summary>
	private slSkill[] m_UsedSkills;

	public void SetDisplayMoveVirtualJoystick(bool display)
	{
		MoveVirtualJoysitck.gameObject.SetActive(display);
	}

	public void OnSetControllerSnake()
	{
		SetDisplayMoveVirtualJoystick(true);
	}

	public void OnUnControllerSnake()
	{
		SetDisplayMoveVirtualJoystick(false);
	}

	public slSkill PopSkill(slSkill.SkillType skillType, int anchors)
	{
		hwmDebug.Assert(anchors >= 0 && anchors < SkillAnchors.Length, "anchors >= 0 && anchors < SkillAnchors.Length");

		slSkill skill = m_AllSkill[(int)skillType];
		hwmDebug.Assert(!m_UsedSkills.Contains(skill), "!m_UsedSkills.Contains(skill)");
		m_UsedSkills[anchors] = skill;
		skill.transform.SetParent(SkillAnchors[anchors], false);
		skill.transform.localPosition = Vector3.zero;

		return skill;
	}

	public void PushSkill(slSkill skill)
	{
		bool containsSkill = false;
		for (int iSkill = 0; iSkill < m_UsedSkills.Length; iSkill++)
		{
			if (m_UsedSkills[iSkill] == skill)
			{
				m_UsedSkills[iSkill] = null;
				containsSkill = true;
				break;
			}
		}

		hwmDebug.Assert(containsSkill, "containsSkill");

		skill.transform.SetParent(SkillCacheRoot);
	}

	protected void Awake()
	{
		hwmDebug.Assert(SkillAnchors.Length == slConstants.SKILL_BUTTONINDEXS.Length, "SkillAnchors.Length == slConstants.SKILL_BUTTONINDEXS.Length");
		m_UsedSkills = new slSkill[slConstants.SKILL_BUTTONINDEXS.Length];

		slSkill[] skills = SkillCacheRoot.GetComponentsInChildren<slSkill>();
		SkillCacheRoot.gameObject.SetActive(false);
		m_AllSkill = new slSkill[(int)slSkill.SkillType.Count];
		hwmDebug.Assert(skills.Length == m_AllSkill.Length, "skills.Length == m_AllSkill.Length");
		for (int iSkill = 0; iSkill < skills.Length; iSkill++)
		{
			slSkill iterSkill = skills[iSkill];
			m_AllSkill[(int)iterSkill.MyType] = iterSkill;
		}
	}

	protected void OnDestroy()
	{
		m_UsedSkills = null;
		m_AllSkill = null;
	}
}