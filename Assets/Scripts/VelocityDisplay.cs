using UnityEngine;
using TMPro;

public class VelocityDisplay : MonoBehaviour
{
    public Rigidbody2D rb;
    public TextMeshProUGUI velocityText;

    void Update()
    {
        float vx = rb.linearVelocity.x;
        float vy = rb.linearVelocity.y;
        float speed = rb.linearVelocity.magnitude;

        velocityText.text =
            $"VX: {vx:F2}\n" +
            $"VY: {vy:F2}\n" +
            $"Speed: {speed:F2}";
    }
}
