using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    private static BoardManager _instance;

    [SerializeField] private ShapeData[] shapesData;
    [SerializeField] private GameObject shapePrefab;

    [SerializeField] private int rows;
    [SerializeField] private int columns;

    [SerializeField] private float paddingHorizontal;
    [SerializeField] private float paddingBottom;

    private SpriteRenderer shapeSpriteRenderer;

    private GameObject[,] instantiatedShapes;
    private List<Shape> adjacentShapes;
    private List<int> distinctColumns;

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
        adjacentShapes = new List<Shape>();
        distinctColumns = new List<int>();
        shapeSpriteRenderer = shapePrefab.GetComponent<SpriteRenderer>();
        SetShapeRect();
        CreateTiles();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }

    public void CreateTiles()
    {
        instantiatedShapes = new GameObject[rows, columns];

        Vector2 offset = shapeSpriteRenderer.bounds.size;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 instantiatedTransform = new Vector3(j * offset.x,
                                                            offset.y * i,
                                                            0f);

                instantiatedShapes[i, j] = CreateShape(instantiatedTransform, i, j);
            }
        }

        SetBoardPosition(offset.x);
    }

    private void SetShapeRect()
    {
        float height = Camera.main.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;

        float shapeRect = shapeSpriteRenderer.sprite.textureRect.width / shapeSpriteRenderer.sprite.pixelsPerUnit * columns;
        shapeRect += paddingHorizontal * 2;
        shapeSpriteRenderer.transform.localScale = new Vector3(width / shapeRect, width / shapeRect);
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
        shapeSpriteRenderer.sprite = randomShape.Sprite;

        GameObject instantiatedShape = Instantiate(shapePrefab, instantiateTransform, shapePrefab.transform.rotation);
        Shape _shape = instantiatedShape.AddComponent<Cube>();
        instantiatedShape.transform.SetParent(transform);
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
        if (adjacentShapes.Count > 1)
        {
            foreach (Shape shape in adjacentShapes)
            {
                instantiatedShapes[shape.row, shape.col] = null;
                Destroy(shape.gameObject);
            }
        }

        StartShiftDown();
    }

    private void StartShiftDown()
    {
        FindDistinctColums();

        foreach (int column in distinctColumns)
        {
            for (int i = 0; i < rows; i++)
            {
                if (instantiatedShapes[i, column] != null)
                    instantiatedShapes[i, column].GetComponent<Shape>().ShiftDown();
            }
        }

        adjacentShapes.Clear();
        distinctColumns.Clear();
    }

    public void AddShapeToAdjacentShapes(Shape shape)
    {
        adjacentShapes.Add(shape);
    }

    public bool IsShapeCheckedBefore(Shape _shape)
    {
        foreach (Shape shape in this.adjacentShapes)
            if (shape == _shape)
                return true;

        return false;
    }

    private List<int> FindDistinctColums()
    {
        foreach (Shape shape in adjacentShapes)
        {
            if (!distinctColumns.Contains(shape.col))
            {
                distinctColumns.Add(shape.col);
            }
        }

        return distinctColumns;
    }

    public int GetRowCount()
    {
        return rows;
    }

    public int GetColumnCount()
    {
        return columns;
    }

    public ref GameObject[,] GetShapeMatrix()
    {
        return ref instantiatedShapes;
    }

    public void DestroyInstantiatedShapes()
    {
        foreach (GameObject shape in instantiatedShapes)
            Destroy(shape);
    }
}
