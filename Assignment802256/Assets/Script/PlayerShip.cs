using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    public float maxSpeed = 6f;
    public float slowRadius = 5f;
    public float targetRadius = 0.5f;
    public float timeToTarget = 0.1f;
    public float rotationSpeed = 5f;

    private Vector2 velocity;
    private Vector2 targetPosition;

    void Update()
    {
        Camera cam = FindObjectOfType<Camera>();
        if (cam != null)
        {
            targetPosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Arrive();
        }
    }

    void Arrive()
    {
        Vector2 position = transform.position;
        Vector2 direction = targetPosition - position;
        float distance = direction.magnitude;

        if (distance < targetRadius)
        {
            velocity = Vector2.zero;
            return;
        }

        float targetSpeed = (distance < slowRadius) ? maxSpeed * (distance / slowRadius) : maxSpeed;
        Vector2 targetVelocity = direction.normalized * targetSpeed;

        Vector2 acceleration = (targetVelocity - velocity) / timeToTarget;
        velocity += acceleration * Time.deltaTime;

        transform.position += (Vector3)velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.deltaTime);
        }
    }
}
