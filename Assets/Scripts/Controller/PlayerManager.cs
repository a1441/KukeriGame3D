using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton

    public static PlayerManager instance;

    void Awake()
    {
        instance = this;
    }


    #endregion

    public GameObject player;

    public void onPlayerDeath()
    {
        Debug.Log("Player Died");
    }

}
