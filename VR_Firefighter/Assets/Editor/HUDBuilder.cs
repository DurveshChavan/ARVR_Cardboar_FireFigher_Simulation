using UnityEngine;
using UnityEditor;
using TMPro;

public class HUDBuilder
{
    [MenuItem("VR Firefighter/Build HUD Canvas")]
    public static void BuildHUD()
    {
        // 1. Find the Main Camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main Camera not found! Cannot attach HUD.");
            return;
        }

        // 2. Create the Canvas
        GameObject canvasObj = new GameObject("HUD_Canvas");
        canvasObj.transform.parent = mainCam.transform;
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>(); // Optional for UI hits

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1920, 1080);
        canvasRect.localPosition = new Vector3(0, 0, 1.5f);
        canvasRect.localEulerAngles = Vector3.zero;
        canvasRect.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);

        // 3. Create TimerText
        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.parent = canvasObj.transform;
        TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.pivot = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition3D = new Vector3(0, -60, 0);
        timerRect.localEulerAngles = Vector3.zero;
        timerRect.localScale = Vector3.one;
        
        timerText.fontSize = 80;
        timerText.color = Color.white;
        timerText.text = "Time: 60s";
        timerText.alignment = TextAlignmentOptions.Center;
        timerObj.SetActive(true);

        // 4. Create ExtinguisherText
        GameObject extObj = new GameObject("ExtinguisherText");
        extObj.transform.parent = canvasObj.transform;
        TextMeshProUGUI extText = extObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform extRect = extObj.GetComponent<RectTransform>();
        extRect.anchorMin = new Vector2(0f, 0f);
        extRect.anchorMax = new Vector2(0f, 0f);
        extRect.pivot = new Vector2(0f, 0f);
        extRect.anchoredPosition3D = new Vector3(200, 100, 0);
        extRect.sizeDelta = new Vector2(900, 200); // specify width
        extRect.localEulerAngles = Vector3.zero;
        extRect.localScale = Vector3.one;

        extText.fontSize = 50;
        ColorUtility.TryParseHtmlString("#FFEE00", out Color yCol);
        extText.color = yCol;
        extText.text = "";
        extText.alignment = TextAlignmentOptions.Left;
        extObj.SetActive(false);

        // 5. Create ResultText
        GameObject resObj = new GameObject("ResultText");
        resObj.transform.parent = canvasObj.transform;
        TextMeshProUGUI resText = resObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform resRect = resObj.GetComponent<RectTransform>();
        resRect.anchorMin = new Vector2(0.5f, 0.5f);
        resRect.anchorMax = new Vector2(0.5f, 0.5f);
        resRect.pivot = new Vector2(0.5f, 0.5f);
        resRect.anchoredPosition3D = Vector3.zero;
        resRect.sizeDelta = new Vector2(1600, 400); // specify width
        resRect.localEulerAngles = Vector3.zero;
        resRect.localScale = Vector3.one;

        resText.fontSize = 100;
        resText.color = Color.white;
        resText.text = "";
        resText.alignment = TextAlignmentOptions.Center;
        resText.textWrappingMode = TextWrappingModes.Normal;
        resObj.SetActive(false);

        // 6. Wire to GameManager
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.timerText = timerText;
            gm.extText = extText;
            gm.resultText = resText;
            EditorUtility.SetDirty(gm);
            Debug.Log("HUD wired to GameManager.");
        }
        else
        {
            Debug.LogWarning("GameManager not found in scene. HUD objects created but not wired.");
        }

        // 7. Verify SelectionScreen decoupling
        GameObject selectionScreen = GameObject.Find("SelectionScreen");
        if (selectionScreen == null) selectionScreen = FindInactiveObjectByName("SelectionScreen");

        if (selectionScreen != null)
        {
            selectionScreen.transform.parent = null;
            selectionScreen.transform.position = new Vector3(0, 1.5f, 3f);
            Debug.Log("SelectionScreen decoupled from camera and set to World Space pos (0, 1.5, 3).");
        }

        Debug.Log("HUD Canvas successfully built and parented to Main Camera.");
    }

    private static GameObject FindInactiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None && objs[i].name == name)
            {
                return objs[i].gameObject;
            }
        }
        return null;
    }
}
