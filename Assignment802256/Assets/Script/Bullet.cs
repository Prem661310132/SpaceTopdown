using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject); // ✅ ลบกระสุนเมื่อโดนผู้เล่น
        }
    }
}
