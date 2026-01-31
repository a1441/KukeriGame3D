using Unity.VisualScripting;
using UnityEngine;

public class DetectionRange : MonoBehaviour
{
    private GameObject player;
    private LightController lightController;
    [SerializeField] private GameObject healthBar;

    [SerializeField] private float detectionRadius = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        lightController = player.GetComponentInChildren<LightController>();
    }

    // Update is called once per frame
    void Update()
    {
        detectionRadius = lightController.range;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= detectionRadius)
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                rend.enabled = true;  // Показва обекта, ако състоянието на играча е "WithoutMask"
            }

            healthBar.GetComponent<WorldHealthBarUI>().ShowHealthBar();
        }
        else
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                rend.enabled = false;  // Показва обекта, ако състоянието на играча е "WithoutMask"
            }

            healthBar.GetComponent<WorldHealthBarUI>().HideHealthBar();
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
