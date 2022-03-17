using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public Text[] goalTexts;

    public GameObject[] goalsUI;

    public GameObject canvas;

    private int currentLevel = 1;

    public Sprite[] shapeSprites;

    private List<Goal> _goals;

    private int remainingGoals;

    public GameObject levelPassed;
    public GameObject tint;
    SaveLoadManager slm = new SaveLoadManager();
    Level myLevel;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Application.targetFrameRate = 60;
            _instance = this;
        }
    }

    public void Start()
    {
        OpenLevel(LevelManager.levelNumber);
    }

    public void CheckGoal(ShapeType shapeType, ShapeColor shapeColor = ShapeColor.None)
    {
        for (int i = 0; i < _goals.Count; i++)
        {
            if (_goals[i].count > 0 && _goals[i].shapeType == shapeType && _goals[i].shapeColor == shapeColor)
            {
                _goals[i].count--;
                goalsUI[i].transform.GetChild(1).GetComponent<Text>().text = _goals[i].count.ToString();
                if (_goals[i].count == 0)
                {
                    remainingGoals--;
                    CompleteGoal(goalsUI[i]);
                }
                if (remainingGoals == 0)
                {
                    LevelPassed();
                }
            }
        }        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveLoadManager slm = new SaveLoadManager();

            //To Write
            int[] shapesArray = {       1, 1, 3, 1, 3, 2,
                                        4, 4, 1, 2, 4, 4,
                                        1, 4, 2, 1, 4, 1,
                                        2, 4, 1, 1, 4, 1,
                                        4, 4, 3, 2, 4, 4,
                                        2, 1, 1, 5, 3, 2,};

                  Goal[] goalsArray = new Goal[3];

                  Goal denemeGoal = new Goal(ShapeType.Cube, ShapeColor.Green, 5); 
                  Goal denemeGoal2 = new Goal(ShapeType.Cube, ShapeColor.Blue, 5);
                  Goal denemeGoal3 = new Goal(ShapeType.Box, ShapeColor.None, 10);

                  goalsArray[0] = denemeGoal;
                  goalsArray[1] = denemeGoal2;
                  goalsArray[2] = denemeGoal3;

                  Level level = new Level
                  {
                      level = 4,
                      row = 6,
                      col = 6,
                      moves = 15,
                      shapesArray = shapesArray,
                      goalsArray = goalsArray
                  };

            slm.WriteLevelData(level);
            Debug.Log("Level " + level.level + " saved!");
        }
    }

    public void LevelPassed()
    {
        StartCoroutine(_LevelPassed());
    }

    private IEnumerator _LevelPassed()
    {
        Debug.Log("level passed");
        LevelManager.isCurrentLevelPassed = true;
        Debug.Log(LevelManager.isCurrentLevelPassed);
        yield return new WaitForSeconds(1f);
        tint.SetActive(true);
        Instantiate(levelPassed, canvas.transform);
        if (LevelManager.levelNumber < LevelManager.totalLevels)
        {
            LevelManager.levelNumber++;
        }
    }

    private void CompleteGoal(GameObject goal)
    {
        goal.transform.GetChild(2).gameObject.SetActive(true);
        goal.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenLevel(int levelNum)
    {
        LevelManager.isCurrentLevelPassed = false;
        GameObject successPopup = GameObject.FindGameObjectWithTag("successPopup");
        GameObject newTint = GameObject.FindGameObjectWithTag("tint");
        if (successPopup != null)
        {
            Destroy(successPopup);
        }
        if (newTint != null)
        {
            newTint.SetActive(false);
        }

        myLevel = slm.LoadLevelData(levelNum.ToString());
        BoardManager.Instance.CreateBoard(myLevel.shapesArray);

        BoardManager.Instance.moves.text = myLevel.moves.ToString();

        _goals = myLevel.goalsArray.ToList();
        remainingGoals = _goals.Count;

        Debug.Log(remainingGoals);

        for (int i = 0; i < _goals.Count; i++)
        {
            goalsUI[i].transform.GetChild(1).gameObject.SetActive(true);
            goalsUI[i].transform.GetChild(2).gameObject.SetActive(false);

            goalsUI[i].transform.GetChild(1).GetComponent<Text>().text = _goals[i].count.ToString();

            if (_goals[i].shapeType == ShapeType.Cube)
            {
                if (_goals[i].shapeColor == ShapeColor.Red)
                {
                    goalsUI[i].transform.GetChild(1).GetComponent<Text>().text = _goals[i].count.ToString();
                    goalsUI[i].transform.GetChild(0).GetComponent<Image>().sprite = shapeSprites[2];
                }
                else if (_goals[i].shapeColor == ShapeColor.Blue)
                {
                    goalsUI[i].transform.GetChild(1).GetComponent<Text>().text = _goals[i].count.ToString();
                    goalsUI[i].transform.GetChild(0).GetComponent<Image>().sprite = shapeSprites[0];
                }
                else if (_goals[i].shapeColor == ShapeColor.Green)
                {
                    goalsUI[i].transform.GetChild(1).GetComponent<Text>().text = _goals[i].count.ToString();
                    goalsUI[i].transform.GetChild(0).GetComponent<Image>().sprite = shapeSprites[1];
                }
            }
            if (_goals[i].shapeType == ShapeType.Box)
            {
                goalsUI[i].transform.GetChild(1).GetComponent<Text>().text = _goals[i].count.ToString();
                goalsUI[i].transform.GetChild(0).GetComponent<Image>().sprite = shapeSprites[3];
            }

        }
    }



}
