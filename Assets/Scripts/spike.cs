using UnityEngine;

public class spike : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit spike!");
            Debug.Log("Game Over!");
        }
    }
}
