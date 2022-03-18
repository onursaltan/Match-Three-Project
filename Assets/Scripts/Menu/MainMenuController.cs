using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public GameObject levelButtons;

    public Button levelButton;

    public List<Button> levelButtonList;

    // Start is called before the first frame update
    void Start()
    {
        int totalLevelCount = Resources.LoadAll<TextAsset>("Levels").Length;

        LevelManager.totalLevels = totalLevelCount;

        for (int i = 0; i < LevelManager.totalLevels; i++)
        {
            Button menuLevelButton = Instantiate(levelButton, levelButtons.transform);
            menuLevelButton.transform.GetChild(0).GetComponent<Text>().text = "Seviye " + (i + 1).ToString();
            levelButtonList.Add(menuLevelButton);
        }

        foreach (Button buttonInstance in levelButtonList)
        {
            buttonInstance.onClick.AddListener(delegate { OpenLevel(levelButtonList.IndexOf(buttonInstance)+1); });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenLevel(int level)
    {
        if (level < LevelManager.totalLevels + 1)
        {
            LevelManager.levelNumber = level;
            SceneManager.LoadScene(1);
        }
    }


    


}
