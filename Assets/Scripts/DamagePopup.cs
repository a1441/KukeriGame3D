// DamagePopup.cs
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 0.60f; // shorter = less travel

    [Header("Motion (pixels)")]
    [SerializeField] private float minFloatSpeed = 40f; // smaller = less travel
    [SerializeField] private float maxFloatSpeed = 70f;

    [SerializeField] private float spawnRandomX = 20f;  // smaller = tighter spawn
    [SerializeField] private float spawnRandomY = 8f;

    [SerializeField] private float driftRandomX = 20f;  // smaller = less drift
    [SerializeField] private float driftRandomY = 10f;

    [Header("Rotation (juice)")]
    [SerializeField] private float maxRotZ = 6f;

    [Header("Pop (juice)")]
    [SerializeField] private bool popScale = true;
    [SerializeField] private float popMultiplier = 1.12f;
    [SerializeField] private float popTime = 0.07f;

    [Header("Wobble (juice)")]
    [SerializeField] private bool wobble = true;
    [SerializeField] private float wobbleAmplitude = 4f;   // smaller = tighter
    [SerializeField] private float wobbleFrequency = 10f;

    float _t;
    Vector3 _startPos;
    Vector3 _drift;
    float _floatSpeed;
    Vector3 _baseScale;

    float _wobbleSeed;

    public void Init(float damage)
    {
        if (!text) text = GetComponent<TextMeshProUGUI>();

        text.text = Mathf.RoundToInt(damage).ToString();

        _t = 0f;
        _baseScale = transform.localScale;

        // Randomize start position (tight)
        Vector3 startOffset = new Vector3(
            Random.Range(-spawnRandomX, spawnRandomX),
            Random.Range(-spawnRandomY, spawnRandomY),
            0f
        );

        _startPos = transform.position + startOffset;

        // Random drift (tight)
        _drift = new Vector3(
            Random.Range(-driftRandomX, driftRandomX),
            Random.Range(0f, driftRandomY),
            0f
        );

        _floatSpeed = Random.Range(minFloatSpeed, maxFloatSpeed);
        _wobbleSeed = Random.Range(0f, 1000f);

        // Random small rotation
        float rotZ = Random.Range(-maxRotZ, maxRotZ);
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

        transform.position = _startPos;

        // Pop scale at start
        transform.localScale = popScale ? (_baseScale * popMultiplier) : _baseScale;

        // Full alpha at start
        var c = text.color;
        c.a = 1f;
        text.color = c;
    }

    void Update()
    {
        _t += Time.deltaTime;
        float p = Mathf.Clamp01(_t / Mathf.Max(0.0001f, lifeTime));

        // Ease-out
        float smooth = 1f - Mathf.Pow(1f - p, 2f);

        Vector3 pos = _startPos
                    + Vector3.up * (_floatSpeed * _t)
                    + _drift * smooth;

        if (wobble)
        {
            float wob = Mathf.Sin((_t + _wobbleSeed) * (Mathf.PI * 2f) * wobbleFrequency);
            pos += new Vector3(wob * wobbleAmplitude, 0f, 0f);
        }

        transform.position = pos;

        // Fade
        if (text)
        {
            var c = text.color;
            c.a = 1f - p;
            text.color = c;
        }

        // Pop scale back down
        if (popScale)
        {
            float sp = (popTime <= 0.0001f) ? 1f : Mathf.Clamp01(_t / popTime);
            transform.localScale = Vector3.Lerp(_baseScale * popMultiplier, _baseScale, sp);
        }

        if (_t >= lifeTime)
            Destroy(gameObject);
    }
}
