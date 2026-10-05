using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-----Audio Source------")]
    //Audio source
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-----Audio Clips------")]
    //Audio Clips
    public AudioClip background;
    public AudioClip BallPop;
    public AudioClip Bomb;
    public AudioClip GameOverAudio;
    

    public static AudioManager instance;

    //to not restart music upon scene change
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //start music when game starts
    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
    //for sound effects
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
