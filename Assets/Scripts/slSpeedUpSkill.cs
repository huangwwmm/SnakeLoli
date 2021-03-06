﻿using System;

public class slSpeedUpSkill : slSkill 
{
	public Properties MyProperties;

	public override void Active(slSnake snake, hwmConstants.ButtonIndex buttonIndex)
	{
		base.Active(snake, buttonIndex);

		snake.OnSpeedUpMovement += OnSpeedUpMovement;
	}

	public override void Deactive()
	{
		m_Snake.OnSpeedUpMovement -= OnSpeedUpMovement;

		base.Deactive();
	}


	public override void DoUpdate()
	{
		bool canUse = CanSpeedUp();

		m_InputButton.GetUIButton().interactable = canUse;

		hwmInput.Button skillInput = hwmSystem.GetInstance().GetInput().GetButton(m_ButtonIndex);
		skillInput.SetEnable(canUse);

		m_Snake.ChangeSpeedState(canUse && skillInput.IsPress()
			? slSnake.SpeedState.SpeedUp 
			: slSnake.SpeedState.Normal); 
	}

	private bool CanSpeedUp()
	{
		return m_Snake.GetSpeedState() == slSnake.SpeedState.SpeedUp
			? CanKeepSpeedUp()
			: CanEnterSpeedUp();
	}

	private bool CanEnterSpeedUp()
	{
		return m_Snake.GetPower() >= m_Snake.GetTweakableProperties().NodeToPower * MyProperties.EnterRequiredNode;
	}

	private bool CanKeepSpeedUp()
	{
		return m_Snake.GetPower() >= m_Snake.GetTweakableProperties().NodeToPower * MyProperties.KeepRequiredNode;
	}

	private void OnSpeedUpMovement(EventArgs args)
	{
		args.CanKeepSkill = CanKeepSpeedUp();
		if (args.CanKeepSkill)
		{
			args.CostPower = MyProperties.CostPower;
			args.Effect = MyProperties.MoveNodeCount;
		}
	}

	[System.Serializable]
	public class Properties
	{
		public int CostPower;
		public int EnterRequiredNode;
		public int KeepRequiredNode;
		public int MoveNodeCount;
	}
}