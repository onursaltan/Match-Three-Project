using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager
{
    private string dataPath;

    public SaveLoadManager()
    {
        dataPath = Application.persistentDataPath;
    }

    public Level LoadLevelData(string level)
    {
        string fullLevelPath = dataPath + "/Level" + level + ".json";
        string levelDataJson = File.ReadAllText(fullLevelPath);
        return JsonUtility.FromJson<Level>(levelDataJson);
    }

    public void WriteLevelData(Level level)
    {
        string levelDataJson = JsonUtility.ToJson(level);
        File.WriteAllText(Application.persistentDataPath + "/Level" + level.level + ".json", levelDataJson);
    }
}
