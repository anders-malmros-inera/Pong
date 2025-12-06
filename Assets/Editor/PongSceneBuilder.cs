using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

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
        // Use the camera orthographic size to place walls slightly inside the vertical bounds
        float camHeight = cam.orthographicSize;
        float inset = 0.25f; // place walls slightly inside so the ball remains visible when bouncing
        float wallY = camHeight - inset;

        // Top wall
        var topWall = new GameObject("TopWall");
        var topCol = topWall.AddComponent<BoxCollider2D>();
        topCol.size = new Vector2(100f, 0.5f);
        topWall.transform.position = new Vector3(0f, wallY, 0f);
        var topRb = topWall.AddComponent<Rigidbody2D>();
        topRb.bodyType = RigidbodyType2D.Static;
        EnsureTagExists("Wall");
        topWall.tag = "Wall";

        // Bottom wall
        var bottomWall = new GameObject("BottomWall");
        var bottomCol = bottomWall.AddComponent<BoxCollider2D>();
        bottomCol.size = new Vector2(100f, 0.5f);
        bottomWall.transform.position = new Vector3(0f, -wallY, 0f);
        var bottomRb = bottomWall.AddComponent<Rigidbody2D>();
        bottomRb.bodyType = RigidbodyType2D.Static;
        bottomWall.tag = "Wall";

        // Helper sprite (builtin UI sprite)
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Playfield and paddle sizing (paddle height = 20% of playfield height)
        float playfieldHeight = cam.orthographicSize * 2f;
        float paddleHeight = playfieldHeight * 0.10f; // 10% of full playfield (half of previous 20%)
        float paddleWidth = 0.5f;
        float paddleVisualDepth = 0.5f;
        float paddleZOffset = -0.5f; // slightly in front of 2D sprite (negative z towards camera)

        // Left Paddle
        var paddleLeft = new GameObject("PaddleLeft");
        var srL = paddleLeft.AddComponent<SpriteRenderer>();
        srL.sprite = sprite;
        srL.color = Color.white;
        paddleLeft.transform.position = new Vector3(-8f, 0f, 0f);
        // Size paddles relative to playfield height
        paddleLeft.transform.localScale = new Vector3(paddleWidth, paddleHeight, 1f);
        var colL = paddleLeft.AddComponent<BoxCollider2D>();
        colL.size = new Vector2(paddleWidth, paddleHeight);
        var rbL = paddleLeft.AddComponent<Rigidbody2D>();
        rbL.bodyType = RigidbodyType2D.Kinematic;
        rbL.freezeRotation = true;
        var paddleScriptL = paddleLeft.AddComponent<Paddle>();
        paddleScriptL.isLeft = true;
        EnsureTagExists("Paddle");
        paddleLeft.tag = "Paddle";
        
        // 3D visual for left paddle (cube) - child of physics object
        var leftVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(leftVis.GetComponent<Collider>());
        leftVis.name = "PaddleLeft_Vis";
        leftVis.transform.SetParent(paddleLeft.transform, false);
        leftVis.transform.localPosition = new Vector3(0f, 0f, paddleZOffset);
        leftVis.transform.localRotation = Quaternion.Euler(10f, 20f, 0f);
        leftVis.transform.localScale = new Vector3(paddleWidth, paddleHeight, paddleVisualDepth);
        var leftR = leftVis.GetComponent<MeshRenderer>();
        if (leftR != null) leftR.material.color = Color.cyan;

        // Right Paddle
        var paddleRight = new GameObject("PaddleRight");
        var srR = paddleRight.AddComponent<SpriteRenderer>();
        srR.sprite = sprite;
        srR.color = Color.white;
        paddleRight.transform.position = new Vector3(8f, 0f, 0f);
        // Size paddles relative to playfield height
        paddleRight.transform.localScale = new Vector3(paddleWidth, paddleHeight, 1f);
        var colR = paddleRight.AddComponent<BoxCollider2D>();
        colR.size = new Vector2(paddleWidth, paddleHeight);
        var rbR = paddleRight.AddComponent<Rigidbody2D>();
        rbR.bodyType = RigidbodyType2D.Kinematic;
        rbR.freezeRotation = true;
        var paddleScriptR = paddleRight.AddComponent<Paddle>();
        paddleScriptR.isLeft = false;
        paddleRight.tag = "Paddle";

        // 3D visual for right paddle (cube)
        var rightVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(rightVis.GetComponent<Collider>());
        rightVis.name = "PaddleRight_Vis";
        rightVis.transform.SetParent(paddleRight.transform, false);
        rightVis.transform.localPosition = new Vector3(0f, 0f, paddleZOffset);
        rightVis.transform.localRotation = Quaternion.Euler(10f, -20f, 0f);
        rightVis.transform.localScale = new Vector3(paddleWidth, paddleHeight, paddleVisualDepth);
        var rightR = rightVis.GetComponent<MeshRenderer>();
        if (rightR != null) rightR.material.color = Color.magenta;

        // Ball
        var ballGO = new GameObject("Ball");
        var srB = ballGO.AddComponent<SpriteRenderer>();
        srB.sprite = sprite;
        srB.color = Color.white;
        ballGO.transform.position = Vector3.zero;
        ballGO.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        ballGO.AddComponent<CircleCollider2D>();
        var rbBall = ballGO.AddComponent<Rigidbody2D>();
        rbBall.gravityScale = 0f;
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
        if (ballR != null) ballR.material.color = Color.white;

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
        var leftText = leftTextGO.AddComponent<Text>();
        leftText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        leftText.text = "0";
        leftText.alignment = TextAnchor.UpperLeft;
        leftText.fontSize = 32;
        var rtL = leftText.GetComponent<RectTransform>();
        rtL.anchorMin = new Vector2(0.25f, 0.95f);
        rtL.anchorMax = rtL.anchorMin;
        rtL.anchoredPosition = Vector2.zero;
        rtL.sizeDelta = new Vector2(100, 50);

        // Right score
        var rightTextGO = new GameObject("RightScoreText");
        rightTextGO.transform.SetParent(canvasGO.transform);
        var rightText = rightTextGO.AddComponent<Text>();
        rightText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rightText.text = "0";
        rightText.alignment = TextAnchor.UpperRight;
        rightText.fontSize = 32;
        var rtR = rightText.GetComponent<RectTransform>();
        rtR.anchorMin = new Vector2(0.75f, 0.95f);
        rtR.anchorMax = rtR.anchorMin;
        rtR.anchoredPosition = Vector2.zero;
        rtR.sizeDelta = new Vector2(100, 50);

        // On-screen help texts near each paddle
        var leftHelpGO = new GameObject("LeftHelpText");
        leftHelpGO.transform.SetParent(canvasGO.transform);
        var leftHelp = leftHelpGO.AddComponent<Text>();
        leftHelp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        leftHelp.text = "W / S";
        leftHelp.alignment = TextAnchor.MiddleCenter;
        leftHelp.fontSize = 18;
        leftHelp.color = Color.gray;
        var helpLRT = leftHelp.GetComponent<RectTransform>();
        helpLRT.anchorMin = new Vector2(0.1f, 0.5f);
        helpLRT.anchorMax = helpLRT.anchorMin;
        helpLRT.anchoredPosition = Vector2.zero;
        helpLRT.sizeDelta = new Vector2(100, 30);

        var rightHelpGO = new GameObject("RightHelpText");
        rightHelpGO.transform.SetParent(canvasGO.transform);
        var rightHelp = rightHelpGO.AddComponent<Text>();
        rightHelp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rightHelp.text = "P / L";
        rightHelp.alignment = TextAnchor.MiddleCenter;
        rightHelp.fontSize = 18;
        rightHelp.color = Color.gray;
        var helpRRT = rightHelp.GetComponent<RectTransform>();
        helpRRT.anchorMin = new Vector2(0.9f, 0.5f);
        helpRRT.anchorMax = helpRRT.anchorMin;
        helpRRT.anchoredPosition = Vector2.zero;
        helpRRT.sizeDelta = new Vector2(100, 30);

        // Center countdown text (hidden initially)
        var countdownGO = new GameObject("CountdownText");
        countdownGO.transform.SetParent(canvasGO.transform);
        var countdownText = countdownGO.AddComponent<Text>();
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.text = "";
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.fontSize = 48;
        countdownText.color = Color.white;
        countdownText.gameObject.SetActive(false);
        var ctRT = countdownText.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0.5f, 0.5f);
        ctRT.anchorMax = ctRT.anchorMin;
        ctRT.anchoredPosition = Vector2.zero;
        ctRT.sizeDelta = new Vector2(200, 100);

        // UIManager
        var uiGO = new GameObject("UIManager");
        var uiManager = uiGO.AddComponent<UIManager>();
        uiManager.leftScoreText = leftText;
        uiManager.rightScoreText = rightText;
        uiManager.countdownText = countdownText;

        // Draw visible playfield border using UI images so players see the active area
        // Top border image
        var topBorderGO = new GameObject("UI_TopBorder");
        topBorderGO.transform.SetParent(canvasGO.transform);
        var topImg = topBorderGO.AddComponent<UnityEngine.UI.Image>();
        topImg.color = Color.white;
        var topRT = topImg.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 1f);
        topRT.anchorMax = new Vector2(1f, 1f);
        topRT.pivot = new Vector2(0.5f, 1f);
        topRT.anchoredPosition = Vector2.zero;
        topRT.sizeDelta = new Vector2(0f, 6f);

        // Bottom border image
        var bottomBorderGO = new GameObject("UI_BottomBorder");
        bottomBorderGO.transform.SetParent(canvasGO.transform);
        var bottomImg = bottomBorderGO.AddComponent<UnityEngine.UI.Image>();
        bottomImg.color = Color.white;
        var bottomRT = bottomImg.GetComponent<RectTransform>();
        bottomRT.anchorMin = new Vector2(0f, 0f);
        bottomRT.anchorMax = new Vector2(1f, 0f);
        bottomRT.pivot = new Vector2(0.5f, 0f);
        bottomRT.anchoredPosition = Vector2.zero;
        bottomRT.sizeDelta = new Vector2(0f, 6f);

        // Left border image
        var leftBorderGO = new GameObject("UI_LeftBorder");
        leftBorderGO.transform.SetParent(canvasGO.transform);
        var leftImg = leftBorderGO.AddComponent<UnityEngine.UI.Image>();
        leftImg.color = Color.white;
        var leftRT = leftImg.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0f, 0f);
        leftRT.anchorMax = new Vector2(0f, 1f);
        leftRT.pivot = new Vector2(0f, 0.5f);
        leftRT.anchoredPosition = Vector2.zero;
        leftRT.sizeDelta = new Vector2(6f, 0f);

        // Right border image
        var rightBorderGO = new GameObject("UI_RightBorder");
        rightBorderGO.transform.SetParent(canvasGO.transform);
        var rightImg = rightBorderGO.AddComponent<UnityEngine.UI.Image>();
        rightImg.color = Color.white;
        var rightRT = rightImg.GetComponent<RectTransform>();
        rightRT.anchorMin = new Vector2(1f, 0f);
        rightRT.anchorMax = new Vector2(1f, 1f);
        rightRT.pivot = new Vector2(1f, 0.5f);
        rightRT.anchoredPosition = Vector2.zero;
        rightRT.sizeDelta = new Vector2(6f, 0f);

        // Winner texts (hidden initially)
        var winnerLeftGO = new GameObject("WinnerLeftText");
        winnerLeftGO.transform.SetParent(canvasGO.transform);
        var winnerLeft = winnerLeftGO.AddComponent<Text>();
        winnerLeft.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winnerLeft.text = "";
        winnerLeft.alignment = TextAnchor.MiddleCenter;
        winnerLeft.fontSize = 64;
        winnerLeft.color = Color.yellow;
        winnerLeft.gameObject.SetActive(false);
        var wlRT = winnerLeft.GetComponent<RectTransform>();
        wlRT.anchorMin = new Vector2(0.25f, 0.6f);
        wlRT.anchorMax = wlRT.anchorMin;
        wlRT.anchoredPosition = Vector2.zero;
        wlRT.sizeDelta = new Vector2(300, 120);

        var winnerRightGO = new GameObject("WinnerRightText");
        winnerRightGO.transform.SetParent(canvasGO.transform);
        var winnerRight = winnerRightGO.AddComponent<Text>();
        winnerRight.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winnerRight.text = "";
        winnerRight.alignment = TextAnchor.MiddleCenter;
        winnerRight.fontSize = 64;
        winnerRight.color = Color.yellow;
        winnerRight.gameObject.SetActive(false);
        var wrRT = winnerRight.GetComponent<RectTransform>();
        wrRT.anchorMin = new Vector2(0.75f, 0.6f);
        wrRT.anchorMax = wrRT.anchorMin;
        wrRT.anchoredPosition = Vector2.zero;
        wrRT.sizeDelta = new Vector2(300, 120);

        // Play again prompts
        var playLeftGO = new GameObject("PlayAgainLeftText");
        playLeftGO.transform.SetParent(canvasGO.transform);
        var playLeft = playLeftGO.AddComponent<Text>();
        playLeft.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playLeft.text = "";
        playLeft.alignment = TextAnchor.MiddleCenter;
        playLeft.fontSize = 22;
        playLeft.color = Color.white;
        playLeft.gameObject.SetActive(false);
        var plRT = playLeft.GetComponent<RectTransform>();
        plRT.anchorMin = new Vector2(0.25f, 0.45f);
        plRT.anchorMax = plRT.anchorMin;
        plRT.anchoredPosition = Vector2.zero;
        plRT.sizeDelta = new Vector2(200, 40);

        var playRightGO = new GameObject("PlayAgainRightText");
        playRightGO.transform.SetParent(canvasGO.transform);
        var playRight = playRightGO.AddComponent<Text>();
        playRight.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playRight.text = "";
        playRight.alignment = TextAnchor.MiddleCenter;
        playRight.fontSize = 22;
        playRight.color = Color.white;
        playRight.gameObject.SetActive(false);
        var prRT = playRight.GetComponent<RectTransform>();
        prRT.anchorMin = new Vector2(0.75f, 0.45f);
        prRT.anchorMax = prRT.anchorMin;
        prRT.anchoredPosition = Vector2.zero;
        prRT.sizeDelta = new Vector2(200, 40);

        // Assign to UIManager
        uiManager.winnerLeftText = winnerLeft;
        uiManager.winnerRightText = winnerRight;
        uiManager.playAgainLeftText = playLeft;
        uiManager.playAgainRightText = playRight;

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
