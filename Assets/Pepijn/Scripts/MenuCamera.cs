using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    public float sweepAngle = 45f;    // Max left/right angle from center
    public float sweepSpeed = 1f;     // Sweep speed

    private float baseYRotation;
    Quaternion startRotation;

    void Start()
    {
        // Store the initial Y rotation (global)
        baseYRotation = transform.eulerAngles.y;
        startRotation = transform.rotation;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * sweepSpeed) * sweepAngle;
        float newY = baseYRotation + offset;

        // Apply only Y rotation, keep X and Z the same
        Quaternion desiredRotation = Quaternion.Euler(0f, newY, 0f);

        desiredRotation.x = startRotation.x;
        desiredRotation.z = startRotation.z;

        transform.rotation = desiredRotation;
    }
}
