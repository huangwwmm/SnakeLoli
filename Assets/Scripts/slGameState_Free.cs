using System.Collections.Generic;
public class slGameState_Free : hwmGameState 
{
	protected override void HandleInProgress()
	{
		slWorld.GetInstance().GetSnakeSystem().GetQuadtree().MergeAndSplitAllNode();
	}
}