using UnityEngine;

public class Interactable : MonoBehaviour
{

    public float radius = 1.5f;

    public virtual void Interact()
    {
        // This method is meant to be overridden
        Debug.Log("Interacted with " + transform.name);
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }   
}
