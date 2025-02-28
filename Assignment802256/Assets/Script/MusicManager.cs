using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance; // ทำให้มีแค่ตัวเดียว
    private AudioSource bgmSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ป้องกันการถูกทำลายตอนเปลี่ยนฉาก
            bgmSource = GetComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Destroy(gameObject); // ถ้ามีตัวอื่นอยู่ ให้ทำลายตัวใหม่
        }
    }
}
