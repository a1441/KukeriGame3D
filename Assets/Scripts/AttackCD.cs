using UnityEngine;
using UnityEngine.UI;

public class AttackCooldownBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private ShockwaveAttack attack;   // auto-found if null
    [SerializeField] private bool hideWhenReady = true;
    [SerializeField] private GameObject visualRoot;    // optional (CooldownBar object)

    void Awake()
    {
        if (!fill)
            fill = GetComponentInChildren<Image>(true);

        // If the cooldown bar is parented under the player, this finds it.
        if (!attack)
            attack = GetComponentInParent<ShockwaveAttack>();

        if (!visualRoot)
            visualRoot = gameObject;
    }

    void Update()
    {
        if (!fill || !attack) return;

        float remaining01 = attack.CooldownRemaining01; // 1 -> 0
        fill.fillAmount = remaining01;

 
    }
}
