using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BowlingPrototypeBootstrap : MonoBehaviour
{
    private const int PinCount = 10;

    [Header("Ball Motion")]
    [SerializeField] private float lateralSpeed = 8f;
    [SerializeField] private float lateralRange = 2.4f;
    [SerializeField] private float launchSpeed = 18f;
    [SerializeField] private float resetDelay = 3f;

    [Header("Lane")]
    [SerializeField] private float laneLength = 24f;
    [SerializeField] private float laneWidth = 6f;

    private GameObject ball;
    private Rigidbody ballRigidbody;
    private readonly List<Rigidbody> pinBodies = new();

    private Text statusText;
    private Text scoreText;

    private bool inAimMode = true;
    private int totalScore;
    private int frameNumber = 1;
    private float resetTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreatePrototypeIfMissing()
    {
        if (FindFirstObjectByType<BowlingPrototypeBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrap = new("BowlingPrototypeBootstrap");
        bootstrap.AddComponent<BowlingPrototypeBootstrap>();
    }

    private void Start()
    {
        SetupCameraAndLighting();
        BuildLane();
        BuildBall();
        BuildPins();
        BuildUi();
        ResetBallForAiming();
    }

    private void Update()
    {
        if (inAimMode)
        {
            float x = Mathf.PingPong(Time.time * lateralSpeed, lateralRange * 2f) - lateralRange;
            ball.transform.position = new Vector3(x, 0.35f, -laneLength * 0.45f);

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
            {
                LaunchBall();
            }
        }
        else
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                CompleteRollAndReset();
            }
        }
    }

    private void SetupCameraAndLighting()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObject = new("Main Camera");
            mainCam = camObject.AddComponent<Camera>();
            camObject.tag = "MainCamera";
        }

        mainCam.transform.position = new Vector3(0f, 10f, -18f);
        mainCam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        mainCam.backgroundColor = new Color(0.07f, 0.07f, 0.1f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;

        Light directionalLight = FindFirstObjectByType<Light>();
        if (directionalLight == null)
        {
            GameObject lightObj = new("Directional Light");
            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
        }

        directionalLight.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        directionalLight.intensity = 1.2f;
    }

    private void BuildLane()
    {
        GameObject lane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lane.name = "Bowling Lane";
        lane.transform.position = new Vector3(0f, 0f, 0f);
        lane.transform.localScale = new Vector3(laneWidth, 0.2f, laneLength);

        Renderer laneRenderer = lane.GetComponent<Renderer>();
        laneRenderer.material.color = new Color(0.6f, 0.42f, 0.24f);

        PhysicsMaterial physicsMaterial = new("Lane PhysMat")
        {
            dynamicFriction = 0.12f,
            staticFriction = 0.16f,
            bounciness = 0.02f,
            frictionCombine = PhysicMaterialCombine.Average,
            bounceCombine = PhysicMaterialCombine.Minimum
        };
        lane.GetComponent<Collider>().material = physicsMaterial;

        CreateGutter(-laneWidth * 0.56f);
        CreateGutter(laneWidth * 0.56f);
    }

    private void CreateGutter(float xPos)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = xPos < 0 ? "Left Gutter Wall" : "Right Gutter Wall";
        wall.transform.position = new Vector3(xPos, 0.55f, 0f);
        wall.transform.localScale = new Vector3(0.3f, 1f, laneLength);
        wall.GetComponent<Renderer>().material.color = new Color(0.12f, 0.12f, 0.14f);
    }

    private void BuildBall()
    {
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Bowling Ball";
        ball.transform.localScale = Vector3.one * 0.7f;
        ball.GetComponent<Renderer>().material.color = new Color(0.12f, 0.25f, 0.8f);

        ballRigidbody = ball.AddComponent<Rigidbody>();
        ballRigidbody.mass = 7f;
        ballRigidbody.drag = 0.07f;
        ballRigidbody.angularDrag = 0.1f;
        ballRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        PhysicsMaterial ballMaterial = new("Ball PhysMat")
        {
            dynamicFriction = 0.12f,
            staticFriction = 0.1f,
            bounciness = 0.04f
        };
        ball.GetComponent<SphereCollider>().material = ballMaterial;
    }

    private void BuildPins()
    {
        float startZ = laneLength * 0.34f;
        float rowSpacing = 1.1f;
        float colSpacing = 0.65f;

        int pinIndex = 0;
        for (int row = 0; row < 4; row++)
        {
            int pinsInRow = row + 1;
            float rowWidth = (pinsInRow - 1) * colSpacing;

            for (int col = 0; col < pinsInRow; col++)
            {
                float x = -rowWidth * 0.5f + col * colSpacing;
                float z = startZ + row * rowSpacing;
                CreatePin(new Vector3(x, 0.5f, z), ++pinIndex);
            }
        }
    }

    private void CreatePin(Vector3 position, int index)
    {
        GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pin.name = $"Pin {index}";
        pin.transform.position = position;
        pin.transform.localScale = new Vector3(0.4f, 0.55f, 0.4f);
        pin.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);

        Rigidbody pinBody = pin.AddComponent<Rigidbody>();
        pinBody.mass = 1.5f;
        pinBody.drag = 0.25f;
        pinBody.angularDrag = 0.2f;
        pinBody.interpolation = RigidbodyInterpolation.Interpolate;

        pinBodies.Add(pinBody);
    }

    private void BuildUi()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObject = new("Bowling UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        statusText = CreateTextElement(canvas.transform, "Status", new Vector2(0f, -30f), 30, TextAnchor.UpperCenter);
        scoreText = CreateTextElement(canvas.transform, "Score", new Vector2(16f, -16f), 24, TextAnchor.UpperLeft);

        statusText.text = "Click to launch the ball";
        scoreText.text = "Frame 1  |  Score: 0";
    }

    private Text CreateTextElement(Transform parent, string name, Vector2 anchoredPos, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new(name);
        textObject.transform.SetParent(parent);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = alignment == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = alignment == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(700f, 80f);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;

        return text;
    }

    private void LaunchBall()
    {
        inAimMode = false;
        ballRigidbody.isKinematic = false;
        ballRigidbody.linearVelocity = new Vector3(0f, 0f, launchSpeed);
        ballRigidbody.angularVelocity = new Vector3(-2f, 0f, 0f);
        resetTimer = resetDelay;
        statusText.text = "Ball launched...";
    }

    private void CompleteRollAndReset()
    {
        int knockedPins = CountKnockedPins();
        totalScore += knockedPins;
        frameNumber++;

        statusText.text = knockedPins switch
        {
            10 => "Strike! Click to launch again",
            0 => "No pins this roll. Click to retry",
            _ => $"You knocked {knockedPins} pins! Click to launch again"
        };
        scoreText.text = $"Frame {frameNumber}  |  Score: {totalScore}";

        ResetBallForAiming();
        ResetPins();
    }

    private void ResetBallForAiming()
    {
        inAimMode = true;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.isKinematic = true;
        ball.transform.position = new Vector3(0f, 0.35f, -laneLength * 0.45f);
        ball.transform.rotation = Quaternion.identity;
    }

    private int CountKnockedPins()
    {
        int knockedPins = 0;
        foreach (Rigidbody pinBody in pinBodies)
        {
            float upDot = Vector3.Dot(pinBody.transform.up, Vector3.up);
            if (upDot < 0.85f || pinBody.transform.position.y < 0.25f)
            {
                knockedPins++;
            }
        }

        return knockedPins;
    }

    private void ResetPins()
    {
        float startZ = laneLength * 0.34f;
        float rowSpacing = 1.1f;
        float colSpacing = 0.65f;

        int pinPointer = 0;
        for (int row = 0; row < 4; row++)
        {
            int pinsInRow = row + 1;
            float rowWidth = (pinsInRow - 1) * colSpacing;

            for (int col = 0; col < pinsInRow; col++)
            {
                Rigidbody pinBody = pinBodies[pinPointer++];
                float x = -rowWidth * 0.5f + col * colSpacing;
                float z = startZ + row * rowSpacing;
                pinBody.linearVelocity = Vector3.zero;
                pinBody.angularVelocity = Vector3.zero;
                pinBody.transform.position = new Vector3(x, 0.5f, z);
                pinBody.transform.rotation = Quaternion.identity;
            }
        }
    }

    private static bool IsPointerOverUi()
    {
        EventSystem eventSystem = EventSystem.current;
        return eventSystem != null && eventSystem.IsPointerOverGameObject();
    }
}
