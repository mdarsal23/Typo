using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public float speed = 20f;
    public float maxDistance = 100f;
    public int maxBounces = 10;
    public LayerMask collisionMask;

    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentSegment = 0;
    private float segmentProgress = 0f;
    
    [SerializeField] private Transform player;
    [SerializeField] private string catchTag = "Player";
    private ThirdPersonShooterController playerController;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<ThirdPersonShooterController>();
        ComputePath();
    }

    void ComputePath()
    {
        Vector3 position = transform.position;
        Vector3 direction = transform.forward;

        pathPoints.Add(position);

        for(int i = 0; i < maxBounces; i++)
        {
            RaycastHit hit;

            if(Physics.Raycast(position, direction, out hit, maxDistance, collisionMask))
            {
                pathPoints.Add(hit.point);

                if(hit.collider.CompareTag("Enemy"))
                {
                    Destroy(hit.collider.gameObject);
                }

                direction = Vector3.Reflect(direction, hit.normal);
                position = hit.point + direction * 0.01f;
            }
            else
            {
                pathPoints.Add(position + direction * maxDistance);
                break;
            }
        }
    }

    void Update()
    {
        if(currentSegment >= pathPoints.Count - 1)
            return;

        Vector3 start = pathPoints[currentSegment];
        Vector3 end = pathPoints[currentSegment + 1];

        float segmentLength = Vector3.Distance(start, end);

        segmentProgress += speed * Time.deltaTime;

        float t = segmentProgress / segmentLength;

        transform.position = Vector3.Lerp(start, end, t);

        if(t >= 1f)
        {
            segmentProgress = 0f;
            currentSegment++;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(catchTag))
        {
            playerController.setHasBullet();
            Debug.Log("Bullet caught by player!");
            Destroy(gameObject);
        }
        else{
            Debug.Log("Bullet collided with: " + collision.gameObject.name);
        }
    }
}