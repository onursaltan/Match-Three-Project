using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public Text[] goalTexts;

    public GameObject[] goalsUI;

    public GameObject canvas;

    public Sprite[] shapeSprites;

    private List<Goal> _goals;

    private int remainingGoals;

    public GameObject levelPassed;
    public GameObject tint;

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
            _instance = this;
        }
    }

    private void Start()
    {
        SaveLoadManager slm = new SaveLoadManager();

        Level myLevel = slm.LoadLevelData("1");
        BoardManager.Instance.CreateBoard(myLevel.shapesArray);

        BoardManager.Instance.moves.text = myLevel.moves.ToString();

        _goals = myLevel.goalsArray.ToList();
        remainingGoals = _goals.Count;

        for (int i = 0; i < _goals.Count; i++)
        {
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
            int[] shapesArray = {       1, 4, 3, 1, 4, 2,
                                        2, 4, 1, 2, 4, 2,
                                        1, 4, 1, 1, 4, 1,
                                        2, 4, 1, 1, 4, 1,
                                        1, 4, 3, 2, 4, 2,
                                        2, 4, 1, 5, 4, 2,};

                  Goal[] goalsArray = new Goal[3];

                  Goal denemeGoal = new Goal(ShapeType.Cube, ShapeColor.Red, 10); 
                  Goal denemeGoal2 = new Goal(ShapeType.Cube, ShapeColor.Blue, 20);
                  Goal denemeGoal3 = new Goal(ShapeType.Box, ShapeColor.None, 5);

                  goalsArray[0] = denemeGoal;
                  goalsArray[1] = denemeGoal2;
                  goalsArray[2] = denemeGoal3;

                  Level level = new Level
                  {
                      level = 2,
                      row = 6,
                      col = 6,
                      moves = 15,
                      shapesArray = shapesArray,
                      goalsArray = goalsArray
                  };

            slm.WriteLevelData(level);
        }
    }

    private void LevelPassed()
    {
        StartCoroutine(_LevelPassed());
    }

    private IEnumerator _LevelPassed()
    {
        yield return new WaitForSeconds(1f);
        tint.SetActive(true);
        Instantiate(levelPassed, canvas.transform);
    }

    private void CompleteGoal(GameObject goal)
    {
        goal.transform.GetChild(2).gameObject.SetActive(true);
        goal.transform.GetChild(1).gameObject.SetActive(false);
    }

}
