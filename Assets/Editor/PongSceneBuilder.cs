using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// Do not directly reference UnityEngine.UI; create UI components via reflection when available.

public class PongSceneBuilder
{
    [MenuItem("Tools/Pong/Generate Pong Scene")]
    public static void GenerateScene()
    {
        // Create a new empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.tag = "MainCamera";

        // Top and bottom walls so the ball bounces off the screen edges
        // Compute playfield extents from the camera
        float inset = 0.25f; // place walls slightly inside so the ball remains visible when bouncing
        float fullWidth = cam.orthographicSize * 2f * cam.aspect;
        float wallThickness = 0.5f;
        float wallY = cam.orthographicSize - inset - (wallThickness * 0.5f);

        // Top wall (physical collider)
        var topWall = new GameObject("TopWall");
        var topCol = topWall.AddComponent<BoxCollider2D>();
        topCol.size = new Vector2(fullWidth + 1f, wallThickness);
        topWall.transform.position = new Vector3(0f, wallY, 0f);
        var topRb = topWall.AddComponent<Rigidbody2D>();
        topRb.bodyType = RigidbodyType2D.Static;
        EnsureTagExists("Wall");
        topWall.tag = "Wall";

        // Visual for top wall (3D cue)
        var topVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(topVis.GetComponent<Collider>());
        topVis.name = "TopWall_Vis";
        topVis.transform.SetParent(topWall.transform, false);
        topVis.transform.localPosition = Vector3.zero;
        topVis.transform.localScale = new Vector3(fullWidth + 1f, wallThickness, 0.5f);
        var topVR = topVis.GetComponent<MeshRenderer>();
        if (topVR != null)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = Color.grey;
            topVR.sharedMaterial = m;
        }

        // Bottom wall (physical collider)
        var bottomWall = new GameObject("BottomWall");
        var bottomCol = bottomWall.AddComponent<BoxCollider2D>();
        bottomCol.size = new Vector2(fullWidth + 1f, wallThickness);
        bottomWall.transform.position = new Vector3(0f, -wallY, 0f);
        var bottomRb = bottomWall.AddComponent<Rigidbody2D>();
        bottomRb.bodyType = RigidbodyType2D.Static;
        bottomWall.tag = "Wall";

