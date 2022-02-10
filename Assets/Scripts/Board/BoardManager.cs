using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    private static BoardManager _instance;
    private int RefillStartPos = 3;

    [SerializeField] private ShapeData[] shapesData;
    [SerializeField] private GameObject shapePrefab;
    [SerializeField] private Text moves;

    [SerializeField] private int rows;
    [SerializeField] private int columns;

    [SerializeField] private int remainingMoves;

    [SerializeField] private float paddingHorizontal;
    [SerializeField] private float paddingBottom;

    
    private SpriteRenderer _shapeSpriteRenderer;
    private GameObject[,] _instantiatedShapes;
    private List<Shape> _adjacentShapes;
    private Dictionary<int, int> _distinctColumns;

    public static BoardManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("Not implemented.");
            }

            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _adjacentShapes = new List<Shape>();
        _distinctColumns = new Dictionary<int, int>();
        _shapeSpriteRenderer = shapePrefab.GetComponent<SpriteRenderer>();

        SetShapeRect();
        CreateTiles();
        int.TryParse(moves.text, out remainingMoves);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }

    public void restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void CreateTiles()
    {
        _instantiatedShapes = new GameObject[rows, columns];

        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 instantiatedTransform = new Vector3(j * offset.x,
                                                            offset.y * i,
                                                            0f);

                _instantiatedShapes[i, j] = CreateShape(instantiatedTransform, i, j);
            }
        }

        SetBoardPosition(offset.x);
    }

    private void SetShapeRect()
    {
        float height = Camera.main.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;

        float shapeRect = _shapeSpriteRenderer.sprite.textureRect.width / _shapeSpriteRenderer.sprite.pixelsPerUnit * columns;
        shapeRect += paddingHorizontal * 2;
        _shapeSpriteRenderer.transform.localScale = new Vector3(width / shapeRect, width / shapeRect);
    }

    private void SetBoardPosition(float offSetX)
    {
        float _offSetX = columns / 2;

        if (columns % 2 == 0)
            _offSetX -= 0.5f;

        _offSetX = _offSetX * offSetX * -1;
        transform.position = new Vector3(_offSetX, -4.77f);

        SetBoardPadding();
    }

    private void SetBoardPadding()
    {
        Vector3 _transform = transform.position;
        _transform.y += paddingBottom / 10;

        transform.position = _transform;
    }

    private GameObject CreateShape(Vector3 instantiateTransform, int i, int j)
    {
        ShapeData randomShape = RandomShape();
        _shapeSpriteRenderer.sprite = randomShape.Sprite;

        GameObject instantiatedShape = Instantiate(shapePrefab, instantiateTransform, shapePrefab.transform.rotation, transform);
        Shape _shape = instantiatedShape.AddComponent<Cube>();
        _shape.SetShapeData(randomShape, i, j);

        return instantiatedShape;
    }

    private ShapeData RandomShape()
    {
        int randInt = Random.Range(0, shapesData.Length);
        return shapesData[randInt];
    }

    public void HandleShiftDown()
    {
        if (_adjacentShapes.Count > 1 && remainingMoves > 0)
        {
            remainingMoves--;
            moves.text = remainingMoves.ToString();
            foreach (Shape shape in _adjacentShapes)
            {
                _instantiatedShapes[shape.row, shape.col] = null;
                shape.Explode();
            }
        }

        StartShiftDown();
        RefillBoard();
    }

    private void StartShiftDown()
    {
        FindDistinctColums();

        foreach (int column in _distinctColumns.Keys)
        {
            for (int i = 0; i < rows; i++)
            {
                if (_instantiatedShapes[i, column] != null)
                    _instantiatedShapes[i, column].GetComponent<Shape>().ShiftDown();
            }
        }

        _adjacentShapes.Clear();
    }

    public void AddShapeToAdjacentShapes(Shape shape)
    {
        _adjacentShapes.Add(shape);
    }

    public bool IsShapeCheckedBefore(Shape _shape)
    {
        foreach (Shape shape in this._adjacentShapes)
            if (shape == _shape)
                return true;

        return false;
    }

    private void FindDistinctColums()
    {
        foreach (Shape shape in _adjacentShapes)
        {
            if (!_distinctColumns.ContainsKey(shape.col))
                _distinctColumns.Add(shape.col, 1);
            else
                _distinctColumns[shape.col]++;
        }
    }

    private void RefillBoard()
    {
        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        foreach (int k in _distinctColumns.Keys)
        {
            int counter = RefillStartPos;
            Vector3 instantiatedTransform = new Vector3(offset.x * k,
                                                        offset.y * rows,
                                                        0f);

            for (int i = 0; i < _distinctColumns[k]; i++)
            {
                GameObject refillShape =  CreateShape(instantiatedTransform, rows + counter, k);
                refillShape.transform.localPosition = new Vector3(offset.x * k, offset.y * (rows + counter), 0f);
                refillShape.GetComponent<Shape>().ShiftDown(true);
                counter++;
            }
        }

        _distinctColumns.Clear();
    }

    public int GetRowCount()
    {
        return rows;
    }

    public int GetColumnCount()
    {
        return columns;
    }

    public GameObject[,] GetShapeMatrix()
    {
        return _instantiatedShapes;
    }

    public void DestroyInstantiatedShapes()
    {
        foreach (GameObject shape in _instantiatedShapes)
            Destroy(shape);
    }
}
