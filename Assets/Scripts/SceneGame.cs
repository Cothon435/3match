using Enums;
using Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SceneGame : MonoBehaviour
{
    private static SceneGame _instance;
    public static SceneGame Instance { get { return _instance; } }

    private Dictionary<string, bool> _visited = new Dictionary<string, bool>(30);
    private List<string> removeTileList = new List<string>(6);
    private HashSet<string> removeTileSet = new HashSet<string>(9);
    private List<List<string>> matchTiles = new List<List<string>>();


    private Dictionary<string, Tile> _gridGrounds = new Dictionary<string, Tile>(30);
    private List<TileObj> _tileObjs = new List<TileObj>(30);

    public int mapWidth;
    public int mapHeight;

    public Tilemap tileMap = null;
    public TileBase groundTile = null;
    public GameObject preFabTileObj = null;
    public GameObject parentObj = null;

    public CubeIndex drobStartCubeIndex = new CubeIndex(0, -4, 4);
    public Vector3 drobStartPos;

    private TileObj _clickTileObj = null;
    public float _genTime = 0.5f;
    private float _currentGenTime = 0.0f;

    public Button StartButton;

    public Tile GetTile(string key)
    {
        if (!_gridGrounds.ContainsKey(key))
        {
            return null;
        }
        return _gridGrounds[key];
    }

    private void Awake()
    {
        _instance = this;
        drobStartCubeIndex = new CubeIndex(0, -4, 4);
        var offset = Util.CubeToOddFlat(drobStartCubeIndex);
        var offsetVec3 = new Vector3Int(offset.row, offset.col);
        var tilePos = tileMap.CellToLocal(offsetVec3);
        drobStartPos = tilePos;
    }

    void Update()
    {
        if (_gridGrounds.Count == 0)
        {
            return;
        }

        SwapInput();

        if (_tileObjs.Count < 30)
        {
            _currentGenTime += Time.deltaTime;
            if(_currentGenTime >= _genTime)
            {
                GenTileObj();
                _currentGenTime = 0.0f;
            }
        }

        CheckMatcheTiming();
    }

    private void CheckMatcheTiming()
    {
        bool isChechMatch = true;
        foreach (var ground in _gridGrounds)
        {
            if (ground.Value.tileObj == null)
            {
                isChechMatch = false;
                break;
            }
            if (ground.Value.tileObj != null)
            {
                if (ground.Value.tileObj.CurrentTime > 0.0f)
                {
                    isChechMatch = false;
                    break;
                }
            }
        }
        if (isChechMatch)
        {
            CheckForMatchs();
        }
    }

    private void SwapInput()
    {
        if(_clickTileObj != null && _clickTileObj.CurrentTime > 0.0f)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var clickCellPos = tileMap.WorldToCell(clickPosition);
            var clickCellCubeIndex = Util.OddFlatToCube(new OffsetIndex(clickCellPos.x, clickCellPos.y));
            if (_gridGrounds.ContainsKey(clickCellCubeIndex.str))
            {
                _clickTileObj = _gridGrounds[clickCellCubeIndex.str].tileObj;
            }
        }

        if (Input.GetMouseButton(0))
        {

            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (_clickTileObj != null && !Util.HexIsInside(clickPosition, _clickTileObj.Tile.vertices))
            {
                var clickCellPos = tileMap.WorldToCell(clickPosition);
                var clickCellCubeIndex = Util.OddFlatToCube(new OffsetIndex(clickCellPos.x, clickCellPos.y));
                if (_gridGrounds.ContainsKey(clickCellCubeIndex.str))
                {
                    //swap
                    var preClickTileObj = _clickTileObj;
                    var nextTileObj = _gridGrounds[clickCellCubeIndex.str].tileObj;
                    if (nextTileObj.Tile.index != _clickTileObj.Tile.index)
                    {
                        _clickTileObj.Swap(nextTileObj, endCallBack: () =>
                        {
                            CheckForMatchs();
                            if(removeTileSet.Count == 0)
                            {
                                preClickTileObj.Swap(nextTileObj);
                            }
                        });
                        _clickTileObj = null;
                    }

                }
            }
        }
    }

    public void LoadMap()
    {
        GenGrounds();
    }

    public void StartGame() {
        LoadMap();
        StartButton.gameObject.SetActive(false);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        float handleSize = HandleUtility.GetHandleSize(transform.position) * 0.1f;
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = (int)(1 * handleSize);

        foreach (var tile in _gridGrounds)
        {
            Handles.color = Color.black;

            Handles.Label(tile.Value.pos, $"{tile.Key}", textStyle);
            var offset = Util.CubeToOddFlat(tile.Value.index);
            var offsetVec3 = new Vector2Int(offset.row, offset.col);

            Handles.Label(tile.Value.pos + new Vector3(0, 0.1f, 0), $"{offsetVec3}", textStyle);
        }
    }