        // Visual for bottom wall (3D cue)
        var bottomVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(bottomVis.GetComponent<Collider>());
        bottomVis.name = "BottomWall_Vis";
        bottomVis.transform.SetParent(bottomWall.transform, false);
        bottomVis.transform.localPosition = Vector3.zero;
        bottomVis.transform.localScale = new Vector3(fullWidth + 1f, wallThickness, 0.5f);
        var bottomVR = bottomVis.GetComponent<MeshRenderer>();
        if (bottomVR != null)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = Color.grey;
            bottomVR.sharedMaterial = m;
        }

        // Helper sprite (builtin UI sprite)
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Playfield and paddle sizing (paddle height = 20% of playfield height)
        float playfieldHeight = cam.orthographicSize * 2f;
        float paddleHeight = playfieldHeight * 0.10f; // 10% of full playfield (half of previous 20%)
        float paddleWidth = 0.5f;
        // Canvas + Score Text (create UI components only if the UI assembly is available)
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Try to add CanvasScaler and GraphicRaycaster if the UI package is present
        var canvasScalerType = System.Type.GetType("UnityEngine.UI.CanvasScaler, UnityEngine.UI");
        if (canvasScalerType != null) canvasGO.AddComponent(canvasScalerType);
        var graphicType = System.Type.GetType("UnityEngine.UI.GraphicRaycaster, UnityEngine.UI");
        if (graphicType != null) canvasGO.AddComponent(graphicType);

        // Reflection-friendly UI types (may be null if uGUI package isn't installed)
        var uiTextType = System.Type.GetType("UnityEngine.UI.Text, UnityEngine.UI");
        var uiImageType = System.Type.GetType("UnityEngine.UI.Image, UnityEngine.UI");

        // Left score (GameObject). If Text type exists, add it and set properties via reflection.
        var leftTextGO = new GameObject("LeftScoreText");
        leftTextGO.transform.SetParent(canvasGO.transform);
        object leftTextComp = null;
        if (uiTextType != null)
        {
            leftTextComp = leftTextGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font");
            if (fontProp != null) fontProp.SetValue(leftTextComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(leftTextComp, "0", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(leftTextComp, TextAnchor.UpperLeft, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(leftTextComp, 32, null);
        }
        var rtL = leftTextGO.GetComponent<RectTransform>();
        // move scores slightly inward so top/bottom wall visuals don't overlap
        rtL.anchorMin = new Vector2(0.35f, 0.92f);
        rtL.anchorMax = rtL.anchorMin;
        rtL.anchoredPosition = Vector2.zero;
        rtL.sizeDelta = new Vector2(100, 50);

        // Right score
        var rightTextGO = new GameObject("RightScoreText");
        rightTextGO.transform.SetParent(canvasGO.transform);
        object rightTextComp = null;
        if (uiTextType != null)
        {
            rightTextComp = rightTextGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font");
            if (fontProp != null) fontProp.SetValue(rightTextComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(rightTextComp, "0", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(rightTextComp, TextAnchor.UpperRight, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(rightTextComp, 32, null);
        }
        var rtR = rightTextGO.GetComponent<RectTransform>();
        rtR.anchorMin = new Vector2(0.65f, 0.92f);
        rtR.anchorMax = rtR.anchorMin;
        rtR.anchoredPosition = Vector2.zero;
        rtR.sizeDelta = new Vector2(100, 50);

        // On-screen help texts near each paddle
        var leftHelpGO = new GameObject("LeftHelpText");
        leftHelpGO.transform.SetParent(canvasGO.transform);
        object leftHelpComp = null;
        if (uiTextType != null)
        {
            leftHelpComp = leftHelpGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(leftHelpComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(leftHelpComp, "W / S", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(leftHelpComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(leftHelpComp, 18, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(leftHelpComp, Color.gray, null);
        }
        var helpLRT = leftHelpGO.GetComponent<RectTransform>();
        helpLRT.anchorMin = new Vector2(0.18f, 0.5f);
        helpLRT.anchorMax = helpLRT.anchorMin;
        helpLRT.anchoredPosition = Vector2.zero;
        helpLRT.sizeDelta = new Vector2(100, 30);

        var rightHelpGO = new GameObject("RightHelpText");
        rightHelpGO.transform.SetParent(canvasGO.transform);
        object rightHelpComp = null;
        if (uiTextType != null)
        {
            rightHelpComp = rightHelpGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(rightHelpComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(rightHelpComp, "P / L", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(rightHelpComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(rightHelpComp, 18, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(rightHelpComp, Color.gray, null);
        }
        var helpRRT = rightHelpGO.GetComponent<RectTransform>();
        helpRRT.anchorMin = new Vector2(0.82f, 0.5f);
        helpRRT.anchorMax = helpRRT.anchorMin;
        helpRRT.anchoredPosition = Vector2.zero;
        helpRRT.sizeDelta = new Vector2(100, 30);

        // Center countdown text (hidden initially)
        var countdownGO = new GameObject("CountdownText");
        countdownGO.transform.SetParent(canvasGO.transform);
        object countdownComp = null;
        if (uiTextType != null)
        {
            countdownComp = countdownGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(countdownComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(countdownComp, "", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(countdownComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(countdownComp, 48, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(countdownComp, Color.white, null);
            // hide initially
            var go = countdownGO; go.SetActive(false);
        }
        var ctRT = countdownGO.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0.5f, 0.5f);
        ctRT.anchorMax = ctRT.anchorMin;
        ctRT.anchoredPosition = Vector2.zero;
        ctRT.sizeDelta = new Vector2(200, 100);
        rbBall.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ballGO.AddComponent<Ball>();

        // 3D visual for ball (sphere)
        var ballVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(ballVis.GetComponent<Collider>());
        ballVis.name = "Ball_Vis";
        ballVis.transform.SetParent(ballGO.transform, false);
        ballVis.transform.localPosition = new Vector3(0f, 0f, -0.25f);
        ballVis.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        var ballR = ballVis.GetComponent<MeshRenderer>();
        if (ballR != null)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = Color.white;
            ballR.sharedMaterial = m;
        }

        // Goals
        var goalLeft = new GameObject("GoalLeft");
        var goalLeftCollider = goalLeft.AddComponent<BoxCollider2D>();
        goalLeftCollider.isTrigger = true;
        goalLeft.transform.position = new Vector3(-12f, 0f, 0f);
        goalLeft.transform.localScale = new Vector3(1f, 10f, 1f);
        EnsureTagExists("GoalLeft");
        goalLeft.tag = "GoalLeft";

        var goalRight = new GameObject("GoalRight");
        var goalRightCollider = goalRight.AddComponent<BoxCollider2D>();
        goalRightCollider.isTrigger = true;
        goalRight.transform.position = new Vector3(12f, 0f, 0f);
        goalRight.transform.localScale = new Vector3(1f, 10f, 1f);
        EnsureTagExists("GoalRight");
        goalRight.tag = "GoalRight";

        // Canvas + Score Text
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Left score
        var leftTextGO = new GameObject("LeftScoreText");
        leftTextGO.transform.SetParent(canvasGO.transform);
        // Try to add UI.Text via reflection if available
        var uiTextType_local = System.Type.GetType("UnityEngine.UI.Text, UnityEngine.UI");
        if (uiTextType_local != null)
        {
            var leftTextComp = leftTextGO.AddComponent(uiTextType_local);
            var fontProp = uiTextType_local.GetProperty("font"); if (fontProp != null) fontProp.SetValue(leftTextComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType_local.GetProperty("text"); if (textProp != null) textProp.SetValue(leftTextComp, "0", null);
            var alignProp = uiTextType_local.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(leftTextComp, TextAnchor.UpperLeft, null);
            var fsProp = uiTextType_local.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(leftTextComp, 32, null);
        }
        var rtL = leftTextGO.GetComponent<RectTransform>();
        // move scores slightly inward so top/bottom wall visuals don't overlap
        rtL.anchorMin = new Vector2(0.35f, 0.92f);
        rtL.anchorMax = rtL.anchorMin;
        rtL.anchoredPosition = Vector2.zero;
        rtL.sizeDelta = new Vector2(100, 50);

        // Right score
        var rightTextGO = new GameObject("RightScoreText");
        rightTextGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var rightTextComp = rightTextGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(rightTextComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(rightTextComp, "0", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(rightTextComp, TextAnchor.UpperRight, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(rightTextComp, 32, null);
        }
        var rtR = rightTextGO.GetComponent<RectTransform>();
        rtR.anchorMin = new Vector2(0.65f, 0.92f);
        rtR.anchorMax = rtR.anchorMin;
        rtR.anchoredPosition = Vector2.zero;
        rtR.sizeDelta = new Vector2(100, 50);

        // On-screen help texts near each paddle
        var leftHelpGO = new GameObject("LeftHelpText");
        leftHelpGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var leftHelpComp = leftHelpGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(leftHelpComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(leftHelpComp, "W / S", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(leftHelpComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(leftHelpComp, 18, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(leftHelpComp, Color.gray, null);
        }
        var helpLRT = leftHelpGO.GetComponent<RectTransform>();
        // move help texts slightly inward from screen edges
        helpLRT.anchorMin = new Vector2(0.18f, 0.5f);
        helpLRT.anchorMax = helpLRT.anchorMin;
        helpLRT.anchoredPosition = Vector2.zero;
        helpLRT.sizeDelta = new Vector2(100, 30);

        var rightHelpGO = new GameObject("RightHelpText");
        rightHelpGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var rightHelpComp = rightHelpGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(rightHelpComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(rightHelpComp, "P / L", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(rightHelpComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(rightHelpComp, 18, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(rightHelpComp, Color.gray, null);
        }
        var helpRRT = rightHelpGO.GetComponent<RectTransform>();
        helpRRT.anchorMin = new Vector2(0.82f, 0.5f);
        helpRRT.anchorMax = helpRRT.anchorMin;
        helpRRT.anchoredPosition = Vector2.zero;
        helpRRT.sizeDelta = new Vector2(100, 30);

        // Center countdown text (hidden initially)
        var countdownGO = new GameObject("CountdownText");
        countdownGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var countdownComp = countdownGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(countdownComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(countdownComp, "", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(countdownComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(countdownComp, 48, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(countdownComp, Color.white, null);
            countdownGO.SetActive(false);
        }
        var ctRT = countdownGO.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0.5f, 0.5f);
        ctRT.anchorMax = ctRT.anchorMin;
        ctRT.anchoredPosition = Vector2.zero;
        ctRT.sizeDelta = new Vector2(200, 100);

        // UIManager
        var uiGO = new GameObject("UIManager");
        var uiManager = uiGO.AddComponent<UIManager>();
        uiManager.leftScoreObj = leftTextGO;
        uiManager.rightScoreObj = rightTextGO;
        uiManager.countdownObj = countdownGO;

        // Draw visible playfield border using UI images (add Images via reflection if available)
        var uiImageType_local = System.Type.GetType("UnityEngine.UI.Image, UnityEngine.UI");

        // Top border image
        var topBorderGO = new GameObject("UI_TopBorder");
        topBorderGO.transform.SetParent(canvasGO.transform);
        if (uiImageType_local != null)
        {
            var topImg = topBorderGO.AddComponent(uiImageType_local);
            var colorProp = uiImageType_local.GetProperty("color"); if (colorProp != null) colorProp.SetValue(topImg, Color.white, null);
        }
        var topRT = topBorderGO.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 1f);
        topRT.anchorMax = new Vector2(1f, 1f);
        topRT.pivot = new Vector2(0.5f, 1f);
        topRT.anchoredPosition = Vector2.zero;
        topRT.sizeDelta = new Vector2(0f, 6f);

        // Bottom border image
        var bottomBorderGO = new GameObject("UI_BottomBorder");
        bottomBorderGO.transform.SetParent(canvasGO.transform);
        if (uiImageType_local != null)
        {
            var bottomImg = bottomBorderGO.AddComponent(uiImageType_local);
            var colorProp = uiImageType_local.GetProperty("color"); if (colorProp != null) colorProp.SetValue(bottomImg, Color.white, null);
        }
        var bottomRT = bottomBorderGO.GetComponent<RectTransform>();
        bottomRT.anchorMin = new Vector2(0f, 0f);
        bottomRT.anchorMax = new Vector2(1f, 0f);
        bottomRT.pivot = new Vector2(0.5f, 0f);
        bottomRT.anchoredPosition = Vector2.zero;
        bottomRT.sizeDelta = new Vector2(0f, 6f);

        // Left border image
        var leftBorderGO = new GameObject("UI_LeftBorder");
        leftBorderGO.transform.SetParent(canvasGO.transform);
        if (uiImageType_local != null)
        {
            var leftImg = leftBorderGO.AddComponent(uiImageType_local);
            var colorProp = uiImageType_local.GetProperty("color"); if (colorProp != null) colorProp.SetValue(leftImg, Color.white, null);
        }
        var leftRT = leftBorderGO.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0f, 0f);
        leftRT.anchorMax = new Vector2(0f, 1f);
        leftRT.pivot = new Vector2(0f, 0.5f);
        leftRT.anchoredPosition = Vector2.zero;
        leftRT.sizeDelta = new Vector2(6f, 0f);

        // Right border image
        var rightBorderGO = new GameObject("UI_RightBorder");
        rightBorderGO.transform.SetParent(canvasGO.transform);
        if (uiImageType_local != null)
        {
            var rightImg = rightBorderGO.AddComponent(uiImageType_local);
            var colorProp = uiImageType_local.GetProperty("color"); if (colorProp != null) colorProp.SetValue(rightImg, Color.white, null);
        }
        var rightRT = rightBorderGO.GetComponent<RectTransform>();
        rightRT.anchorMin = new Vector2(1f, 0f);
        rightRT.anchorMax = new Vector2(1f, 1f);
        rightRT.pivot = new Vector2(1f, 0.5f);
        rightRT.anchoredPosition = Vector2.zero;
        rightRT.sizeDelta = new Vector2(6f, 0f);

        // Winner texts (hidden initially)
        var winnerLeftGO = new GameObject("WinnerLeftText");
        winnerLeftGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var winnerLeftComp = winnerLeftGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(winnerLeftComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(winnerLeftComp, "", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(winnerLeftComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(winnerLeftComp, 64, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(winnerLeftComp, Color.yellow, null);
            winnerLeftGO.SetActive(false);
        }
        var wlRT = winnerLeftGO.GetComponent<RectTransform>();
        wlRT.anchorMin = new Vector2(0.25f, 0.6f);
        wlRT.anchorMax = wlRT.anchorMin;
        wlRT.anchoredPosition = Vector2.zero;
        wlRT.sizeDelta = new Vector2(300, 120);

        var winnerRightGO = new GameObject("WinnerRightText");
        winnerRightGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var winnerRightComp = winnerRightGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(winnerRightComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(winnerRightComp, "", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(winnerRightComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(winnerRightComp, 64, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(winnerRightComp, Color.yellow, null);
            winnerRightGO.SetActive(false);
        }
        var wrRT = winnerRightGO.GetComponent<RectTransform>();
        wrRT.anchorMin = new Vector2(0.75f, 0.6f);
        wrRT.anchorMax = wrRT.anchorMin;
        wrRT.anchoredPosition = Vector2.zero;
        wrRT.sizeDelta = new Vector2(300, 120);

        // Play again prompts
        var playLeftGO = new GameObject("PlayAgainLeftText");
        playLeftGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var playLeftComp = playLeftGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(playLeftComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(playLeftComp, "", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(playLeftComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(playLeftComp, 22, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(playLeftComp, Color.white, null);
            playLeftGO.SetActive(false);
        }
        var plRT = playLeftGO.GetComponent<RectTransform>();
        plRT.anchorMin = new Vector2(0.25f, 0.45f);
        plRT.anchorMax = plRT.anchorMin;
        plRT.anchoredPosition = Vector2.zero;
        plRT.sizeDelta = new Vector2(200, 40);

        var playRightGO = new GameObject("PlayAgainRightText");
        playRightGO.transform.SetParent(canvasGO.transform);
        if (uiTextType != null)
        {
            var playRightComp = playRightGO.AddComponent(uiTextType);
            var fontProp = uiTextType.GetProperty("font"); if (fontProp != null) fontProp.SetValue(playRightComp, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null);
            var textProp = uiTextType.GetProperty("text"); if (textProp != null) textProp.SetValue(playRightComp, "", null);
            var alignProp = uiTextType.GetProperty("alignment"); if (alignProp != null) alignProp.SetValue(playRightComp, TextAnchor.MiddleCenter, null);
            var fsProp = uiTextType.GetProperty("fontSize"); if (fsProp != null) fsProp.SetValue(playRightComp, 22, null);
            var colorProp = uiTextType.GetProperty("color"); if (colorProp != null) colorProp.SetValue(playRightComp, Color.white, null);
            playRightGO.SetActive(false);
        }
        var prRT = playRightGO.GetComponent<RectTransform>();
        prRT.anchorMin = new Vector2(0.75f, 0.45f);
        prRT.anchorMax = prRT.anchorMin;
        prRT.anchoredPosition = Vector2.zero;
        prRT.sizeDelta = new Vector2(200, 40);

        // Assign to UIManager
        uiManager.winnerLeftObj = winnerLeftGO;
        uiManager.winnerRightObj = winnerRightGO;
        uiManager.playAgainLeftObj = playLeftGO;
        uiManager.playAgainRightObj = playRightGO;

        // GameManager
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        gm.ball = ballGO.GetComponent<Ball>();
        gm.ui = uiManager;

        // Link ball reference for paddles' AI (they find ball on Start)

        // Ensure Scenes folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/PongScene.unity");

        // Ping objects in hierarchy
        Selection.activeGameObject = gmGO;

        Debug.Log("Pong scene generated: Assets/Scenes/PongScene.unity\nRemember to review and tweak inspector values (Ball speed, Paddle speed, etc.).");
    }

    static void EnsureTagExists(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            var t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue == tag) return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
