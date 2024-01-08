using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonDataService : IDataService
{

	public bool SaveData<T>(string RelativePath, T Data, bool Encrypted)
	{
		string path = Application.persistentDataPath + RelativePath;
		try
		{
			if (File.Exists(path))
			{
				Debug.Log("Data exists. Delete old file and write new");
				File.Delete(path);
			}
			else
			{
				Debug.Log("First time write file");
			}
			using FileStream stream = File.Create(path);
			stream.Close();
			File.WriteAllText(path, JsonConvert.SerializeObject(Data));
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError($"Unable to save data due to: {ex.Message} {ex.StackTrace}");
			return false;
		}
	}

	public T LoadData<T>(string RelativePath, bool Encrypted)
	{
		string path = Application.persistentDataPath + RelativePath;

		if (!File.Exists(path))
		{
			Debug.LogError($"Cannot load file at {path}. File does not exist!");
			throw new FileNotFoundException(path);
		}

		try
		{
			T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
			return data;
		}catch (Exception ex)
		{
			Debug.LogError($"failed to load data due to: {ex.Message} {ex.StackTrace}");
			throw ex;
		}
	}
}
