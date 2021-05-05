using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour {
    public Sprite shipSmall, shipDamaged, shipDestroyed, missed;
    public Sprite[] ship4 = new Sprite[4];
    public Sprite[] ship3 = new Sprite[3];
    public Sprite[] ship2 = new Sprite[2];

    public static Board playerBoard;
    public static Board enemyBoard;
    [HideInInspector]public Board turnBoard;

    [HideInInspector]public Tile selectedTile = null;
    [HideInInspector]public int currentSize = 4;
    int currentTotal = 1;
    int amount = 1;

    public GameObject winUI, loseUI;

    [HideInInspector]public Direction chosenDirection = Direction.Right;
    public GameObject okButton;
    public Transform directionIcon;

    public GameObject buildUI;

    public Transform boards;

    public Text shipSizeText;

    public Sprite[] ShipSprites(int size)
    {
        switch (size)
        {
            case 1: return new Sprite[] { shipSmall };
            case 2: return ship2;
            case 3: return ship3;
            case 4: return ship4;
            default:
                Debug.LogError("Size must be between 1 and 4 !!!");
                return null;
        }
    }
    
    public enum GameState { Setup, GamePlay, GameOver }
    public GameState gameState = GameState.Setup;

    #region Singleton
    public static GameMaster instance;

    void Awake()
    {
        instance = this;
    }
    #endregion

    void Win()
    {
        winUI.SetActive(true);
    }

    void ShowHiddenShips()
    {
        foreach (Ship ship in enemyBoard.ships)
        {
            if (ship.Destroyed) continue;

            foreach (var part in ship.parts)
            {
                if (!part.destroyed)
                {
                    part.tile.shipImage.enabled = true;
                }
            }
        }
    }

    void Lose()
    {
        loseUI.SetActive(true);

        Invoke("ShowHiddenShips", 3);
    }

    public void NextStage()
    {
        gameState++;
        Board winner = turnBoard;
        turnBoard = playerBoard;

        foreach (Tile tile in playerBoard.tiles) tile.button.interactable = false;
        foreach (Tile tile in enemyBoard.tiles) tile.button.interactable = gameState == GameState.GamePlay;

        if(gameState == GameState.GameOver)
        {
            if (winner == playerBoard) Win();
            else Lose();
        }
    }

    public void ChangeDirection()
    {
        if (chosenDirection == Direction.Down) chosenDirection = Direction.Right;
        else chosenDirection++;

        directionIcon.rotation = Quaternion.Euler(0, 0, (int)chosenDirection * 90);

        if(selectedTile != null)
        if(!playerBoard.CanBuildHere(selectedTile.x, selectedTile.y, chosenDirection, currentSize))
        {
            Deselect();
        }
    }

    public void ChangeTurn()
    {
        bool activate = turnBoard == enemyBoard;

        foreach (Tile tile in enemyBoard.tiles)
        {
            tile.button.interactable = !tile.opened && activate;
        }

        if (!activate)
        {
            turnBoard = enemyBoard;
            enemyBoard.MakeMove();
        }
        else
        {
            turnBoard = playerBoard;
        }
    }

    public void OK()
    {
        new Ship(currentSize, selectedTile.x, selectedTile.y, chosenDirection, playerBoard, true);
        amount--;

        if(amount == 0)
        {
            if(currentSize > 1)
            {
                currentSize--;
                shipSizeText.text = "Ship Size: " + currentSize;
                currentTotal++;
                amount = currentTotal;
            }
            else
            {
                buildUI.SetActive(false);
                boards.localScale = Vector2.one * 1.6f;
                boards.position = boards.parent.position;

                NextStage();
            }
        }

        Deselect();
    }

    public void Select(Tile tile)
    {
        selectedTile = tile;

        okButton.transform.position = tile.transform.position + Vector3.right * 50;
        okButton.SetActive(true);
    }

    public void Deselect()
    {
        selectedTile = null;
        okButton.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
