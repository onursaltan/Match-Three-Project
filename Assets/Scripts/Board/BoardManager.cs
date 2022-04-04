using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready, Merging, RocketExplosion, DiscoExplosion, BombExplosion, BigDiscoExplosion, BigBombExplosion, DiscoRocketExplosion, DiscoBombExplosion, RocketBombExplosion, DoubleRocket
}

public class BoardManager : MonoBehaviour
{
    private static BoardManager _instance;
    private const int RefillStartPos = 5;

    //temp
    public GameObject RocketMergeEffect;
    public GameObject BombMergeEffect;
    public GameObject DiscoMergeEffect;
    public GameObject LightBallCube;
    public GameObject BigBombEffect;
    public GameObject GreatBombEffect;
    public GameObject DiscoHighlight;
    public GameObject RocketBombMerge;
    public GameObject DiscoExplosionBlueAnim;
    public GameObject DiscoExplosionRedAnim;
    public GameObject DiscoExplosionGreenAnim;
    public GameObject DoubleDiscoMerge;
    public GameObject DoubleBombMerge;
    //
    [SerializeField] private ShapeData[] allShapeDatas;

    [SerializeField] private ShapeData[] shapeDatas;
    [SerializeField] private List<Sprite> mergeSprites;
    [SerializeField] private ShapeData[] BasicCubes;
    [SerializeField] private GameObject shapePrefab;
    [SerializeField] private GameObject tint;
    [SerializeField] private GameObject noMovesLeft;
    [SerializeField] public Text moves;

    [SerializeField] public int rows;
    [SerializeField] public int columns;

    [SerializeField] public int remainingMoves;

    [SerializeField] private float paddingHorizontal;
    [SerializeField] private float paddingBottom;

    private GameState _gameState;

    private SpriteRenderer _shapeSpriteRenderer;
    private Shape[,] _instantiatedShapes;
    private List<int> _explodedRows;
    private List<Shape> _adjacentShapes;
    private Dictionary<int, int> _distinctColumns;
    private bool _isMergesFound = false;

    public static BoardManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<BoardManager>();
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
            _explodedRows = new List<int>();
            _distinctColumns = new Dictionary<int, int>();
            _shapeSpriteRenderer = shapePrefab.GetComponent<SpriteRenderer>();

