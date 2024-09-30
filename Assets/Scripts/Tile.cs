using UnityEngine;
using System.Collections.Generic;
using Structs;
using Enums;

public class Tile
{
    public CubeIndex index;
    public Vector3 pos;
    public bool isOcupied = false;
    public TileObj tileObj = null;

    public List<Vector2> vertices = new List<Vector2>(6);

    public Tile(CubeIndex index, Vector3 pos)
    {
        this.index = index;
        this.pos = pos;
        for (int i = 0; i < 6; i++)
        {
            this.vertices.Add(Util.Corner(pos, 0.5f, i));
        }
    }


    //public static Tile operator +(Tile one, Tile two)
    //{
    //    Tile ret = new Tile(one.index + two.index);
    //    return ret;
    //}

    #region A* Herustic Variables
    public int MoveCost { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get { return GCost + HCost; } }
    public Tile Parent { get; set; }
    #endregion
}

