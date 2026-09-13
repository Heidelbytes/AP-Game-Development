using UnityEngine;

public class Bootstrap : MonoBehaviour
{
  [SerializeField] private GameObject gameManagerPrefab;
  [SerializeField] private GameObject waveManagerPrefab;

  private void Awake()
  {
    if (GameManager.Instance == null)
      {
        Instantiate(gameManagerPrefab);
      }

    if (WaveManager.Instance == null)
      {
        Instantiate(waveManagerPrefab);
      }
  }
}
