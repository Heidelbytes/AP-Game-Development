using UnityEngine;
using TMPro;

public class UI_Stats : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI stats;

    void Update()
    {
        stats.text =
        "Energy Crystals: " + GameManager.Instance.GetEnergyCrystals() 
        + "\nBeacon Health: " + GameManager.Instance.GetBeaconHealth();
    }
}
