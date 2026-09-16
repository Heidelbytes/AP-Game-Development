using UnityEngine;

public enum gameState {Running, Paused, Ended}
public enum gamePhase {DefensivePhase, BuildingPhase}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private gameState state;
    private gamePhase phase;
    private int waveCount;


    [Header("Stats:")]  
    [SerializeField] public int energyCrystals;
    [SerializeField] public int beaconHealth;


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

    private void Start() {
        WaveManager.Instance.StartWave(1);
        WaveManager.Instance.StartWave(0);
    }


    public int GetEnergyCrystals()
    {
        return energyCrystals;
    }

    public int GetBeaconHealth()
    {
        return beaconHealth;
    }
    
    public gamePhase getGamePhase() 
    {
        return phase;
    }

    public void switchToDefensivePhase() 
    {
        phase = gamePhase.DefensivePhase;
    }

    public void switchToBuidlingPhase() 
    {
        phase = gamePhase.BuildingPhase;
    }
};
