using System.Collections;
using TMPro;
using UnityEngine;

public class NpcTagCountUI : MonoBehaviour
{
    [Header("Counter")]
    [SerializeField] private string npcTag = "NPC";
    [SerializeField] private TMP_Text label;

    [Header("End Screens (drag the root objects here)")]
    [SerializeField] private GameObject winScreenObject;
    [SerializeField] private GameObject loseScreenObject;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.6f;

    private CanvasGroup _winGroup;
    private CanvasGroup _loseGroup;

    private int _remaining;
    private bool _ended;

    private Coroutine _winFadeRoutine;
    private Coroutine _loseFadeRoutine;

    private void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();

        _winGroup = EnsureCanvasGroup(winScreenObject);
        _loseGroup = EnsureCanvasGroup(loseScreenObject);

        InitGroup(_winGroup, false);
        InitGroup(_loseGroup, false);
    }

    private void OnEnable()
    {
        // Count ONCE at start
        _remaining = SafeCountByTag(npcTag);
        UpdateLabel();

        // Win immediately if there are 0 enemies at start
        if (!_ended && _remaining == 0)
        {
            _ended = true;
            ShowWin();
        }

        EnemyDeathHandler.OnAnyNpcDied += OnNpcDied;
        PlayerDeathHandler.OnPlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        EnemyDeathHandler.OnAnyNpcDied -= OnNpcDied;
        PlayerDeathHandler.OnPlayerDied -= OnPlayerDied;
    }

    private void OnNpcDied()
    {
        if (_ended) return;

        // Decrement ONLY (no recount)
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

        _remaining = SafeCountByTag(npcTag);
        UpdateLabel();

        if (_remaining == 0)
        {
            _ended = true;
            ShowWin();
        }
    }

    // ----- WIN / LOSE -----

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

    // ----- Helpers -----

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
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeRoutine(g, targetAlpha, interactive));
    }

    private IEnumerator FadeRoutine(CanvasGroup g, float targetAlpha, bool interactive)
    {
        float startAlpha = g.alpha;
        float dur = Mathf.Max(0.01f, fadeDuration);
        float t = 0f;

        // If fading in, enable interaction immediately
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

        // If faded out, disable interaction
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
}
