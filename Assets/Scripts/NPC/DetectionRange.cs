using UnityEngine;

public class DetectionRange : MonoBehaviour
{
    private GameObject player;
    private Renderer rend;
    private LightController lightController;

    [SerializeField] private float detectionRadius = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rend = GetComponent<Renderer>();
        lightController = player.GetComponentInChildren<LightController>();
    }

    // Update is called once per frame
    void Update()
    {
        detectionRadius = lightController.range;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= detectionRadius)
        {
            rend.enabled = true;   // Прави обекта видим
        }
        else
        {
            rend.enabled = false;  // Скрива обекта
        }
    }

    private bool CheckPlayerState()
    {
        if (lightController != null && lightController.currentState == LightController.State.WithoutMask)
        {
            return true;
        }
        return false;
    }
}
