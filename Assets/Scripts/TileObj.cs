using Enums;
using System;
using System.Collections;
using UnityEngine;

public class TileObj : MonoBehaviour
{
    private Tile _tile;
    public Tile Tile
    {
        get { return _tile; }
        set
        {
            if (_tile != null)
            {
                _tile.tileObj = value.tileObj;
                _tile.isOcupied = value.tileObj != null;
            }
            _tile = value;
            _tile.tileObj = this;
            _tile.isOcupied = true;
        }
    }

    private BlockType _blockType;
    public BlockType BlockType
    {
        get { return _blockType; }
        set
        {
            _blockType = value;
            spriteRenderer.sprite = Util.GetBlockSprite(_blockType);
        }
    }
    public SpriteRenderer spriteRenderer;

    public float moveTime = 0.5f;
    private float _currentTime = 0.0f;
    public float CurrentTime { get { return _currentTime; } }
    private Coroutine _coJump = null;


    void Update()
    {
        //코루틴 작동중이면 이동 안함
        if (_currentTime > 0.0f)
        {
            return;
        }

        ActionMove();
    }

    public void ActionMove()
    {
        var nextTile = FindNextTile();
        if (nextTile == null)
        {
            return;
        }

        Tile = nextTile;
        Jump(transform.position, nextTile.pos);
    }


    public Tile FindNextTile()
    {
        var nextTile = MoveDown();
        if (nextTile == null)
        {
            var isLeft = UnityEngine.Random.Range(0, 2) == 0;
            nextTile = isLeft ? MoveLDown() : MoveRDown();
            if (nextTile == null)
            {
                nextTile = isLeft ? MoveRDown() : MoveLDown();
            }
        }
        return nextTile;
    }

    public Tile MoveDown()
    {
        var nextTile = _tile.NextTileDir(HexDir.Down);
        if (nextTile == null)
        {
            return null;
        }
        if (nextTile.isOcupied)
        {
            return null;
        }
        return nextTile;
    }

    public Tile MoveLDown()
    {
        if (!IsMoveLRDown())
        {
            return null;
        }
        var nextLDownTile = _tile.NextTileDir(HexDir.LDown);
        if (nextLDownTile == null)
        {
            return null;
        }
        if (nextLDownTile.isOcupied)
        {
            return null;
        }
        return nextLDownTile;
    }

    public Tile MoveRDown()
    {
        if (!IsMoveLRDown())
        {
            return null;
        }
        var nextRDownTile = _tile.NextTileDir(HexDir.RDown);
        if (nextRDownTile == null)
        {
            return null;
        }
        if (nextRDownTile.isOcupied)
        {
            return null;
        }
        return nextRDownTile;
    }

    public bool IsMoveLRDown()
    {
        var nextLUpTile = _tile.NextTileDir(HexDir.LUp);
        var nextRUpTile = _tile.NextTileDir(HexDir.RUp);
        if (nextLUpTile != null && nextRUpTile != null && 
            !nextLUpTile.isOcupied && !nextRUpTile.isOcupied)
        {
            return true;
        }
        var upTile = SceneGame.Instance.GetNeighbourForDir(_tile.index, HexDir.Up);
        if (upTile != null && upTile.isOcupied)
        {
            return false;
        }
        return true;
    }


    public void Swap(TileObj nextTileObj, bool onAni = true, Action endCallBack = null)
    {
        if (onAni)
        {
            Jump(_tile.pos, nextTileObj.Tile.pos, endCallBack);
            nextTileObj.Jump(nextTileObj.Tile.pos, _tile.pos);
        }
        else
        {
            this.transform.position = nextTileObj.Tile.pos;
            nextTileObj.transform.position = _tile.pos;
        }
        SwapData(nextTileObj);
    }

    private void SwapData(TileObj nextTileObj)
    {
        var temp = SceneGame.Instance.GetTile(this.Tile.index.str);
        this._tile = SceneGame.Instance.GetTile(nextTileObj.Tile.index.str);
        nextTileObj.Tile = temp;
    }

    public void Jump(Vector3 startPos, Vector3 endPos, Action endCallback = null)
    {
        if (_coJump != null)
        {
            StopCoroutine(_coJump);
            _coJump = null;
        }

        _coJump = StartCoroutine(MoveTowardsTarget(startPos, endPos, endCallback));
    }

    private IEnumerator MoveTowardsTarget(Vector3 startPos, Vector3 endPos, Action endCallback = null)
    {
        _currentTime = 0.0f;
        while (_currentTime < moveTime)
        {
            _currentTime += Time.deltaTime;
            float t = _currentTime / moveTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        if (endCallback != null)
        {
            endCallback.Invoke();
        }
        _currentTime = 0.0f;
    }
}
