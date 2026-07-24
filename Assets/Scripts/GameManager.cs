using UnityEngine;

public enum gameState {Running, Paused, Ended}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private gameState state;


    [Header("Stats:")]  
    [SerializeField] private int energyCrystals;
    [SerializeField] private int beaconHealth;


    [Header("Additional components:")]
    [SerializeField] private GameObject UI_EndingScreen;
    


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetEnergyCrystals()
    {
        return energyCrystals;
    }

    public int GetBeaconHealth()
    {
        return beaconHealth;
    }
    
}
