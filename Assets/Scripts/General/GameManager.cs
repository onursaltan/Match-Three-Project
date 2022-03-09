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

    private List<Goal> _goals;

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

        _goals = myLevel.goalsArray.ToList();

       /* Goal denemeGoal = new Goal(ShapeType.Cube, ShapeColor.Red, 10);
        Goal denemeGoal2 = new Goal(ShapeType.Cube, ShapeColor.Blue, 20);

        _goals.Add(denemeGoal);
        _goals.Add(denemeGoal2);*/
    }

    public void CheckGoal(ShapeType shapeType, ShapeColor shapeColor = ShapeColor.None)
    {
        for (int i = 0; i < _goals.Count; i++)
        {
            if (_goals[i].count > 0 && _goals[i].shapeType == shapeType && _goals[i].shapeColor == shapeColor)
            {
                _goals[i].count--;
                goalTexts[i].text = _goals[i].shapeColor + " " + _goals[i].shapeType + "\n" + _goals[i].count;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveLoadManager slm = new SaveLoadManager();

            //To Write
            int[] shapesArray = {       1, 2, 4, 1, 2, 2,
                                        2, 3, 3, 3, 3, 2,
                                        1, 3, 1, 1, 3, 1,
                                        4, 3, 1, 1, 3, 1,
                                        1, 3, 3, 3, 3, 2,
                                        2, 1, 1, 5, 2, 2,};

                  Goal[] goalsArray = new Goal[2];

                  Goal denemeGoal = new Goal(ShapeType.Cube, ShapeColor.Red, 10); 
                  Goal denemeGoal2 = new Goal(ShapeType.Cube, ShapeColor.Blue, 20);

                  goalsArray[0] = denemeGoal;
                  goalsArray[1] = denemeGoal2;

                  Level level = new Level
                  {
                      level = 1,
                      row = 6,
                      col = 6,
                      shapesArray = shapesArray,
                      goalsArray = goalsArray
                  };

            slm.WriteLevelData(level);
        }
    }
}
