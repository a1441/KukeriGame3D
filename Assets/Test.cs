using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform player;      // Слагаш тук референция към играча
    public float visibleRange = 3;  // Рейндж на видимост

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= visibleRange)
        {
            rend.enabled = true;   // Прави обекта видим
        }
        else
        {
            rend.enabled = false;  // Скрива обекта
        }
    }
}
