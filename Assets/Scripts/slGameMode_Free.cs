using System.Collections;
using UnityEngine;

public class slGameMode_Free : hwmGameMode 
{
	protected override IEnumerator HandleStartPlay_Co()
	{
		yield return StartCoroutine(slWorld.GetInstance().GetFoodSystem().EnterMap_Co());
	}
}