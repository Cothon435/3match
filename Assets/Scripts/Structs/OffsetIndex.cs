using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structs
{
    [System.Serializable]
    public struct OffsetIndex
    {
        public int row;
        public int col;

        public OffsetIndex(int row, int col)
        {
            this.row = row; this.col = col;
        }
    }
}
