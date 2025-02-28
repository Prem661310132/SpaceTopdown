using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource bgmSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
            bgmSource = GetComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
