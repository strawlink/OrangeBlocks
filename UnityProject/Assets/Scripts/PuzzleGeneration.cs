using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleGeneration : MonoBehaviour
{
    #region Inspector variables

    [SerializeField]
    private Transform Blocker;

    [SerializeField]
    private GameObject IngameContainer;

    [SerializeField]
    private GameObject MenuContainer;

    [SerializeField]
    private GameObject PiecePrefab;

    [SerializeField]
    private GameObject PuzzleContainer;

    [SerializeField]
    private Camera NormalCamera;

    [SerializeField]
    private Text TextLabel;

    #endregion Inspector variables

    private int currentHeight;
    private int currentWidth;
    private PieceBehaviour[,] gameGrid = null;

    private long pressCount = 0;

    private Vector2[] presses = null;

    private int stepCount = 0;

    public static bool AllowInput { get; private set; }

    public static bool GameIsActive { get; private set; }

    #region UI Button-events

    public void GenerateOne()
    {
        GenerateGrid(3, 3);
    }

    public void GenerateThree()
    {
        GenerateGrid(7, 7);
    }

    public void GenerateTwo()
    {
        GenerateGrid(3, 5);
    }

    #endregion UI Button-events

    private void OnPress(int x, int y)
    {
        PatternPress(x, y);

        if (GameIsActive)
        {
            UpdatePressCount(pressCount + 1);

            if (DidCompleteGame())
            {
                GameIsActive = false;
                AllowInput = false;

                Debug.Log("Game is over");
            }
        }
    }

    public void ShowMenu(bool state)
    {
        MenuContainer.SetActive(state);
        PuzzleContainer.SetActive(!state);
        IngameContainer.SetActive(!state);

        if (GameIsActive)
        {
            AllowInput = !state;
        }
        else
        {
            AllowInput = false;
        }
    }

    private void Awake()
    {
        ShowMenu(true);
    }

    private void DeleteGrid()
    {
        if (gameGrid != null)
        {
            foreach (PieceBehaviour piece in gameGrid)
            {
                GameObject.Destroy(piece.gameObject);
            }

            gameGrid = null;
        }
    }
    
    private bool DidCompleteGame()
    {
        bool? state = null; // the state that all the pieces need to be in, if null, can be either

        foreach (PieceBehaviour item in gameGrid)
        {
            if (state == null)
            {
                state = item.CurrentState;
            }
            else if (item.CurrentState != state)
            {
                return false;
            }
        }

        return true;
    }

    // Make all pieces non-kinematic and move them using device accelerometer
    // Currently disabled
    private void EasterEgg()
    {
        if (gameGrid != null)
        {
            // Will conflict with menu-button
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                foreach (var item in gameGrid)
                {
                    item.GetComponent<Rigidbody2D>().isKinematic = !item.GetComponent<Rigidbody2D>().isKinematic;
                }
            }

            foreach (var item in gameGrid)
            {
                Rigidbody2D rigid = item.GetComponent<Rigidbody2D>();

                if (!rigid.isKinematic)
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        rigid.AddForce(new Vector2(Input.acceleration.x, Input.acceleration.y) * 9.82f);
                    }
                }
            }
        }
    }

    private void GenerateGrid(int width, int height)
    {
        ShowMenu(false);
        Blocker.localScale = new Vector3(width, height, 1);
        GameIsActive = false;
        AllowInput = true;
        DeleteGrid();

        currentWidth = width;
        currentHeight = height;

        gameGrid = new PieceBehaviour[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject spawnedObject = GameObject.Instantiate(PiecePrefab, new Vector2(x, y), Quaternion.identity) as GameObject;
                spawnedObject.transform.parent = PuzzleContainer.transform;
                spawnedObject.transform.localPosition = new Vector2(x, y);

                gameGrid[x, y] = spawnedObject.GetComponent<PieceBehaviour>();
                gameGrid[x, y].Initialize(x, y);
            }
        }

        presses = new Vector2[50];
        for (int i = 0; i < 50; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            gameGrid[x, y].Press();
            presses[i] = new Vector2(x, y);
        }

        NormalCamera.transform.position = new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, -10);
        NormalCamera.orthographicSize = Mathf.Max(width, height);

        UpdatePressCount(0);

        stepCount = 0;

        GameIsActive = true;
    }

    private void OnDisable()
    {
        PieceBehaviour.OnPress -= OnPress;
    }

    private void OnEnable()
    {
        PieceBehaviour.OnPress += OnPress;
    }

    private void PatternPress(int x, int y)
    {
        gameGrid[x, y].Press(false);

        if (x > 0)
        {
            gameGrid[x - 1, y].Press(false);
        }

        if (x < currentWidth - 1)
        {
            gameGrid[x + 1, y].Press(false);
        }

        if (y > 0)
        {
            gameGrid[x, y - 1].Press(false);
        }

        if (y < currentHeight - 1)
        {
            gameGrid[x, y + 1].Press(false);
        }
    }

    private IEnumerator Test()
    {
        while (GameIsActive)
        {
            int x = Random.Range(0, currentWidth);
            int y = Random.Range(0, currentHeight);

            gameGrid[x, y].Press();

            if (pressCount % 10000 == 0)
            {
                yield return null;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            GenerateGrid(3, 3);
        }

        if (Input.GetKeyDown(KeyCode.F2) && stepCount < 10)
        {
            Vector2 pos = presses[9 - stepCount];
            gameGrid[(int)pos.x, (int)pos.y].Press();
            stepCount++;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMenu(!MenuContainer.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            StartCoroutine(Test());
        }
    }

    private void UpdatePressCount(long count)
    {
        pressCount = count;
        TextLabel.text = count.ToString();
    }
}