using UnityEngine;

public enum Direction { Right, Up, Left, Down }

public class Ship
{
    public ShipPart[] parts;
    public int x, y;

    public ShipPart this[int x, int y]
    {
        get
        {
            switch (direction)
            {
                case Direction.Right:
                    if (this.y != y) return null;
                    if (this.x < x || x - this.x > Size) return null;

                    return parts[x - this.x];

                case Direction.Down:
                    if (this.x != x) return null;
                    if (this.y < y || y - this.y > Size) return null;

                    return parts[y - this.y];

                case Direction.Left:
                    if (this.y != y) return null;
                    if (this.x > x || this.x - x > Size) return null;

                    return parts[this.x - x];

                case Direction.Up:
                    if (this.x != x) return null;
                    if (this.y > y || this.y - y > Size) return null;

                    return parts[this.y - y];

                default:
                    Debug.LogError("There is no such direction as " + direction);
                    return null;
            }
        }
    }

    public int Size { get { return parts.Length; } }
    public bool Destroyed
    {
        get
        {
            foreach (ShipPart part in parts) if (!part.destroyed) return false;

            return true;
        }
    }
    public Direction direction;
    public Board board;

    public Ship(int size, int x, int y, Direction direction, Board board, bool visibly)
    {
        if(size > 4 || size < 1)
        {
            Debug.LogError("Size must be 1, 2, 3 or 4");
            return;
        }

        this.x = x;
        this.y = y;

        this.direction = direction;
        this.board = board;

        board.ships.Add(this);

        GameMaster master = GameMaster.instance;

        parts = new ShipPart[size];
        Sprite[] sprites = master.ShipSprites(size);

        for(int i = 0; i < size; i++)
        {
            parts[i] = new ShipPart(this);
        }

        switch (size)
        {
            case 1:
                board.tiles[x, y].AssignPart(parts[0], master.shipSmall, direction, visibly);
                break;

            case 2:
                board.tiles[x, y].AssignPart(parts[0], sprites[0], direction, visibly);

                int posX = x;
                int posY = y;

                if (direction == Direction.Right) posX++;
                else if (direction == Direction.Down) posY++;
                else if (direction == Direction.Left) posX--;
                else if (direction == Direction.Up)   posY--;

                board.tiles[posX, posY].AssignPart(parts[1], sprites[1], direction, visibly);

                break;

            case 3:
                board.tiles[x, y].AssignPart(parts[0], sprites[0], direction, visibly);

                int xBy = 0;
                int yBy = 0;

                switch (direction)
                {
                    case Direction.Right: xBy = 1;
                        break;
                    case Direction.Down: yBy = 1;
                        break;
                    case Direction.Left: xBy = -1;
                        break;
                    case Direction.Up: yBy = -1;
                        break;
                }

                posX = x;
                posY = y;

                posX += xBy;
                posY += yBy;

                board.tiles[posX, posY].AssignPart(parts[1], sprites[1], direction, visibly);

                posX += xBy;
                posY += yBy;

                board.tiles[posX, posY].AssignPart(parts[2], sprites[2], direction, visibly);

                break;

            case 4:
                board.tiles[x, y].AssignPart(parts[0], sprites[0], direction, visibly);

                xBy = 0;
                yBy = 0;

                switch (direction)
                {
                    case Direction.Right:
                        xBy = 1;
                        break;
                    case Direction.Down:
                        yBy = 1;
                        break;
                    case Direction.Left:
                        xBy = -1;
                        break;
                    case Direction.Up:
                        yBy = -1;
                        break;
                }

                posX = x;
                posY = y;

                for(int i = 1; i < 3; i++)
                {
                    posX += xBy;
                    posY += yBy;

                    board.tiles[posX, posY].AssignPart(parts[i], sprites[i], direction, visibly);
                }

                posX += xBy;
                posY += yBy;

                board.tiles[posX, posY].AssignPart(parts[3], sprites[3], direction, visibly);

                break;
        }
    }
}
