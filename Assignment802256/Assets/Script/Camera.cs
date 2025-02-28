using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player; // ตัวละครที่กล้องต้องตาม
    public float smoothSpeed = 5f; // ค่าความเร็วในการตาม

    void LateUpdate()
    {
        if (player == null) return;

        // ให้กล้องตามตำแหน่งของตัวละคร แต่ไม่หมุน
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
