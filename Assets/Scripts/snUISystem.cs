using System.Collections.Generic;
using UnityEngine;

public class snUISystem
{
	private Dictionary<string, snUIRoot> m_UIRoots;

	public void Initialize()
	{
		m_UIRoots = new Dictionary<string, snUIRoot>();
	}

	public void Destroy()
	{
		foreach (KeyValuePair<string, snUIRoot> kv in m_UIRoots)
		{
			kv.Value.OnUIRootDestroy();
		}
		m_UIRoots.Clear();
		m_UIRoots = null;
	}

	public T InstantiateUIRoot<T>(string uiRootName) where T : snUIRoot
	{
		snDebug.Assert(!m_UIRoots.ContainsKey(uiRootName), "!m_UIRoots.ContainsKey(uiRootName)");
		GameObject uiRootGameObject = Object.Instantiate(snSystem.GetInstance().GetAssetLoader().LoadPrefab(snAssetLoader.AssetType.UIRoot, uiRootName)) as GameObject;
		snUIRoot uiRoot = uiRootGameObject.GetComponent<snUIRoot>();
		snDebug.Assert(uiRoot != null, "uiRoot != null");
		m_UIRoots.Add(uiRootName, uiRoot);
		uiRoot.OnUIRootInitialize();
		uiRoot.OnUIRootDisplay();
		return uiRoot as T;
	}

	public void DestroyUIRoot(string uiRootName)
	{
		snDebug.Assert(m_UIRoots.ContainsKey(uiRootName), "m_UIRoots.ContainsKey(uiRootName)");
		snUIRoot uiRoot = m_UIRoots[uiRootName];
		snDebug.Assert(uiRoot != null, "uiRoot != null");
		uiRoot.OnUIRootHide();
		uiRoot.OnUIRootDestroy();
		m_UIRoots.Remove(uiRootName);
	}
}