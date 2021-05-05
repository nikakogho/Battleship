using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {
    public Tile[,] tiles;
    public List<Ship> ships = new List<Ship>();

    public bool belongsToPlayer;
    public bool enemyShipsVisible;

    GameMaster master;

    void Awake()
    {
        if (belongsToPlayer) GameMaster.playerBoard = this;
        else GameMaster.enemyBoard = this;

        tiles = new Tile[10, 10];

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                Tile tile = transform.GetChild(y * 10 + x).GetComponent<Tile>();
                tile.board = this;
                tile.x = x;
                tile.y = y;

                tiles[x, y] = tile;
            }
        }
    }

    void Start()
    {
        master = GameMaster.instance;

        if (!belongsToPlayer)
        {
            GenerateShips();
        }
    }

    struct Pair
    {
        public int x, y;

        public Pair(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator==(Pair a, Pair b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator!=(Pair a, Pair b)
        {
            return !(a == b);
        }
    }

    void AssignCanUse(List<Pair> canUse)
    {
        for(int i = canUse.Count - 1; i >= 0; i--)
        {
            if (tiles[canUse[i].x, canUse[i].y].part != null) canUse.RemoveAt(i);
            else
            {
                bool done = false;

                for(int x = canUse[i].x - 1; x <= canUse[i].x + 1; x += 2)
                {
                    for (int y = canUse[i].y - 1; y <= canUse[i].y + 1; y += 2)
                    {
                        if(x > 0 && y > 0 && x < 10 && y < 10)
                        {
                            if(tiles[x, y].part != null)
                            {
                                canUse.RemoveAt(i);
                                done = true;
                                break;
                            }
                        }
                    }

                    if (done) break;
                }
            }
        }
    }

    void GenerateShips()
    {
        List<Pair> canUse = new List<Pair>();

        for(int y = 0; y < 10; y++)
        {
            for(int x = 0; x < 10; x++)
            {
                canUse.Add(new Pair(x, y));
            }
        }
        
        for (int i = 0; i < 1; i++)
        {
            MakeShip(4, canUse);
        }

        for (int i = 0; i < 2; i++)
        {
            MakeShip(3, canUse);
        }

        for (int i = 0; i < 3; i++)
        {
            MakeShip(2, canUse);
        }

        for (int i = 0; i < 4; i++)
        {
            MakeShip(1, canUse);
        }
    }

    bool EnemyIsDone { get { return master.turnBoard == GameMaster.playerBoard; } }

    void EnemyMove(Tile[,] playerTiles)
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                #region Got Damaged Guy
                if (playerTiles[x, y].DamagedOnly)
                {
                    if ((x < 9 && playerTiles[x + 1, y].DamagedOnly) || (x > 0 && playerTiles[x - 1, y].DamagedOnly))
                    {
                        for (int delta = 1; delta < 4; delta++)
                        {
                            int x1 = x - delta;
                            int x2 = x + delta;

                            if (x1 >= 0 && playerTiles[x1 + 1, y].Damaged && !playerTiles[x1, y].opened)
                            {
                                playerTiles[x1, y].GetAttacked();
                                return;
                            }

                            if(x2 < 10 && playerTiles[x2 - 1, y].Damaged && !playerTiles[x2, y].opened)
                            {
                                playerTiles[x2, y].GetAttacked();
                                return;
                            }
                        }
                    }
                    else if ((y < 9 && playerTiles[x, y + 1].DamagedOnly) || (y > 0 && playerTiles[x, y - 1].DamagedOnly))
                    {
                        for (int delta = 1; delta < 4; delta++)
                        {
                            int y1 = y - delta;
                            int y2 = y + delta;

                            if (y1 >= 0 && playerTiles[x, y1 + 1].Damaged && !playerTiles[x, y1].opened)
                            {
                                playerTiles[x, y1].GetAttacked();
                                return;
                            }

                            if (y2 < 10 && playerTiles[x, y2 - 1].Damaged && !playerTiles[x, y2].opened)
                            {
                                playerTiles[x, y2].GetAttacked();
                                return;
                            }
                        }
                    }
                    else
                    {
                        List<Pair> pickFrom = new List<Pair>();

                        for(int X = -1; X < 2; X++)
                        {
                            for(int Y = -1; Y < 2; Y++)
                            {
                                if ((X == 0) == (Y == 0)) continue;
                                if (X + x > 9 || X + x < 0) continue;
                                if (Y + y > 9 || Y + y < 0) continue;

                                Tile[,] enemyTiles = belongsToPlayer ? GameMaster.enemyBoard.tiles : GameMaster.playerBoard.tiles;

                                if (enemyTiles[X + x, Y + y].opened) continue;

                                pickFrom.Add(new Pair(X, Y));
                            }
                        }

                        if (pickFrom.Count > 0)
                        {
                            Pair pos = pickFrom[Random.Range(0, pickFrom.Count)];

                            playerTiles[x + pos.x, y + pos.y].GetAttacked();
                            return;
                        }
                    }
                }
                #endregion
            }
        }

        List<Tile> yetToOpenTiles = new List<Tile>();

        foreach (Tile tile in playerTiles)
        {
            if (!tile.opened) yetToOpenTiles.Add(tile);
        }

        yetToOpenTiles[Random.Range(0, yetToOpenTiles.Count)].GetAttacked();
    }

    IEnumerator MoveLoop()
    {
        Tile[,] playerTiles = GameMaster.playerBoard.tiles;

        while (!EnemyIsDone)
        {
            yield return new WaitForSeconds(0.6f);
            EnemyMove(playerTiles);
        }
    }

    public void MakeMove()
    {
        StartCoroutine(MoveLoop());
    }

    void MakeShip(int size, List<Pair> canUse)
    {
        Direction shipDirection = (Direction)Random.Range(0, 2);
        Pair shipPos;

        List<Pair> canReallyUse = new List<Pair>();

        for(int i = 0; i < canUse.Count; i++)
        {
            int x = canUse[i].x;
            int y = canUse[i].y;

            bool canUseThisOne = CanBuildHere(x, y, shipDirection, size);

            if (canUseThisOne)
            {
                canReallyUse.Add(canUse[i]);
            }
        }

        shipPos = canReallyUse[Random.Range(0, canReallyUse.Count)];

        new Ship(size, shipPos.x, shipPos.y, shipDirection, this, enemyShipsVisible);
        AssignCanUse(canUse);
    }

    public void SelectTile(Tile tile)
    {
        if(master.selectedTile == tile)
        {
            master.Deselect();
        }
        else if(CanBuildHere(tile.x, tile.y, master.chosenDirection, master.currentSize))
        {
            master.Select(tile);
        }
        else
        {
            master.Deselect();
        }
    }

    public bool CanBuildHere(int x, int y, Direction direction, int size)
    {
        if (direction == Direction.Right)
        {
            if (x + size > 10)
            {
                return false;
            };

            for (int X = x; X < x + size; X++)
            {
                if (tiles[X, y].part != null)
                {
                    return false;
                }

                for (int neighborX = X - 1; neighborX <= X + 1; neighborX++)
                {
                    for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
                    {
                        if (neighborY >= 0 && neighborX >= 0 && neighborX < 10 && neighborY < 10)
                        {
                            if (tiles[neighborX, neighborY].part != null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        else if (direction == Direction.Down)
        {
            if (y + size > 10)
            {
                return false;
            }

            for (int Y = y; Y < y + size; Y++)
            {
                if (tiles[x, Y].part != null)
                {
                    return false;
                }

                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    for (int neighborY = Y - 1; neighborY <= Y + 1; neighborY++)
                    {
                        if (neighborY >= 0 && neighborX >= 0 && neighborX < 10 && neighborY < 10)
                        {
                            if (tiles[neighborX, neighborY].part != null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        else if (direction == Direction.Left)
        {
            if (x - size < -1)
            {
                return false;
            };

            for (int X = x - size + 1; X < x + 1; X++)
            {
                if (tiles[X, y].part != null)
                {
                    return false;
                }

                for (int neighborX = X - 1; neighborX <= X + 1; neighborX++)
                {
                    for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
                    {
                        if (neighborY >= 0 && neighborX >= 0 && neighborX < 10 && neighborY < 10)
                        {
                            if (tiles[neighborX, neighborY].part != null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        else if (direction == Direction.Up)
        {
            if (y - size < -1)
            {
                return false;
            }

            for (int Y = y - size + 1; Y < y + 1; Y++)
            {
                if (tiles[x, Y].part != null)
                {
                    return false;
                }

                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    for (int neighborY = Y - 1; neighborY <= Y + 1; neighborY++)
                    {
                        if (neighborY >= 0 && neighborX >= 0 && neighborX < 10 && neighborY < 10)
                        {
                            if (tiles[neighborX, neighborY].part != null)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }
}