#endif

    private void GenGrounds()
    {
        int mapSize = Mathf.Max(mapWidth, mapHeight);
        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapWidth, -q - mapWidth);
            int r2 = Mathf.Min(mapHeight, -q + mapHeight);
            for (int r = r1; r <= r2; r++)
            {
                var tileIndex = new CubeIndex(q, r, -q - r);
                var offset = Util.CubeToOddFlat(tileIndex);
                var offsetVec3 = new Vector3Int(offset.row, offset.col);
                var tilePos = tileMap.CellToLocal(offsetVec3);
                Tile tile = new Tile(tileIndex, tilePos);

                tileMap.SetTile(offsetVec3, groundTile);
                var tileIndexStr = tile.index.str;
                _visited.Add(tileIndexStr, false);
                _gridGrounds.Add(tileIndexStr, tile);
            }
        }
    }
    public void GenAllTileObj()
    {
        if(_gridGrounds.Count == 0)
        {
            return;
        }

        var childs = parentObj.GetComponentsInChildren<Transform>();
        foreach (var child in childs)
        {
            if (child != parentObj.transform)
            {
                Destroy(child.gameObject);
            }
        }
        _tileObjs.Clear();
        foreach (var ground in _gridGrounds)
        {
            var tile = ground.Value;
            tile.isOcupied = false;
            tile.tileObj = null;
        }

        foreach (var ground in _gridGrounds)
        {
            var tile = ground.Value;
            if (tile.tileObj != null)
            {
                continue;
            }
            var go = Instantiate(preFabTileObj);
            go.transform.parent = parentObj.transform;
            go.transform.localPosition = tile.pos;
            var tileObj = go.GetComponent<TileObj>();
            go.name = $"Hex{tile.index.str}";
            tileObj.Tile = tile;
            tileObj.BlockType = RandBlockType();
            _tileObjs.Add(tileObj);
        }

        CheckForMatchs(true);
        if (matchTiles.Count == 0)
        {
            GenAllTileObj();
            return;
        }

        SwapMatchedTiles();
    }

    public void SwapMatchedTiles()
    {
        MatchedSwap();
        CheckForMatchs(true);
        if (matchTiles.Count > 0)
        {
            SwapMatchedTiles();
        }
    }

    public void MatchedSwap()
    {
        foreach (var match in matchTiles)
        {
            var randIndex = UnityEngine.Random.Range(0, match.Count);
            var randMatchTileKey = match[randIndex];
            foreach (var tilekey in match)
            {
                var tile = _gridGrounds[randMatchTileKey];
                var neighbours = Neighbours(tile);
                foreach (var neighbour in neighbours)
                {
                    if (tile.tileObj.BlockType != neighbour.tileObj.BlockType)
                    {
                        tile.tileObj.Swap(neighbour.tileObj, false);
                        return;
                    }
                }
            }
        }
    }

    public void CreateTest(CubeIndex index)
    {
        var tile = _gridGrounds[index.str];
        var go = Instantiate(preFabTileObj);
        go.transform.parent = parentObj.transform;
        go.transform.localPosition = tile.pos;
        go.name = $"Hex{tile.index.str}";
        var tileObj = go.GetComponent<TileObj>();
        tileObj.Tile = tile;
        tileObj.BlockType = BlockType.Blue;
        _tileObjs.Add(tileObj);
    }

    public void GenTileObj()
    {
        var go = Instantiate(preFabTileObj);
        go.transform.parent = parentObj.transform;
        go.transform.localPosition = drobStartPos;
        var tileObj = go.GetComponent<TileObj>();
        Tile tile = new Tile(drobStartCubeIndex, drobStartPos);

        tileObj.Tile = tile;
        go.name = $"Hex{tile.index.str}";

        tileObj.BlockType = RandBlockType();
        _tileObjs.Add(tileObj);

    }

    public void CheckForMatchs(bool onlyFindMatch = false)
    {
        removeTileSet.Clear();
        matchTiles.Clear();
        foreach (var tile in _gridGrounds)
        {
            MatchTwoDir(tile.Value.index, HexDir.RUp, HexDir.LDown, onlyFindMatch);
            MatchTwoDir(tile.Value.index, HexDir.LUp, HexDir.RDown, onlyFindMatch);
            MatchTwoDir(tile.Value.index, HexDir.Up, HexDir.Down, onlyFindMatch);
        }

        if (onlyFindMatch)
        {
            return;
        }

        foreach (var removeTileIndex in removeTileSet)
        {
            var removeTile = _gridGrounds[removeTileIndex];
            _tileObjs.Remove(removeTile.tileObj);

            Destroy(removeTile.tileObj.gameObject);
            removeTile.isOcupied = false;
            removeTile.tileObj = null;
        }
    }


    private void MatchTwoDir(CubeIndex index, HexDir dir1, HexDir dir2, bool findMatch = false)
    {
        var indexKey = index.str;
        if (!_gridGrounds.ContainsKey(indexKey))
        {
            return;
        }

        var tile = _gridGrounds[indexKey];
        if (tile.tileObj == null)
        {
            return;
        }

        if (removeTileSet.Contains(indexKey))
        {
            return;
        }

        foreach (var key in _visited.Keys.ToList())
        {
            _visited[key] = false;
        }

        removeTileList.Clear();

        int matchingItemCount = 1;
        removeTileList.Add(index.str);
        _visited[indexKey] = true;

        var type = tile.tileObj.BlockType;
        var dir1Tile = index + Util.GetCubeIndexDir(dir1);
        if (CheckTile(dir1Tile))
        {
            matchingItemCount += DFSDir(dir1Tile, type, dir1);
        }
        var dir2Tile = index + Util.GetCubeIndexDir(dir2);
        if (CheckTile(dir2Tile))
        {
            matchingItemCount += DFSDir(dir2Tile, type, dir2);
        }

        if (matchingItemCount >= 3)
        {
            if (findMatch)
            {
                var matchList = new List<string>(6);
                foreach (var removeTileIndex in removeTileList)
                    matchList.Add(removeTileIndex);
                matchTiles.Add(matchList);
            }

            foreach (var removeTileIndex in removeTileList)
            {
                removeTileSet.Add(removeTileIndex);
            }

        }
    }

    private int DFSDir(CubeIndex index, BlockType type, HexDir dir)
    {
        var indexKey = index.str;
        if (!_gridGrounds.ContainsKey(indexKey))
        {
            return 0;
        }

        var tile = _gridGrounds[indexKey];
        if (tile.tileObj == null)
        {
            return 0;
        }

        if (tile.tileObj.BlockType != type)
        {
            return 0;
        }

        int matchingItemCount = 1;
        _visited[indexKey] = true;
        removeTileList.Add(indexKey);

        var dirTile = tile.index + Util.GetCubeIndexDir(dir);
        if (CheckTile(dirTile))
        {
            matchingItemCount += DFSDir(dirTile, type, dir);
        }

        return matchingItemCount;
    }

    private bool CheckTile(CubeIndex index)
    {
        var dirTileStr = index.str;
        if (!_visited.ContainsKey(dirTileStr))
        {
            return false;
        }

        return !_visited[dirTileStr];
    }

    public BlockType RandBlockType()
    {
        var blockRandom = UnityEngine.Random.Range(0, 6);
        return (BlockType)Enum.Parse(typeof(BlockType), blockRandom.ToString());
    }

    public Tile GetNeighbourForDir(Tile tile, HexDir dir)
    {
        CubeIndex o = tile.index + Util.GetCubeIndexDir(dir);
        if (!_gridGrounds.ContainsKey(o.str))
            return null;

        return _gridGrounds[o.str];
    }

    public Tile GetNeighbourForDir(CubeIndex index, HexDir dir)
    {
        CubeIndex o = index + Util.GetCubeIndexDir(dir);
        if (!_gridGrounds.ContainsKey(o.str))    
            return null;

        return _gridGrounds[o.str];
    }

    public List<Tile> Neighbours(Tile tile)
    {
        List<Tile> ret = new List<Tile>();
        CubeIndex o;

        for (int i = 0; i < 6; i++)
        {
            o = tile.index + Util.directions[i];
            if (_gridGrounds.ContainsKey(o.str))
                ret.Add(_gridGrounds[o.str]);
        }
        return ret;
    }

    public List<Tile> Neighbours(CubeIndex index)
    {
        return Neighbours(TileAt(index));
    }

    public List<Tile> Neighbours(int x, int y, int z)
    {
        return Neighbours(TileAt(x, y, z));
    }

    public List<Tile> Neighbours(int x, int z)
    {
        return Neighbours(TileAt(x, z));
    }

    public Tile TileAt(CubeIndex index)
    {
        if (_gridGrounds.ContainsKey(index.str))
            return _gridGrounds[index.str];
        return null;
    }

    public Tile TileAt(int x, int y, int z)
    {
        return TileAt(new CubeIndex(x, y, z));
    }

    public Tile TileAt(int x, int z)
    {
        return TileAt(new CubeIndex(x, z));
    }
}
