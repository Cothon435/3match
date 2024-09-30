using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMatch: MonoBehaviour
{
    public CanvasScaler canvasScale;
    public ScreenOrientation orientation;
    public static Vector2 landscapeSIze = new Vector2(1280, 720);
    public static Vector2 portraitSIze = new Vector2(720, 1280);

    private void Start()
    {
        Setting();
        orientation = ScreenOrientation.Portrait;
        StartCoroutine(CheckOrientation());
    }

    private void Setting()
    {
        //Default 해상도 비율
        float fixedAspectRatio = (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown) ? 9f / 16f : 16f / 9f;

        //현재 해상도의 비율
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;

        bool isLandScape;
        isLandScape = currentAspectRatio > fixedAspectRatio;

        //현재 해상도 가로 비율이 더 길 경우 0 or 1, 1280x720 or 720x1280
        float matchWidthOrHeight = isLandScape ? 0 : 1;
        canvasScale.matchWidthOrHeight = matchWidthOrHeight;
        canvasScale.referenceResolution = isLandScape ? landscapeSIze : portraitSIze;
    }

    public IEnumerator CheckOrientation()
    {
        while(true)
        {
            if(orientation != Screen.orientation)
            {
                Setting();
            }
            orientation = Screen.orientation;
            yield return null;
        }
    }


}
