using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    public float maxSpeed = 4f;
    public float predictionTime = 1f;
    public float avoidanceStrength = 2f;
    public float whiskerAngle = 25f;
    public float frontWhiskerLength = 4f;
    public float sideWhiskerLength = 2f;
    public float separationRadius = 1.2f; // ป้องกันการชนกันเอง

    private Vector2 velocity;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 pursueForce = Pursue(player.position);
        Vector2 avoidanceForce = Avoidance();
        Vector2 separationForce = SeparateFromOthers();

        Debug.Log($"Avoidance Force: {avoidanceForce}");
        Debug.Log($"Velocity Before: {velocity}");

        velocity += (pursueForce + avoidanceForce + separationForce) * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += (Vector3)velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    Vector2 Pursue(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        float distance = direction.magnitude;
        float speed = velocity.magnitude;
        float prediction = (speed > 0) ? distance / speed : 0;
        prediction = Mathf.Min(prediction, predictionTime);
        Vector2 futurePosition = targetPosition + (Vector2)player.GetComponent<Rigidbody2D>().velocity * prediction;
        return (futurePosition - (Vector2)transform.position).normalized * maxSpeed;
    }

    Vector2 Avoidance()
    {
        Vector2 forward = velocity.normalized;
        Vector2 avoidanceForce = Vector2.zero;
        Vector2 rayStart = (Vector2)transform.position; // ลองใช้จุดเริ่มต้นที่เป๊ะขึ้น

        int layerMask = LayerMask.GetMask("Asteroid");
        RaycastHit2D hitFront = Physics2D.Raycast(rayStart, forward, frontWhiskerLength, layerMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart, Quaternion.Euler(0, 0, whiskerAngle) * forward, sideWhiskerLength, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart, Quaternion.Euler(0, 0, -whiskerAngle) * forward, sideWhiskerLength, layerMask);

        bool isBlockedFront = hitFront.collider != null;
        bool isBlockedLeft = hitLeft.collider != null;
        bool isBlockedRight = hitRight.collider != null;

        if (isBlockedFront)
        {
            Debug.Log("Front is block");
            if (!isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward);
            else if (!isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward);
            else avoidanceForce -= forward;
        }

        if (isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward);
        Debug.Log("Left is block");
        if (isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward);
        Debug.Log("Right is block");

        return avoidanceForce.normalized * (avoidanceStrength * 8);
    }

    Vector2 SeparateFromOthers()
    {
        Vector2 forward = velocity.normalized;
        Vector2 avoidanceForce = Vector2.zero;
        Vector2 rayStart = (Vector2)transform.position;

        int layerMask = LayerMask.GetMask("Enemy");
        RaycastHit2D hitFront = Physics2D.Raycast(rayStart, forward, frontWhiskerLength * 0.75f, layerMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart, Quaternion.Euler(0, 0, whiskerAngle) * forward, sideWhiskerLength * 0.75f, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart, Quaternion.Euler(0, 0, -whiskerAngle) * forward, sideWhiskerLength * 0.75f, layerMask);

        bool isBlockedFront = hitFront.collider != null;
        bool isBlockedLeft = hitLeft.collider != null;
        bool isBlockedRight = hitRight.collider != null;

        // ลดแรงหลบออกจากกันเพื่อให้ยังขยับได้
        float separationFactor = avoidanceStrength * 0.5f;

        if (isBlockedFront)
        {
            if (!isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward) * separationFactor;
            else if (!isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward) * separationFactor;
            else avoidanceForce -= forward * separationFactor * 0.5f; // ลดการดันถอยหลัง
        }

        if (isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward) * separationFactor;
        if (isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward) * separationFactor;

        return avoidanceForce; // ไม่ normalize ให้แรงยังมีน้ำหนัก
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return; // วาดเฉพาะตอนเล่นเกม

        Gizmos.color = Color.red;
        Vector2 forward = velocity.normalized;
        Vector2 rayStart = (Vector2)transform.position + forward * 0.5f;

        // Raycast ด้านหน้า
        Gizmos.DrawLine(rayStart, rayStart + forward * frontWhiskerLength);

        // Raycast ด้านซ้าย
        Gizmos.DrawLine(rayStart, rayStart + (Vector2)(Quaternion.Euler(0, 0, whiskerAngle) * forward) * sideWhiskerLength);

        // Raycast ด้านขวา
        Gizmos.DrawLine(rayStart, rayStart + (Vector2)(Quaternion.Euler(0, 0, -whiskerAngle) * forward) * sideWhiskerLength);

        Gizmos.color = Color.blue; // ใช้สีน้ำเงินสำหรับ Raycast ศัตรู

        Gizmos.DrawLine(rayStart, rayStart + forward * (frontWhiskerLength * 0.5f));
        Gizmos.DrawLine(rayStart, rayStart + (Vector2)(Quaternion.Euler(0, 0, whiskerAngle) * forward) * (sideWhiskerLength * 0.5f));
        Gizmos.DrawLine(rayStart, rayStart + (Vector2)(Quaternion.Euler(0, 0, -whiskerAngle) * forward) * (sideWhiskerLength * 0.5f));
    }
}
