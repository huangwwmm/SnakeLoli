using System;
using UnityEngine;

public static class hwmObserver 
{
	public static Action<hwmActor> OnActorCreate;
	public static Action<hwmActor> OnActorDestroy;

	public static void NotifyActorCreate(hwmActor actor)
	{
		if (OnActorCreate != null)
		{
			OnActorCreate(actor);
		}
	}

	public static void NotifyActorDestroy(hwmActor actor)
	{
		if (OnActorDestroy != null)
		{
			OnActorDestroy(actor);
		}
	}
}