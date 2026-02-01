using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LightController : MonoBehaviour
{
    [SerializeField] private Light playerLight;
    [SerializeField] private float increaseIntenseAmount = 100f;
    [SerializeField] private float increaseRange = 100f;
    [SerializeField] private GameObject directionalLight; 

    private float originalLightRange;
    private float originalIntensityAmount;
    private Coroutine rangeCoroutine;

    [SerializeField] private float duration = 2f;
    [SerializeField] private RangedEnemyController[] rangedEnemies; 

    public float visibleRange = 10f;
    public float range;
    public LayerMask objectLayer;
    public enum State
    {
        WithMask,
        WithoutMask
    }

    public State currentState;

    private bool firstTime = false;

    [SerializeField] private EnemyConnector enemyConnector;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalLightRange = playerLight.range;
        range = playerLight.range;
        originalIntensityAmount = playerLight.intensity;
        currentState = State.WithMask;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ChangeRangeSmooth();
        }
    }

    private void ChangeRangeSmooh()
    {
        StopAllCoroutines();

        if (currentState == State.WithMask)
        {
            enemyConnector.ConnectOnMask();
            StartCoroutine(SmoothRangeReturn());
            currentState = State.WithoutMask;
            return;
        }
        else
        {
            enemyConnector.ConnectWithoutMask();
            StartCoroutine(SmoothRangeChange());
            currentState = State.WithMask;
            return;
        }
    }

    private void ChangeRangeSmooth()
    {
        if (rangeCoroutine != null)
            StopCoroutine(rangeCoroutine);

        if (currentState == State.WithMask)
        {
            currentState = State.WithoutMask;

            rangeCoroutine = StartCoroutine(SmoothRangeReturn());
        }
        else
        {
            currentState = State.WithMask;

            rangeCoroutine = StartCoroutine(SmoothRangeChange());
        }
    }

    private IEnumerator SmoothRangeChange()
    {
        float startRange = playerLight.range;
        float startIntensity = playerLight.intensity;
        float time = 0f;

        foreach (var ranged in rangedEnemies)
        {
            ranged.ResetShootRadius(); // ⬅️ ВАЖНО
        }

        range = increaseRange;

        while (time < duration)
        {
            time += Time.deltaTime;
            Debug.Log(time);
            playerLight.intensity = Mathf.Lerp(startIntensity, increaseIntenseAmount, time / duration);
            playerLight.range = Mathf.Lerp(startRange, increaseRange, time / duration);
            yield return null;
        }

        playerLight.intensity = increaseIntenseAmount;
        playerLight.range = increaseRange;
    }

    private IEnumerator SmoothRangeReturn()
    {
        float startRange = playerLight.range;
        float startIntensity = playerLight.intensity;
        float time = 0f;

        foreach (var ranged in rangedEnemies)
        {
            ranged.IncreaseShootRadius();
        }

        range = originalLightRange;

        while (time < duration)
        {
            time += Time.deltaTime;
            playerLight.intensity = Mathf.Lerp(startIntensity, originalIntensityAmount, time / 0.5f);
            playerLight.range = Mathf.Lerp(startRange, originalLightRange, time / 0.5f);
            range = playerLight.range;
            yield return null;
        }

        playerLight.intensity = originalIntensityAmount;
        playerLight.range = originalLightRange;
    }
}
