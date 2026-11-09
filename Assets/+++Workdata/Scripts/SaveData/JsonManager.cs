using System.IO;
using UnityEngine;

public static class JsonManager
{
    #region PlayerPref
    public static void SaveToPlayerPref<T>(string playerPrefKey, T data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log(json);
        PlayerPrefs.SetString(playerPrefKey, json);
        PlayerPrefs.Save();
    }

    public static T LoadFromPlayerPref<T>(string playerPrefKey)
    {
        return JsonUtility.FromJson<T>(PlayerPrefs.GetString(playerPrefKey));
    }

    #endregion
    
    #region TextFile

    public static void SaveToTextFile<T>(string path, T data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }
    
    public static void SaveToTextFile<T>(string path, string fileName, T data)
    {
        string json = JsonUtility.ToJson(data);
        string newPath = path + "\\" + fileName;
        File.WriteAllText(newPath, json);
    }

    public static T LoadFromTextFile<T>(string path)
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }
    
    public static T LoadFromTextFile<T>(string path, string fileName)
    {
        string newPath = path + "\\" + fileName;
        string json = File.ReadAllText(newPath);
        return JsonUtility.FromJson<T>(json);
    }
    
    #endregion
}