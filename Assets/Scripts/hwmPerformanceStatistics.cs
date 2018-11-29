using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public interface hwmIPerformanceStatistics
{
	void Initialize();
	void Dispose();
	void LogAndRecord();
	hwmPerformanceStatisticsItem LoadOrCreateItem(string itemName, bool ignoreHistoryOnLoad = false);
	hwmPerformanceStatisticsItem Start(string itemName);
	void Start(hwmPerformanceStatisticsItem item);
	hwmPerformanceStatisticsItem Finish(string itemName);
	void Finish(hwmPerformanceStatisticsItem item);
	hwmPerformanceStatisticsItem ClearHistory(string itemName);
	void ClearHistory(hwmPerformanceStatisticsItem item);
	hwmPerformanceStatisticsItem Pause(string itemName);
	void Pause(hwmPerformanceStatisticsItem item);
	hwmPerformanceStatisticsItem Resume(string itemName);
	void Resume(hwmPerformanceStatisticsItem item);
}

public class hwmPerformanceStatistics : hwmIPerformanceStatistics
{
	private string m_RecordDirectory;
	private Dictionary<string, hwmPerformanceStatisticsItem> m_Items;

	public void Initialize()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		m_RecordDirectory = Application.dataPath + "/../Temp/PerformanceStatistics/";
#elif UNITY_ANDROID
		m_RecordDirectory = Application.persistentDataPath + "/PerformanceStatistics/";
#endif
		if (!Directory.Exists(m_RecordDirectory))
		{
			Directory.CreateDirectory(m_RecordDirectory);
		}

		m_Items = new Dictionary<string, hwmPerformanceStatisticsItem>();
	}

	public void Dispose()
	{
		m_Items.Clear();
		m_Items = null;
	}

	public hwmPerformanceStatisticsItem LoadOrCreateItem(string itemName, bool ignoreHistoryOnLoad = false)
	{
		hwmPerformanceStatisticsItem item;
		if (!m_Items.TryGetValue(itemName, out item))
		{
			item = new hwmPerformanceStatisticsItem();
			item._Name = itemName;
			item._Stopwatch = new Stopwatch();
			item._Stopwatch.Reset();
			item._Historys = new List<hwmPerformanceStatisticsItem.History>();

			if (!ignoreHistoryOnLoad)
			{
				string itemFilePath = m_RecordDirectory + itemName + ".txt";
				if (File.Exists(itemFilePath))
				{
					string[] historyLines = File.ReadAllLines(itemFilePath);
					for (int iHistory = 0; iHistory < historyLines.Length; iHistory++)
					{
						string iterLine = historyLines[iHistory];
						hwmPerformanceStatisticsItem.History history;
						if (hwmPerformanceStatisticsItem.TryDeserializeHistory(iterLine, out history))
						{
							item._Historys.Add(history);
						}
					}
				}
			}

			m_Items.Add(itemName, item);
		}
		return item;
	}

	public hwmPerformanceStatisticsItem Start(string itemName)
	{
		hwmPerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		Start(item);
		return item;
	}

	public void Start(hwmPerformanceStatisticsItem item)
	{
		item._Stopwatch.Reset();
		item._Stopwatch.Start();
	}

	public hwmPerformanceStatisticsItem Finish(string itemName)
	{
		hwmPerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		Finish(item);
		return item;
	}

	public void Finish(hwmPerformanceStatisticsItem item)
	{
		item._Stopwatch.Stop();
		hwmPerformanceStatisticsItem.History history = new hwmPerformanceStatisticsItem.History();
		history._Ticks = item._Stopwatch.ElapsedTicks;
		item._Historys.Add(history);
	}

	public hwmPerformanceStatisticsItem ClearHistory(string itemName)
	{
		hwmPerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		ClearHistory(item);
		return item;
	}

	public void ClearHistory(hwmPerformanceStatisticsItem item)
	{
		item._Historys.Clear();
	}

	public void LogAndRecord()
	{
		List<string> historyStrs = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("PerformanceStatistics:");
		foreach (KeyValuePair<string, hwmPerformanceStatisticsItem> item in m_Items)
		{
			string itemFilePath = m_RecordDirectory + item.Value._Name + ".txt";
			if (File.Exists(itemFilePath))
			{
				File.Delete(itemFilePath);
			}

			List<hwmPerformanceStatisticsItem.History> historys = item.Value._Historys;
			long[] ticks = new long[historys.Count];
			historyStrs.Clear();
			for (int iHistory = Mathf.Max(0, historys.Count - hwmPerformanceStatisticsItem.MAX_RECORD_HISTORY_COUNT); iHistory < historys.Count; iHistory++)
			{
				hwmPerformanceStatisticsItem.History iterHistory = historys[iHistory];
				ticks[iHistory] = iterHistory._Ticks;
				historyStrs.Add(hwmPerformanceStatisticsItem.SerializeHistory(iterHistory));
			}
			File.WriteAllLines(itemFilePath, historyStrs.ToArray());

			if (historys.Count > 0)
			{
				ValueAnalysis tickAnalysis = new ValueAnalysis(ticks);
				stringBuilder.AppendLine(string.Format("Name: {0} Count:{1} AvgTick:{2:F2}"
					, item.Value._Name
					, historys.Count
					, tickAnalysis.Avg));
			}
		}
		historyStrs.Clear();
		historyStrs = null;

		UnityEngine.Debug.Log(stringBuilder.ToString());
	}

	public hwmPerformanceStatisticsItem Pause(string itemName)
	{
		hwmPerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		Pause(item);
		return item;
	}

	public void Pause(hwmPerformanceStatisticsItem item)
	{
		item._Stopwatch.Stop();
	}

	public hwmPerformanceStatisticsItem Resume(string itemName)
	{
		hwmPerformanceStatisticsItem item = LoadOrCreateItem(itemName);
		Resume(item);
		return item;
	}

	public void Resume(hwmPerformanceStatisticsItem item)
	{
		item._Stopwatch.Start();
	}

	private struct ValueAnalysis
	{
		public int Count;
		public long Max;
		public long Min;
		public double Avg;

		public ValueAnalysis(long[] values)
		{
			Count = values.Length;
			Max = long.MinValue;
			Min = long.MaxValue;

			double total = 0;
			for (int iValue = 1; iValue < Count; iValue++)
			{
				long iterValue = values[iValue];
				Max = iterValue > Max ? iterValue : Max;
				Min = iterValue < Min ? iterValue : Min;
				total += iterValue;
			}

			if (Count > 2)
			{
				Avg = (total - Min - Max) / (Count - 2);
			}
			else
			{
				Avg = total / Count;
			}
		}
	}
}

