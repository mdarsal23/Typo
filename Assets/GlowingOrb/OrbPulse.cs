using UnityEngine;

public class OrbPulse : MonoBehaviour
{
    public Material orbMaterial;
    public float speed = 2f;
    public float minIntensity = 2f;
    public float maxIntensity = 8f;

    void Update()
    {
        float emission = Mathf.Lerp(minIntensity, maxIntensity, 
                        (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        orbMaterial.SetColor("_EmissionColor", Color.cyan * emission);
        // transform.position += Vector3.up * Mathf.Sin(Time.time) * 0.001f;
    }
}