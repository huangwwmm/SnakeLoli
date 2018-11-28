using UnityEngine;

public class slRadarSkill : slSkill
{
	public Properties MyProperties;

	private bool m_IsRaderState;

	public override void DoUpdate()
	{
		bool canUse = CanRadar();
		hwmInput.Button skillInput = hwmSystem.GetInstance().GetInput().GetButton(m_ButtonIndex);

		if (m_IsRaderState)
		{
			if (canUse
				&& skillInput.GetState() != hwmInput.Button.State.Up)
			{
				m_Snake.CostPower(Time.deltaTime * MyProperties.CostPower);
			}
			else
			{
				slWorld.GetInstance().GetPlayerController().GetCamera().EnableRadarState(false, MyProperties.CameraSizeProperties);
				m_IsRaderState = false;
			}
		}
		else
		{
			m_InputButton.GetUIButton().interactable = canUse;

			skillInput.SetEnable(canUse);

			if (skillInput.GetState() == hwmInput.Button.State.Up)
			{
				slWorld.GetInstance().GetPlayerController().GetCamera().EnableRadarState(true, MyProperties.CameraSizeProperties);
				m_IsRaderState = true;
			}
		}
	}

	private bool CanRadar()
	{
		return m_Snake.GetPower() >= m_Snake.GetTweakableProperties().NodeToPower
			* (m_IsRaderState
				? MyProperties.KeepRequiredNode
				: MyProperties.EnterRequiredNode);

	}

	[System.Serializable]
	public class Properties
	{
		public float CostPower;
		public int EnterRequiredNode;
		public int KeepRequiredNode;
		public slCamera.SizeProperties CameraSizeProperties;
	}
}