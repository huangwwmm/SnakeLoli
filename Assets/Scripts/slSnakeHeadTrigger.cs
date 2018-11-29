using System;
using UnityEngine;

public class slSnakeHeadTrigger : MonoBehaviour
{
	public Action<Collider2D> OnTriggerEnter;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (OnTriggerEnter != null
			&& gameObject.name != collider.gameObject.name)
		{
			OnTriggerEnter(collider);
		}
	}
}