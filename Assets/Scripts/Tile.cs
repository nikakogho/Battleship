using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Image shipImage;
    [HideInInspector]public Button button;
    public ShipPart part;
    public Board board;
    public int x, y;

    GameMaster master;

    [HideInInspector]public bool opened = false;

    public bool Damaged { get { return part != null && part.destroyed; } }
    public bool DamagedOnly { get { return shipImage.sprite == master.shipDamaged; } }
    public bool Destroyed { get { return part != null && part.ship.Destroyed; } }

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void Start()
    {
        master = GameMaster.instance;

        button.interactable = board.belongsToPlayer;
    }

    public bool Open()
    {
        if (opened) return false;

        button.interactable = false;
        opened = true;

        shipImage.enabled = true;

        if (part == null)
        {
            shipImage.sprite = master.missed;
            return false;
        }

        part.destroyed = true;

        shipImage.sprite = master.shipDamaged;

        switch (part.ship.direction)
        {
            case Direction.Right:
            case Direction.Left:
                if ((x > 0 && board.tiles[x - 1, y].Damaged) || (x < 9 && board.tiles[x + 1, y].Damaged))
                {
                    if (y > 0) board.tiles[x, y - 1].Open();
                    if (y < 9) board.tiles[x, y + 1].Open();
                }

                break;

            case Direction.Down:
            case Direction.Up:
                if ((y > 0 && board.tiles[x, y - 1].Damaged) || (y < 9 && board.tiles[x, y + 1].Damaged))
                {
                    if (x > 0) board.tiles[x - 1, y].Open();
                    if (x < 9) board.tiles[x + 1, y].Open();
                }

                break;
        }

        if (x > 0)
        {
            if (y > 0) board.tiles[x - 1, y - 1].Open();
            if (y < 9) board.tiles[x - 1, y + 1].Open();
        }

        if (x < 9)
        {
            if (y > 0) board.tiles[x + 1, y - 1].Open();
            if (y < 9) board.tiles[x + 1, y + 1].Open();
        }

        if (part.ship.Destroyed)
        {
            foreach(var shipPart in part.ship.parts)
            {
                shipPart.tile.shipImage.sprite = master.shipDestroyed;
            }

            ShipPart[] edgeParts = part.ship.Size == 1 ? new ShipPart[] { part } : new ShipPart[] { part.ship.parts[0], part.ship.parts[part.ship.Size - 1] }; 

            foreach(var shipPart in edgeParts)
            {
                for(int X = shipPart.tile.x - 1; X < shipPart.tile.x  +2; X++)
                {
                    for (int Y = shipPart.tile.y - 1; Y < shipPart.tile.y + 2; Y++)
                    {
                        if(X >= 0 && Y >= 0 && X < 10 && Y < 10)
                        {
                            board.tiles[X, Y].Open();
                        }
                    }
                }
            }

            bool livingShipLeft = false;

            foreach(Ship ship in board.ships)
            {
                if (!ship.Destroyed)
                {
                    livingShipLeft = true;
                    break;
                }
            }

            if (!livingShipLeft)
            {
                master.NextStage();
            }
        }

        return true;
    }

    public void AssignPart(ShipPart part, Sprite picture, Direction direction, bool visibly)
    {
        this.part = part;
        part.tile = this;
        shipImage.enabled = visibly;
        shipImage.sprite = picture;

        shipImage.transform.rotation = Quaternion.Euler(0, 0, ((int)direction + 2) * 90);
    }

    public void GetAttacked()
    {
        if(!Open())
        master.ChangeTurn();
    }

    public void Click()
    {
        if (board.belongsToPlayer)
        {
            board.SelectTile(this);
        }
        else
        {
            GetAttacked();
        }
    }
}
