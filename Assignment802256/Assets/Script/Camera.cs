using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player; // ����Ф÷����ͧ��ͧ���
    public float smoothSpeed = 5f; // ��Ҥ�������㹡�õ��

    void LateUpdate()
    {
        if (player == null) return;

        // �����ͧ������˹觢ͧ����Ф� �������ع
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
