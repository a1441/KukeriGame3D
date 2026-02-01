using UnityEngine;

public class EnemyConnector : MonoBehaviour
{
    public RangedEnemyController[] rangedEnemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectOnMask()
    {
        foreach (var ranged  in rangedEnemies)
        {
            ranged.IncreaseShootRadius();
        }
    }

    public void ConnectWithoutMask()
    {
        foreach (var ranged in rangedEnemies)
        {
            ranged.ResetShootRadius();
        }
    }
}