            SetShapeRect();
        }
    }

    private void Start()
    {
        //  CreateBoard();
        //int.TryParse(moves.text, out remainingMoves);
        _gameState = GameState.Ready;
        FindMerges();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void CreateBoard()
    {
        _instantiatedShapes = new Shape[rows, columns];

        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 instantiatedTransform = new Vector3(j * offset.x,
                                                            (offset.y - 0.08f) * i,
                                                            0f);

                _instantiatedShapes[i, j] = CreateShape(instantiatedTransform, i, j);
            }
        }

        SetBoardPosition(offset.x);
    }

    public void CreateBoard(int[] shapesArray)
    {
        _instantiatedShapes = new Shape[rows, columns];
        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 instantiatedTransform = new Vector3(j * offset.x,
                                                            (offset.y - 0.08f) * i,
                                                            0f);

                _instantiatedShapes[i, j] = CreateSpecificShape(instantiatedTransform, i, j, shapesArray[index++]);
            }
        }

        SetBoardPosition(offset.x);
        FindMerges();
    }

    private void SetShapeRect()
    {
        float height = Camera.main.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;

        float shapeRect = columns;

        shapeRect += (paddingHorizontal / 10) * 2;
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

    private Shape CreateShape(Vector3 instantiateTransform, int i, int j)
    {
        ShapeData randomShape = RandomShape();

        GameObject instantiatedShape = Instantiate(shapePrefab, instantiateTransform, shapePrefab.transform.rotation, transform);
        Shape _shape = instantiatedShape.AddComponent<Cube>();
        _shape.SetShapeData(randomShape, i, j);

        return _shape;
    }

    private Shape CreateSpecificShape(Vector3 instantiateTransform, int i, int j, int shapeReference)
    {
        GameObject instantiatedShape = Instantiate(shapePrefab, instantiateTransform, shapePrefab.transform.rotation, transform);
        instantiatedShape.transform.localPosition = instantiateTransform;
        Shape _shape = null;

        if (allShapeDatas[shapeReference].ShapeType == ShapeType.Cube)
            _shape = instantiatedShape.AddComponent<Cube>();
        else if (allShapeDatas[shapeReference].ShapeType == ShapeType.Box)
            _shape = instantiatedShape.AddComponent<Box>();
        else if (allShapeDatas[shapeReference].ShapeType == ShapeType.Bomb)
            _shape = instantiatedShape.AddComponent<Bomb>();
        else if (allShapeDatas[shapeReference].ShapeType == ShapeType.Rocket)
            _shape = instantiatedShape.AddComponent<Rocket>();
        else if (allShapeDatas[shapeReference].ShapeType == ShapeType.Disco)
            _shape = instantiatedShape.AddComponent<Disco>();
        else if (allShapeDatas[shapeReference].ShapeType == ShapeType.Box2)
            _shape = instantiatedShape.AddComponent<Box1>();
        else if (allShapeDatas[shapeReference].ShapeType == ShapeType.Box3)
            _shape = instantiatedShape.AddComponent<Box2>();


        _shape.SetShapeData(allShapeDatas[shapeReference], i, j);

        return _shape;
    }

    private ShapeData RandomShape()
    {
        int randInt = Random.Range(0, BasicCubes.Length);
        return BasicCubes[randInt];
    }

    public bool IsShapeCheckedBefore(List<Shape> adjacentShapes, Shape _shape)
    {
        foreach (Shape shape in adjacentShapes)
            if (shape == _shape)
                return true;

        return false;
    }

    public void SetAdjacentShapes(List<Shape> adjacentShapes)
    {
        _adjacentShapes = adjacentShapes;
    }

    public IEnumerator StartShiftDownTrigger()
    {
        yield return new WaitUntil(() => _gameState == GameState.Ready);
        StartShiftDown();
    }

    public IEnumerator PopUpTrigger()
    {
        yield return new WaitUntil(() => _gameState == GameState.Ready);
        CheckForPassed();
    }

    public void CheckForPassed()
    {
            if (!LevelManager.isCurrentLevelPassed)
            {
                Debug.Log("bitti");
                noMovesLeft.SetActive(true);
                tint.SetActive(true);
            }
    }

    public void StartShiftDown()
    {
        FindEmptyCells();

        if (_gameState == GameState.Ready)
        {
            foreach (int column in _distinctColumns.Keys)
                for (int i = 0; i < rows; i++)
                    if (_instantiatedShapes[i, column] != null)
                        _instantiatedShapes[i, column].GetComponent<Shape>().ShiftDown();

            RefillBoard(_distinctColumns);
        }
    }

    public void StartShiftDown(List<int> columns)
    {
        int counter;

        foreach (int column in columns)
        {
            counter = 0;

            for (int i = 0; i < rows; i++)
            {
                if (_instantiatedShapes[i, column] != null)
                    _instantiatedShapes[i, column].GetComponent<Shape>().ShiftDown();
                else
                    counter++;
            }
                

            RefillColumn(counter, column);
        }

        //FindEmptyCells();
        //RefillBoard(_distinctColumns);
    }

    private int FindEmptyRowsOfColumn(int col)
    {
        int count = 0;

        for (int i = 0; i < rows; i++)
            if (_instantiatedShapes[i, col] == null)
                count++;

        return count;
    }

    private void RefillColumn(int emptyCount, int column)
    {
        Vector2 offset = _shapeSpriteRenderer.bounds.size;
        int counter = 3;
        //Debug.Log("empty count: " + emptyCount + " column number: " + column);
        for (int i = 0; i < emptyCount; i++)
        {
            Shape refillShape = CreateShape(Vector3.zero, rows + counter, column);
            refillShape.transform.localPosition = new Vector3(offset.x * column, offset.y * (rows + counter), 0f);
            refillShape.GetComponent<Shape>().ShiftDown(true);
            counter++;
        }
    }

    public void DelayedShiftDown(List<int> columns, float delay)
    {
        StartCoroutine(WaitForShiftDown(columns, delay));
    }
    private IEnumerator WaitForShiftDown(List<int> columns, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartShiftDown(columns);
    }

   

    private void FindEmptyCells()
    {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < columns; j++)
                if (_instantiatedShapes[i, j] == null)
                    IncreaseDistinctColumns(_distinctColumns, j);
    }

    private void FindDistinctColums()
    {
        foreach (Shape shape in _adjacentShapes)
        {
            if (!_distinctColumns.ContainsKey(shape._col))
                _distinctColumns.Add(shape._col, 1);
            else
                _distinctColumns[shape._col]++;
        }
    }

    private void RefillBoard(Dictionary<int, int> distinctColumns)
    {
        Vector2 offset = _shapeSpriteRenderer.bounds.size;

        foreach (int k in distinctColumns.Keys)
        {
            int counter = RefillStartPos;
            Vector3 instantiatedTransform = new Vector3(offset.x * k,
                                                        offset.y * rows,
                                                        0f);

            for (int i = 0; i < distinctColumns[k]; i++)
            {
                Shape refillShape = CreateShape(instantiatedTransform, rows + counter, k);
                refillShape.transform.localPosition = new Vector3(offset.x * k, offset.y * (rows + counter), 0f);
                refillShape.GetComponent<Shape>().ShiftDown(true);
                counter++;
            }
        }

        distinctColumns.Clear();
    }

    #region Find Merge

    public void FindMerges()
    {
        if (!_isMergesFound)
            StartCoroutine(FindMergesRoutine());
    }

    private IEnumerator FindMergesRoutine()
    {
        List<Shape> instantiatedShapes = Array2DToList(_instantiatedShapes);
        List<Shape> adjacentShapes = new List<Shape>();

        for (int i = 0; i < instantiatedShapes.Count; i++)
        {
            if (instantiatedShapes.ElementAt(i) != null)
            {
                instantiatedShapes.ElementAt(i).FindAdjacentShapes(true, adjacentShapes, null);
                RemoveIntersections(instantiatedShapes, adjacentShapes);
                SetMergesSprite(adjacentShapes);
                adjacentShapes.Clear();
            }
        }

        yield return null;
    }

    private void RemoveIntersections(List<Shape> instantiatedShapes, List<Shape> adjacentShapes)
    {
        foreach (Shape shape in instantiatedShapes.ToList())
            if (adjacentShapes.Contains(shape))
                instantiatedShapes.Remove(shape);
    }

    public List<T> Array2DToList<T>(T[,] arr)
    {
        List<T> arrayList = new List<T>();

        if (arr != null)
            foreach (T shape in arr)
                arrayList.Add(shape);

        return arrayList;
    }

    private void SetMergesSprite(List<Shape> adjacentShapes)
    {
        foreach (Shape shape in adjacentShapes)
            shape.SetMergeSprite(adjacentShapes.Count);
    }

    public void ReverseShapesSprite()
    {
        foreach (Shape shape in _instantiatedShapes)
            if (shape != null)
                shape.ReverseShapeSprite();
    }

    #endregion

    public void ReloadShapeToList(Shape shape, int row, int col)
    {
        _instantiatedShapes[row, col] = shape;
    }

    public void IncreaseDistinctColumns(Dictionary<int, int> distinctColumns, int col)
    {
        if (!distinctColumns.ContainsKey(col))
            distinctColumns.Add(col, 1);
        else
            distinctColumns[col] += 1;
    }

    public void FullFillDistinctColumns(float waitFullFill)
    {
        _distinctColumns.Clear();

        for (int i = 0; i < columns; i++)
            _distinctColumns.Add(i, rows);

        StartCoroutine(StartFullFill(waitFullFill));
    }

    private IEnumerator StartFullFill(float waitFullFill)
    {
        yield return new WaitForSeconds(waitFullFill);
        StartShiftDown();
    }

    public void DecreaseRemainingMoves()
    {
        if (remainingMoves > 0)
        {
            remainingMoves--;
            moves.text = remainingMoves.ToString();
        }

        if (remainingMoves == 0)
        {
            StartCoroutine(PopUpTrigger());
        }
    }

    public void SetGameState(GameState targetGameState, GameState sourceGameState = GameState.Ready)
    {
        if (_gameState != GameState.DiscoExplosion)
            _gameState = targetGameState;
        else
        {
            if (sourceGameState == GameState.DiscoExplosion)
            {
                _gameState = targetGameState;
                //StartShiftDown();
            }
        }
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    public void SetIsMergesFound(bool b)
    {
        _isMergesFound = b;
    }

    public Dictionary<int, int> GetDistinctColumns()
    {
        return _distinctColumns;
    }

    public List<Shape> GetAdjacentShapes()
    {
        return _adjacentShapes;
    }

    public List<int> GetExplodedRows()
    {
        return _explodedRows;
    }

    public Shape[,] GetInstantiatedShapes()
    {
        return _instantiatedShapes;
    }

    public int GetRowCount()
    {
        return rows;
    }

    public int GetColumnCount()
    {
        return columns;
    }

    public Shape[,] GetShapeMatrix()
    {
        return _instantiatedShapes;
    }

    public ShapeData GetShapeData(ShapeType shapeType, ShapeColor shapeColor = ShapeColor.None)
    {
        foreach (ShapeData shapeData in shapeDatas)
            if (shapeData.ShapeType == shapeType && shapeData.ShapeColor == shapeColor)
                return shapeData;

        return null;
    }

    public Sprite GetMergeSprite(CubeOperation cubeOperation, string color)
    {
        string spriteName = color;

        if (cubeOperation == CubeOperation.TurnIntoRocket)
            spriteName += "_rocket_cube";
        else if (cubeOperation == CubeOperation.TurnIntoBomb)
            spriteName += "_bomb_cube";
        else if (cubeOperation == CubeOperation.TurnIntoDisco)
            spriteName += "_disco_cube";

        return mergeSprites.Find(sprt => sprt.name == spriteName);
    }

    public void RemoveFromInstantiatedShapes(int row, int col)
    {
        _instantiatedShapes[row, col] = null;
    }

    public void DestroyShape(Shape shape)
    {
        _instantiatedShapes[shape._row, shape._col] = null;
        Destroy(shape.gameObject);
    }

    public void DestroyInstantiatedShapes()
    {
        foreach (Shape shape in _instantiatedShapes)
            Destroy(shape);
    }

    /*IEnumerator RestartButtonWithDelay(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("kontrol edildi");
        if (!LevelManager.isCurrentLevelPassed)
        {
            noMovesLeft.SetActive(true);
            tint.SetActive(true);
        }
        else yield break;
    }*/

    public bool isMovesLeft()
    {
        if (remainingMoves > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
