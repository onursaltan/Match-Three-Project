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

    private Vector2 offset;

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
        shapeSpriteRenderer = shapePrefab.GetComponent<SpriteRenderer>();
        SetShapeRect(columns);
        offset = shapeSpriteRenderer.bounds.size;
        SetBoardPosition();
     //   SetBoardPadding();
        CreateTiles(offset.x, offset.y);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            PrintAdjacentShapes();
    }
    public void CreateTiles(float xOffset, float yOffset)
    {
        instantiatedShapes = new GameObject[rows, columns];

        float startX = transform.position.x;
        float startY = transform.position.y;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 instantiatedTransform = new Vector3(startY + (yOffset * j),
                                                            startX + (i * xOffset),
                                                            0f);

                instantiatedShapes[i, j] = CreateShape(instantiatedTransform, i, j);
            }
        }
    }

    private void SetShapeRect(int rows)
    {
        float height = Camera.main.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;

        float shapeRect = shapeSpriteRenderer.sprite.textureRect.width / shapeSpriteRenderer.sprite.pixelsPerUnit * rows;
        shapeRect += paddingHorizontal * 2;
        shapeSpriteRenderer.transform.localScale = new Vector3(width / shapeRect, width / shapeRect);
    }

    private void SetBoardPosition()
    {
        float _offSetX = rows / 2;

        if (rows % 2 == 0)
            _offSetX -= 0.5f;

        _offSetX = _offSetX * offset.x * -1;
        transform.position = new Vector3(_offSetX, _offSetX);
    }

    private void SetBoardPadding()
    {
        Vector3 _transform = transform.position;
      //  _transform.x += paddingHorizontal / (30 / 2);
        _transform.y += paddingBottom / 10;

        transform.position = _transform;
    }

    private GameObject CreateShape(Vector3 instantiateTransform, int x, int y)
    {
        ShapeData randomShape = RandomShape();
        shapeSpriteRenderer.sprite = randomShape.Sprite;

        GameObject instantiatedShape = Instantiate(shapePrefab, instantiateTransform, shapePrefab.transform.rotation);
        Shape _shape = instantiatedShape.AddComponent<Cube>();
        instantiatedShape.transform.SetParent(transform);
        _shape.SetShapeData(randomShape, x, y);

        return instantiatedShape;
    }

    private ShapeData RandomShape()
    {
        int randInt = Random.Range(0, shapesData.Length);
        return shapesData[randInt];
    }

    public GameObject[,] GetShapeMatrix()
    {
        return instantiatedShapes;
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

    public List<Shape> GetAdjacentShapes()
    {
        return adjacentShapes;
    }

    public void PrintAdjacentShapes()
    {
        if (this.adjacentShapes.Count > 1)
        {
            foreach (Shape shape in this.adjacentShapes)
            {
                shape.gameObject.SetActive(false);
            }
        }

        this.adjacentShapes.Clear();
    }

    public int GetRowCount()
    {
        return rows;
    }

    public int GetColumnCount()
    {
        return columns;
    }

    public void DestroyInstantiatedShapes()
    {
        foreach (GameObject shape in instantiatedShapes)
            Destroy(shape);
    }
}
