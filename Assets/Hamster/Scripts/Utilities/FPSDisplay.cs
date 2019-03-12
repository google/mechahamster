using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple framerate display
public class FPSDisplay
{
    GUIStyle style;
    Rect displayRect;
    double lastDeltaTime;
    UnityEngine.Networking.CustomNetworkManagerHUD hud;

    public FPSDisplay()
    {
        style = new GUIStyle();
        displayRect = new Rect();
    }

    // Update is called once per frame
    public void Update()
    {
        lastDeltaTime = Time.unscaledDeltaTime;
    }

    public void OnGUI()
    {
        int width = Screen.width;
        int height = Screen.height;
        int fontHeight = height / 50;

        displayRect.height = fontHeight;
        displayRect.width = width;

        style.alignment = TextAnchor.UpperRight;
        style.fontSize = fontHeight;

        double fps = 1.0f / lastDeltaTime;
        if (fps >= 30f)
            style.normal.textColor = Color.green;
        else if (fps < 24f)
            style.normal.textColor = Color.red;
        else
            style.normal.textColor = Color.yellow;

        string displayText = fps.ToString("f2");
        GUI.Label(displayRect, displayText, style);
    }
}
