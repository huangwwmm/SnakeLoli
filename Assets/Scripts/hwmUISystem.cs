﻿using System.Collections.Generic;
using UnityEngine;

public class hwmUISystem
{
	private Dictionary<string, hwmUIRoot> m_UIRoots;

	public void Initialize()
	{
		m_UIRoots = new Dictionary<string, hwmUIRoot>();
	}

	public void Dispose()
	{
		foreach (KeyValuePair<string, hwmUIRoot> kv in m_UIRoots)
		{
			kv.Value.OnUIRootDispose();
		}
		m_UIRoots.Clear();
		m_UIRoots = null;
	}

	public T InstantiateUIRoot<T>(string uiRootName) where T : hwmUIRoot
	{
		hwmDebug.Assert(!m_UIRoots.ContainsKey(uiRootName), "!m_UIRoots.ContainsKey(uiRootName)");
		GameObject uiRootGameObject = Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.UIRoot, uiRootName)) as GameObject;
		Object.DontDestroyOnLoad(uiRootGameObject);
		hwmUIRoot uiRoot = uiRootGameObject.GetComponent<hwmUIRoot>();
		hwmDebug.Assert(uiRoot != null, "uiRoot != null");
		m_UIRoots.Add(uiRootName, uiRoot);
		uiRoot.OnUIRootInitialize();
		return uiRoot as T;
	}

	public void DisposeUIRoot(string uiRootName)
	{
		hwmDebug.Assert(m_UIRoots.ContainsKey(uiRootName), "m_UIRoots.ContainsKey(uiRootName)");
		hwmUIRoot uiRoot = m_UIRoots[uiRootName];
		hwmDebug.Assert(uiRoot != null, "uiRoot != null");
		uiRoot.OnUIRootDispose();
		m_UIRoots.Remove(uiRootName);
	}
}