using UnityEngine;
using System.Collections;

public class EnemyShip : MonoBehaviour
{
    public float maxSpeed = 4f;
    public float predictionTime = 1f;
    public float avoidanceStrength = 2f;
    public float whiskerAngle = 25f;
    public float frontWhiskerLength = 4f;
    public float sideWhiskerLength = 2f;
    public float separationRadius = 1.2f;

    private Vector2 velocity;
    private Transform player;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;  // Prefab กระสุน
    public Transform firePoint;      // ตำแหน่งที่ยิง
    public float bulletSpeed = 8f;   // ความเร็วกระสุน
    public float fireRate = 1f;      // อัตราการยิง (กระสุนต่อวินาที)
    private float nextFireTime = 0f; // เวลาที่สามารถยิงครั้งต่อไป

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

        velocity += (pursueForce + avoidanceForce + separationForce) * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += (Vector3)velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // ✅ ยิงกระสุนจากตำแหน่งของ firePoint ปัจจุบันของยานลำนั้นๆ
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * bulletSpeed; // ✅ กระสุนเคลื่อนที่ไปข้างหน้า
        }

        Destroy(bullet, 3f); // ✅ ลบกระสุนอัตโนมัติหลัง 3 วินาที
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
        Vector2 rayStart = (Vector2)transform.position;

        int layerMask = LayerMask.GetMask("Asteroid");
        RaycastHit2D hitFront = Physics2D.Raycast(rayStart, forward, frontWhiskerLength, layerMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart, Quaternion.Euler(0, 0, whiskerAngle) * forward, sideWhiskerLength, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart, Quaternion.Euler(0, 0, -whiskerAngle) * forward, sideWhiskerLength, layerMask);

        bool isBlockedFront = hitFront.collider != null;
        bool isBlockedLeft = hitLeft.collider != null;
        bool isBlockedRight = hitRight.collider != null;

        if (isBlockedFront)
        {
            if (!isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward);
            else if (!isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward);
            else avoidanceForce -= forward;
        }

        if (isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward);
        if (isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward);

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

        float separationFactor = avoidanceStrength * 0.5f;

        if (isBlockedFront)
        {
            if (!isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward) * separationFactor;
            else if (!isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward) * separationFactor;
            else avoidanceForce -= forward * separationFactor * 0.5f;
        }

        if (isBlockedLeft) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, -90) * forward) * separationFactor;
        if (isBlockedRight) avoidanceForce += (Vector2)(Quaternion.Euler(0, 0, 90) * forward) * separationFactor;

        return avoidanceForce;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Vector2 forward = velocity.normalized;
        Vector2 rayStart = (Vector2)transform.position;

        Gizmos.DrawLine(rayStart, rayStart + forward * frontWhiskerLength);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rayStart, rayStart + (Vector2)(Quaternion.Euler(0, 0, whiskerAngle) * forward) * sideWhiskerLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(rayStart, rayStart + (Vector2)(Quaternion.Euler(0, 0, -whiskerAngle) * forward) * sideWhiskerLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }

}
