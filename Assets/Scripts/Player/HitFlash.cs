using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.08f;

    [Tooltip("How strong the flash is. 0 = no change, 1 = full flash color")]
    [SerializeField, Range(0f, 1f)] private float flashAmount = 0.75f;

    [Header("Flash Colors")]
    [SerializeField] private Color flashColor = Color.white;   // set to red or white in Inspector

    [Header("Renderers (leave empty = auto find children)")]
    [SerializeField] private Renderer[] renderers;

    private Color[][] _originalColors;
    private Coroutine _flashRoutine;

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        CacheOriginalColors();
    }

    void CacheOriginalColors()
    {
        _originalColors = new Color[renderers.Length][];

        for (int r = 0; r < renderers.Length; r++)
        {
            var mats = renderers[r].materials; // per-instance material copies
            _originalColors[r] = new Color[mats.Length];

            for (int m = 0; m < mats.Length; m++)
                _originalColors[r][m] = GetMatColor(mats[m]);
        }
    }

    Color GetMatColor(Material mat)
    {
        if (mat.HasProperty(BaseColorId)) return mat.GetColor(BaseColorId);
        if (mat.HasProperty(ColorId)) return mat.GetColor(ColorId);
        return Color.white;
    }

    void SetMatColor(Material mat, Color c)
    {
        if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, c);
        else if (mat.HasProperty(ColorId)) mat.SetColor(ColorId, c);
    }

    /// <summary>Default flash using the Inspector color.</summary>
    public void Flash()
    {
        Flash(flashColor);
    }

    /// <summary>Flash with a custom color (ex: red on damage, white on parry).</summary>
    public void Flash(Color color)
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine(color));
    }

    IEnumerator FlashRoutine(Color color)
    {
        // Apply flash
        for (int r = 0; r < renderers.Length; r++)
        {
            if (!renderers[r]) continue;

            var mats = renderers[r].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Color original = _originalColors[r][m];
                Color flashed = Color.Lerp(original, color, flashAmount);
                SetMatColor(mats[m], flashed);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Revert
        for (int r = 0; r < renderers.Length; r++)
        {
            if (!renderers[r]) continue;

            var mats = renderers[r].materials;
            for (int m = 0; m < mats.Length; m++)
                SetMatColor(mats[m], _originalColors[r][m]);
        }

        _flashRoutine = null;
    }
}
