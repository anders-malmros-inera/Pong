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

        // Helper sprite (builtin UI sprite)
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Left Paddle
        var paddleLeft = new GameObject("PaddleLeft");
        var srL = paddleLeft.AddComponent<SpriteRenderer>();
        srL.sprite = sprite;
        srL.color = Color.white;
        paddleLeft.transform.position = new Vector3(-8f, 0f, 0f);
        paddleLeft.transform.localScale = new Vector3(0.5f, 2f, 1f);
        paddleLeft.AddComponent<BoxCollider2D>();
        var rbL = paddleLeft.AddComponent<Rigidbody2D>();
        rbL.bodyType = RigidbodyType2D.Kinematic;
        rbL.freezeRotation = true;
        var paddleScriptL = paddleLeft.AddComponent<Paddle>();
        paddleScriptL.isLeft = true;
        EnsureTagExists("Paddle");
        paddleLeft.tag = "Paddle";

        // Right Paddle
        var paddleRight = new GameObject("PaddleRight");
        var srR = paddleRight.AddComponent<SpriteRenderer>();
        srR.sprite = sprite;
        srR.color = Color.white;
        paddleRight.transform.position = new Vector3(8f, 0f, 0f);
        paddleRight.transform.localScale = new Vector3(0.5f, 2f, 1f);
        paddleRight.AddComponent<BoxCollider2D>();
        var rbR = paddleRight.AddComponent<Rigidbody2D>();
        rbR.bodyType = RigidbodyType2D.Kinematic;
        rbR.freezeRotation = true;
        var paddleScriptR = paddleRight.AddComponent<Paddle>();
        paddleScriptR.isLeft = false;
        paddleRight.tag = "Paddle";

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

        // UIManager
        var uiGO = new GameObject("UIManager");
        var uiManager = uiGO.AddComponent<UIManager>();
        uiManager.leftScoreText = leftText;
        uiManager.rightScoreText = rightText;

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