public class hwmEmptyPerformanceStatistics : hwmIPerformanceStatistics
{
	public hwmPerformanceStatisticsItem ClearHistory(string itemName)
	{
		return null;
	}

	public void ClearHistory(hwmPerformanceStatisticsItem item)
	{
	}

	public void Dispose()
	{
	}

	public hwmPerformanceStatisticsItem Finish(string itemName)
	{
		return null;
	}

	public void Finish(hwmPerformanceStatisticsItem item)
	{
	}

	public void Initialize()
	{
	}

	public hwmPerformanceStatisticsItem LoadOrCreateItem(string itemName, bool ignoreHistoryOnLoad = false)
	{
		return null;
	}

	public void LogAndRecord()
	{
	}

	public hwmPerformanceStatisticsItem Pause(string itemName)
	{
		return null;
	}

	public void Pause(hwmPerformanceStatisticsItem item)
	{
	}

	public hwmPerformanceStatisticsItem Resume(string itemName)
	{
		return null;
	}

	public void Resume(hwmPerformanceStatisticsItem item)
	{
	}

	public hwmPerformanceStatisticsItem Start(string itemName)
	{
		return null;
	}

	public void Start(hwmPerformanceStatisticsItem item)
	{
	}
}

public class hwmPerformanceStatisticsItem
{
	public const int MAX_RECORD_HISTORY_COUNT = 8192;

	internal string _Name;
	internal Stopwatch _Stopwatch;
	internal List<History> _Historys;

	public static string SerializeHistory(History history)
	{
		return history._Ticks.ToString();
	}

	public static bool TryDeserializeHistory(string str, out History history)
	{
		return long.TryParse(str, out history._Ticks);
	}

	public struct History
	{
		internal long _Ticks;
	}
}