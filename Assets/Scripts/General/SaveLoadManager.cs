using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager
{

    public SaveLoadManager()
    {
    }

    public Level LoadLevelData(string level)
    {
        string levelDataJson = Resources.Load("Levels" + "/Level" + level).ToString();
        return JsonUtility.FromJson<Level>(levelDataJson);
    }

    public void WriteLevelData(Level level)
    {
        #if UNITY_EDITOR
        string path = "Assets/Resources/Levels";
        string levelDataJson = JsonUtility.ToJson(level);

        File.WriteAllText(path + "/Level" + level.level + ".json", levelDataJson);
        #endif
    }
}
