using UnityEngine;

public enum gameState {Running, Paused, Ended}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int energyCrystals;

    public int beaconHealth;

    public gameState state;

    public GameObject UI_EndingScreen;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Instance.beaconHealth = 500;
        }
        else
        {
            Destroy(gameObject);
        }

        state = gameState.Running;
    }

    void Update()
    {
      if(Instance.beaconHealth <= 0)
      {
        state = gameState.Ended;
        Time.timeScale = 0;
        UI_EndingScreen.SetActive(true);
      }
    }
}  