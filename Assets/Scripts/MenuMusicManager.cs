using UnityEngine;
using System.Collections;

public class MenuMusicManager : MonoBehaviour
{
    [Header("Müzik Ayarlarý")]
    public AudioSource musicSource;
    public float delaySeconds = 2.0f;
    public float fadeDuration = 1.5f;

    [Header("Buton Sesleri")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private float maxMusicVolume;

    void Start()
    {
        // Eðer AudioSource atanmamýþsa, objedeki ilk AudioSource'u bulur
        if (musicSource == null) musicSource = GetComponent<AudioSource>();

        maxMusicVolume = musicSource.volume;
        StartCoroutine(MusicLoopWithFade());
    }

    // --- MÜZÝK DÖNGÜSÜ ---
    IEnumerator MusicLoopWithFade()
    {
        while (true)
        {
            // Müzik bitene kadar bekle (Sondan fade süresi kadar önce baþla)
            yield return new WaitUntil(() => musicSource.time >= (musicSource.clip.length - fadeDuration));

            yield return StartCoroutine(FadeVolume(0, fadeDuration));
            musicSource.Stop();

            yield return new WaitForSeconds(delaySeconds);

            musicSource.volume = 0;
            musicSource.Play();
            yield return StartCoroutine(FadeVolume(maxMusicVolume, fadeDuration));
        }
    }

    IEnumerator FadeVolume(float target, float duration)
    {
        float start = musicSource.volume;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        musicSource.volume = target;
    }

    // --- BUTON FONKSÝYONLARI ---
    public void PlayHoverSound()
    {
        if (hoverSound != null) sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayClickSound()
    {
        if (clickSound != null) sfxSource.PlayOneShot(clickSound);
    }
}