using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcTagCountUI : MonoBehaviour
{
    [Header("Counter")]
    [SerializeField] private string npcTag = "NPC";
    [SerializeField] private TMP_Text label;

    [Header("End Screens (drag the root objects here)")]
    [SerializeField] private GameObject winScreenObject;
    [SerializeField] private GameObject loseScreenObject;

    [Header("Tutorial Screens (drag 4 root objects here, order matters)")]
    [SerializeField] private GameObject[] tutorialScreenObjects = new GameObject[4];
    [SerializeField] private bool startTutorialOnEnable = true;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.6f;

    private CanvasGroup _winGroup;
    private CanvasGroup _loseGroup;

    private CanvasGroup[] _tutorialGroups;
    private int _tutorialIndex = -1;
    private bool _tutorialActive;

    private int _remaining;
    private bool _ended;

    private Coroutine _winFadeRoutine;
    private Coroutine _loseFadeRoutine;
    private Coroutine _tutorialFadeRoutine;

    private void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();

        _winGroup = EnsureCanvasGroup(winScreenObject);
        _loseGroup = EnsureCanvasGroup(loseScreenObject);

        InitGroup(_winGroup, false);
        InitGroup(_loseGroup, false);

        // Tutorial groups
        _tutorialGroups = new CanvasGroup[tutorialScreenObjects.Length];
        for (int i = 0; i < tutorialScreenObjects.Length; i++)
        {
            _tutorialGroups[i] = EnsureCanvasGroup(tutorialScreenObjects[i]);
            InitGroup(_tutorialGroups[i], false);
        }
    }

    private void OnEnable()
    {
        // Count ONCE at start
        _remaining = SafeCountByTag(npcTag);
        UpdateLabel();

        // Subscribe
        EnemyDeathHandler.OnAnyNpcDied += OnNpcDied;
        PlayerDeathHandler.OnPlayerDied += OnPlayerDied;

        // Start tutorial (pauses the game)
        if (startTutorialOnEnable && tutorialScreenObjects != null && tutorialScreenObjects.Length > 0)
        {
            StartTutorial();
        }
        else
        {
            // Win immediately if there are 0 enemies at start (only if not in tutorial)
            if (!_ended && _remaining == 0)
            {
                _ended = true;
                ShowWin();
            }
        }
    }

    private void OnDisable()
    {
        EnemyDeathHandler.OnAnyNpcDied -= OnNpcDied;
        PlayerDeathHandler.OnPlayerDied -= OnPlayerDied;
    }

    // ---------------- Tutorial ----------------

    private void StartTutorial()
    {
        _tutorialActive = true;
        _tutorialIndex = 0;

        // Pause gameplay during tutorial
        Time.timeScale = 0f;

        // Show first tutorial screen
        for (int i = 0; i < _tutorialGroups.Length; i++)
            InitGroup(_tutorialGroups[i], false);

        FadeIn(_tutorialGroups[_tutorialIndex], ref _tutorialFadeRoutine);
    }

    private void AdvanceTutorial()
    {
        if (!_tutorialActive) return;

        var current = (_tutorialIndex >= 0 && _tutorialIndex < _tutorialGroups.Length)
            ? _tutorialGroups[_tutorialIndex]
            : null;

        int nextIndex = _tutorialIndex + 1;

        // Fade out current
        if (current) FadeOut(current, ref _tutorialFadeRoutine);

        // If no more screens -> end tutorial
        if (nextIndex >= _tutorialGroups.Length)
        {
            EndTutorial();
            return;
        }

        // Fade in next
        _tutorialIndex = nextIndex;
        var next = _tutorialGroups[_tutorialIndex];
        FadeIn(next, ref _tutorialFadeRoutine);
    }

    private void EndTutorial()
    {
        _tutorialActive = false;
        _tutorialIndex = -1;

        // Hide all tutorial screens (safety)
        for (int i = 0; i < _tutorialGroups.Length; i++)
            InitGroup(_tutorialGroups[i], false);

        // Resume gameplay if not ended
        if (!_ended) Time.timeScale = 1f;

        // Now that tutorial is gone, allow instant win check
        if (!_ended && _remaining == 0)
        {
            _ended = true;
            ShowWin();
        }
    }

    // ---------------- Events ----------------

    private void OnNpcDied()
    {
        if (_ended) return;
        if (_tutorialActive) return; // prevent win popping under tutorial

        _remaining = Mathf.Max(0, _remaining - 1);
        UpdateLabel();

        if (_remaining == 0)
        {
            _ended = true;
            ShowWin();
        }
    }

    private void OnPlayerDied()
    {
        if (_ended) return;
        if (_tutorialActive) return; // prevent lose popping under tutorial

        _ended = true;
        ShowLose();
    }

    private void UpdateLabel()
    {
        if (label) label.text = $"Spirits Remaining: {_remaining}";
    }

    // Optional manual resync if you ever need it (spawns/pooling)
    public void Recount()
    {
        if (_ended) return;
        if (_tutorialActive) return;

        _remaining = SafeCountByTag(npcTag);
        UpdateLabel();

        if (_remaining == 0)
        {
            _ended = true;
            ShowWin();
        }
    }

    // ---------------- WIN / LOSE ----------------

    private void ShowWin()
    {
        FadeIn(_winGroup, ref _winFadeRoutine);
        FadeOut(_loseGroup, ref _loseFadeRoutine);
    }

    private void ShowLose()
    {
        FadeIn(_loseGroup, ref _loseFadeRoutine);
        FadeOut(_winGroup, ref _winFadeRoutine);
    }

    // ---------------- Helpers ----------------

    private int SafeCountByTag(string tag)
    {
        try { return GameObject.FindGameObjectsWithTag(tag).Length; }
        catch { return 0; }
    }

    private CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        if (!go) return null;

        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();

        // Keep object active so it can fade; CanvasGroup controls visibility.
        go.SetActive(true);
        return cg;
    }

    private void InitGroup(CanvasGroup g, bool visible)
    {
        if (!g) return;
        g.alpha = visible ? 1f : 0f;
        g.interactable = visible;
        g.blocksRaycasts = visible;
    }

    private void FadeIn(CanvasGroup g, ref Coroutine routine)
    {
        if (!g) return;
        StartFade(g, 1f, true, ref routine);
    }

    private void FadeOut(CanvasGroup g, ref Coroutine routine)
    {
        if (!g) return;
        StartFade(g, 0f, false, ref routine);
    }

    private void StartFade(CanvasGroup g, float targetAlpha, bool interactive, ref Coroutine routine)
    {
        // NOTE: we do NOT force Time.timeScale here anymore.
        // Tutorial controls pause; Win/Lose also uses unscaled time so it can animate while paused.
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeRoutine(g, targetAlpha, interactive));
    }

    private IEnumerator FadeRoutine(CanvasGroup g, float targetAlpha, bool interactive)
    {
        float startAlpha = g.alpha;
        float dur = Mathf.Max(0.01f, fadeDuration);
        float t = 0f;

        if (targetAlpha > 0.001f)
        {
            g.blocksRaycasts = true;
            g.interactable = true;
        }

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dur);
            g.alpha = Mathf.Lerp(startAlpha, targetAlpha, p);
            yield return null;
        }

        g.alpha = targetAlpha;

        if (targetAlpha <= 0.001f)
        {
            g.blocksRaycasts = false;
            g.interactable = false;
        }
        else
        {
            g.blocksRaycasts = interactive;
            g.interactable = interactive;
        }
    }

    public void ReloadScene()
    {
        if (_winFadeRoutine != null) StopCoroutine(_winFadeRoutine);
        if (_loseFadeRoutine != null) StopCoroutine(_loseFadeRoutine);
        if (_tutorialFadeRoutine != null) StopCoroutine(_tutorialFadeRoutine);

        Time.timeScale = 1f;
        Debug.Log("[NpcTagCountUI] Reloading scene...");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        // Tutorial click to advance
        if (_tutorialActive && Input.GetMouseButtonDown(0))
        {
            AdvanceTutorial();
            return;
        }

        // Existing escape reload after end
        if (_ended && Input.GetKeyDown(KeyCode.Escape))
            ReloadScene();
    }
}
