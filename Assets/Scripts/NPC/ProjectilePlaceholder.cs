using UnityEngine;

public class ProjectilePlaceholder : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float damage = 10f;

    private float lifeTimer;

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile hit {other.name} for {damage} damage.");
        Destroy(gameObject);
    }
}
