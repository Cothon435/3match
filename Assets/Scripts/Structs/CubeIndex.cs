using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structs
{
    [System.Serializable]
    public struct CubeIndex
    {
        public int x;
        public int y;
        public int z;
        public string str;

        public CubeIndex(int x, int y, int z)
        {
            this.x = x; this.y = y; this.z = z;
            this.str = $"[{x},{y},{z}]";
        }

        public CubeIndex(int x, int z)
        {
            this.x = x; this.z = z; this.y = -x - z;
            this.str = $"[{x},{y},{z}]";
        }

        public static CubeIndex operator +(CubeIndex one, CubeIndex two)
        {
            return new CubeIndex(one.x + two.x, one.y + two.y, one.z + two.z);
        }
        public static CubeIndex operator *(CubeIndex one, int scal)
        {
            return new CubeIndex(one.x * scal, one.y * scal, one.z * scal);
        }
        public static bool operator ==(CubeIndex one, CubeIndex two)
        {
            return one.x == two.x && one.y == two.y && one.z == two.z;
        }
        public static bool operator !=(CubeIndex one, CubeIndex two)
        {
            return !(one == two);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            CubeIndex o = (CubeIndex)obj;
            if ((System.Object)o == null)
                return false;
            return ((x == o.x) && (y == o.y) && (z == o.z));
        }

        public override int GetHashCode()
        {
            return (x.GetHashCode() ^ (y.GetHashCode() + (int)(Mathf.Pow(2, 32) / (1 + Mathf.Sqrt(5)) / 2) + (x.GetHashCode() << 6) + (x.GetHashCode() >> 2)));
        }

        public override string ToString()
        {
            return $"[{x},{y},{z}]";
        }
    }
}
