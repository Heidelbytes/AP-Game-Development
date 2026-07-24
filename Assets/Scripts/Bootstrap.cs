using UnityEngine;

public class Bootstrap : MonoBehaviour
{
  [SerializeField] private GameObject gameManagerPrefab;

  private void Awake()
  {
    if (GameManager.Instance == null)
      {
        Instantiate(gameManagerPrefab);
      }
  }
}
