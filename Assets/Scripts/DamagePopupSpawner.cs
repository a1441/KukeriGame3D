// DamagePopupSpawner.cs
using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private Camera worldCamera;

    void Awake()
    {
        if (!worldCamera) worldCamera = Camera.main;
    }

    public void Spawn(float damage, Vector3 worldPos)
    {
        if (!popupPrefab || !worldCamera) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return; // behind camera

        DamagePopup popup = Instantiate(popupPrefab, transform);
        popup.transform.position = screenPos;
        popup.Init(damage);
    }
}
