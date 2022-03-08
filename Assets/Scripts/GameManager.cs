using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        _goals = new List<Goal>();

        Goal denemeGoal = new Goal(ShapeType.Cube, ShapeColor.Red, 10);
        Goal denemeGoal2 = new Goal(ShapeType.Cube, ShapeColor.Blue, 20);

        _goals.Add(denemeGoal);
        _goals.Add(denemeGoal2);
    }

    public void CheckGoal(ShapeType shapeType, ShapeColor shapeColor = ShapeColor.None)
    {
        for(int i = 0; i < _goals.Count; i++)
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
            Debug.Log(_goals[0].count);
            Debug.Log(_goals[1].count);
        }
    }
}

public class Goal
{
    public ShapeType shapeType;
    public ShapeColor shapeColor;
    public int count;

    public Goal(ShapeType shapeType, ShapeColor shapeColor, int count)
    {
        this.shapeType = shapeType;
        this.shapeColor = shapeColor;
        this.count = count;
    }
}
