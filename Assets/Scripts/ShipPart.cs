using UnityEngine;

public class ShipPart
{
    public Ship ship;
    public bool destroyed = false;
    public Tile tile;

    public ShipPart(Ship ship)
    {
        this.ship = ship;
    }
}
