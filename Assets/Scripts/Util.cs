using Enums;
using Structs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class Util
{
    public static Vector3 Corner(Vector3 origin, float radius, int corner)
    {
        float angle = 60 * corner;
        angle *= Mathf.PI / 180;
        return new Vector3(origin.x + radius * Mathf.Cos(angle), origin.y + radius * Mathf.Sin(angle), 0.0f);
    }

    public static bool HexIsInside(Vector2 point, List<Vector2> vertices)
    {
        //crosses는 점q와 오른쪽 반직선과 다각형과의 교점의 개수
        int crosses = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            int j = (i + 1) % vertices.Count();
            //점 B가 선분 (p[i], p[j])의 y좌표 사이에 있음
            if ((vertices[i].y > point.y) != (vertices[j].y > point.y))
            {
                //atX는 점 B를 지나는 수평선과 선분 (p[i], p[j])의 교점
                double atX = (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x;
                //atX가 오른쪽 반직선과의 교점이 맞으면 교점의 개수를 증가시킨다.
                if (point.x < atX)
                    crosses++;
            }
        }
        return crosses % 2 > 0;
    }

    public static Tile NextTileDir(this Tile tile, HexDir dir)
    {
        if (tile == null)
        {
            return null;
        }

        var nextTile = SceneGame.Instance.GetNeighbourForDir(tile.index, dir);
        if (nextTile == null)
        {
            return null;
        }
        //if (nextTile.isOcupied)
        //{
        //    return null;
        //}
        return nextTile;
    }

    public static OffsetIndex CubeToEvenFlat(CubeIndex c)
    {
        OffsetIndex o;
        o.row = c.x;
        o.col = c.z + (c.x + (c.x & 1)) / 2;
        return o;
    }

    public static CubeIndex EvenFlatToCube(OffsetIndex o)
    {
        CubeIndex c;
        c.x = o.col;
        c.z = o.row - (o.col + (o.col & 1)) / 2;
        c.y = -c.x - c.z;
        c.str = $"[{c.x},{c.y},{c.z}]";
        return c;
    }

    public static OffsetIndex CubeToOddFlat(CubeIndex c)
    {
        OffsetIndex o;
        o.col = c.x;
        o.row = c.z + (c.x - (c.x & 1)) / 2;
        return o;
    }

    public static CubeIndex OddFlatToCube(OffsetIndex o)
    {
        CubeIndex c;
        c.x = o.col;
        c.z = o.row - (o.col - (o.col & 1)) / 2;
        c.y = -c.x - c.z;
        c.str = $"[{c.x},{c.y},{c.z}]";
        return c;
    }

    public static OffsetIndex CubeToEvenPointy(CubeIndex c)
    {
        OffsetIndex o;
        o.row = c.z;
        o.col = c.x + (c.z + (c.z & 1)) / 2;
        return o;
    }

    public static CubeIndex EvenPointyToCube(OffsetIndex o)
    {
        CubeIndex c;
        c.x = o.col - (o.row + (o.row & 1)) / 2;
        c.z = o.row;
        c.y = -c.x - c.z;
        c.str = $"[{c.x},{c.y},{c.z}]";
        return c;
    }

    public static OffsetIndex CubeToOddPointy(CubeIndex c)
    {
        OffsetIndex o;
        o.row = c.z;
        o.col = c.x + (c.z - (c.z & 1)) / 2;
        return o;
    }

    public static CubeIndex OddPointyToCube(OffsetIndex o)
    {
        CubeIndex c;
        c.x = o.col - (o.row - (o.row & 1)) / 2;
        c.z = o.row;
        c.y = -c.x - c.z;
        c.str = $"[{c.x},{c.y},{c.z}]";
        return c;
    }

    public static CubeIndex[] directions = new CubeIndex[]
{
        new CubeIndex(1, -1, 0), // RUp
        new CubeIndex(1, 0, -1), // RDown
        new CubeIndex(0, 1, -1), // Down
        new CubeIndex(-1, 1, 0), // LDown
        new CubeIndex(-1, 0, 1), // LUp
        new CubeIndex(0, -1, 1) // Up
};
    public static CubeIndex GetCubeIndexDir(HexDir dir)
    {
        switch (dir)
        {
            case HexDir.RUp:
                return directions[0];
            case HexDir.RDown:
                return directions[1];
            case HexDir.Down:
                return directions[2];
            case HexDir.LDown:
                return directions[3];
            case HexDir.LUp:
                return directions[4];
            case HexDir.Up:
                return directions[5];
            default:
                return directions[0];
        }

    }
    public static Sprite GetBlockSprite(BlockType type)
    {
        Sprite sprite = null;
        switch (type)
        {
            case BlockType.Blue:
                sprite = Global.Instance.blueNormalBlock;
                break;
            case BlockType.Green:
                sprite = Global.Instance.greenNormalBlock;
                break;
            case BlockType.Orange:
                sprite = Global.Instance.orangeNormalBlock;
                break;
            case BlockType.Purple:
                sprite = Global.Instance.purpleNormalBlock;
                break;
            case BlockType.Red:
                sprite = Global.Instance.redNormalBlock;
                break;
            case BlockType.Yellow:
                sprite = Global.Instance.yellowNormalBlock;
                break;
            default:
                break;
        }
        return sprite;
    }

}
