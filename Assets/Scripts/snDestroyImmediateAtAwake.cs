using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snDestroyImmediateAtAwake : MonoBehaviour
{
	protected void Awake()
	{
		DestroyImmediate(gameObject);
	}
}