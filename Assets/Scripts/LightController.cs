using System.Collections;
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

    [SerializeField] private float duration = 2f;

    public float visibleRange = 10f;
    public float range;
    public LayerMask objectLayer;
    public enum State
    {
        WithMask,
        WithoutMask
    }

    public State currentState;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalLightRange = playerLight.range;
        range = playerLight.range;
        originalIntensityAmount = playerLight.intensity;
        currentState = State.WithMask;
    }

    private void Update()
    {
        IncreaseLight();
    }

    private void IncreaseLight()
    {
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    if (currentState == State.WithMask)
        //    {
                
        //        range = playerLight.range;
        //        currentState = State.WithoutMask;

        //    }
        //    else
        //    {
        //        playerLight.range = originalLightRange;
        //        playerLight.intensity = originalIntensityAmount;
        //        range = playerLight.range;
        //        currentState = State.WithMask;
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.L))
        {
            ChangeRangeSmooth();
        }
    }

    private void ChangeRangeSmooth()
    {
        StopAllCoroutines();

        if (currentState == State.WithMask)
        {
            StartCoroutine(SmoothRangeReturn());
            currentState = State.WithoutMask;
            return;
        }
        else
        {
            StartCoroutine(SmoothRangeChange());
            currentState = State.WithMask;
            return;
        }
    }

    private IEnumerator SmoothRangeChange()
    {
        float startRange = playerLight.range;
        float startIntensity = playerLight.intensity;
        float time = 0f;

        range = increaseRange;

        while (time < duration)
        {
            time += Time.deltaTime;
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

        //range = originalLightRange;

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
