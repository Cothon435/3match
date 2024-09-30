using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    private static Global _instance;
    public static Global Instance { get { Init(); return _instance; } }

    public Sprite blueNormalBlock;
    public Sprite greenNormalBlock;
    public Sprite orangeNormalBlock;
    public Sprite purpleNormalBlock;
    public Sprite redNormalBlock;
    public Sprite yellowNormalBlock;

    public static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("Global");
            if (go == null)
            {
                go = new GameObject { name = "Global" };
                go.AddComponent<Global>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Global>();
        }
    }
}
