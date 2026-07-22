using UnityEngine;

public class PlayerTrailController : MonoBehaviour
{
    public float minSpeed = 5f;
    public float offSpeed = 4f;

    Rigidbody2D rb;
    TrailRenderer trail;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = false;
    }

    void Update()
    {
        float speed = rb.velocity.magnitude;   // FIXED

        if (speed >= minSpeed && !trail.enabled)
            trail.enabled = true;
        else if (speed <= offSpeed && trail.enabled)
            trail.enabled = false;
    }
}
