using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Range(0, 1)]
    public float SfxVolume;
    [Range(0, 1)]
    public float MusicVolume;

    AudioSource sfx;
    AudioSource[] offSources;
    AudioSource[] onSources;

    private float nextSound;

    public enum Sound
    {
        No,
        Action,
        PickupInstruments,
        PickupMoney,
        PickupFans,
        Finish
    }

    [Header("Song Clips")]
    public AudioClip[] drummyOff;
    public AudioClip[] bassyOff;
    public AudioClip[] keysyOff;

    public AudioClip[] drummyOn;
    public AudioClip[] bassyOn;
    public AudioClip[] keysyOn;

    [Header("Fx Clips")]
    public AudioClip no;
    public AudioClip action;
    public AudioClip pickupInstruments;
    public AudioClip pickupMoney;
    public AudioClip pickupFans;
    public AudioClip finnish;

    private static bool created = false;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;

            offSources = new AudioSource[3];
            onSources = new AudioSource[3];

            GameObject sfx2DS = new GameObject("SFX_Source");
            sfx = sfx2DS.AddComponent<AudioSource>();
            sfx.volume = SfxVolume;
            DontDestroyOnLoad(sfx2DS.gameObject);

            GameObject source1 = new GameObject("MusicSource drummyOffSource");
            offSources[0] = source1.AddComponent<AudioSource>();
            offSources[0].volume = 0;
            DontDestroyOnLoad(source1.gameObject);
            GameObject source2 = new GameObject("MusicSource bassyOffSource");
            offSources[1] = source2.AddComponent<AudioSource>();
            offSources[1].volume = 0;
            DontDestroyOnLoad(source2.gameObject);
            GameObject source3 = new GameObject("MusicSource keysyOffSource");
            offSources[2] = source3.AddComponent<AudioSource>();
            offSources[2].volume = 0;
            DontDestroyOnLoad(source3.gameObject);
            GameObject source4 = new GameObject("MusicSource drummyOnSource");
            onSources[0] = source4.AddComponent<AudioSource>();
            onSources[0].volume = 0;
            DontDestroyOnLoad(source4.gameObject);
            GameObject source5 = new GameObject("MusicSource bassyOnSource");
            onSources[1] = source5.AddComponent<AudioSource>();
            onSources[1].volume = 0;
            DontDestroyOnLoad(source5.gameObject);
            GameObject source6 = new GameObject("MusicSource keysyOnSource");
            onSources[2] = source6.AddComponent<AudioSource>();
            onSources[2].volume = 0;
            DontDestroyOnLoad(source6.gameObject);

            nextSound = Time.time;
            ChangeSong(0);
        }
    }

    public void ChangeSong(int chapter)
    {
        StartCoroutine(FadeSong(chapter));
    }

    public void TurnOff(int character)
    {
        StartCoroutine(FadeOff(character));
    }

    public void TurnFilter(int character)
    {
        StartCoroutine(FadeFilter(character));
    }

    public void TurnOn(int character)
    {
        StartCoroutine(FadeOn(character));
    }

    public void TurnOnAll()
    {
        StartCoroutine(FadeOn(0));
        StartCoroutine(FadeOn(1));
        StartCoroutine(FadeOn(2));
    }

    public void PlaySound(Sound clipName)
    {
        if (Time.time > nextSound)
        {
            AudioClip clip = null;
            switch (clipName)
            {
                case Sound.No:
                    clip = no;
                    break;
                case Sound.Action:
                    clip = action;
                    break;
                case Sound.PickupInstruments:
                    clip = pickupInstruments;
                    break;
                case Sound.PickupMoney:
                    clip = pickupMoney;
                    break;
                case Sound.PickupFans:
                    clip = pickupFans;
                    break;
                case Sound.Finish:
                    clip = finnish;
                    break;
            }
            if (clip != null)
            {
                sfx.clip = clip;
                sfx.time = 0f;
                sfx.loop = false;
                sfx.Play();
            }
            nextSound = Time.time + 0.5f;
        }
    }

    IEnumerator FadeSong(int song)
    {
        float[] startOff = new float[3];
        float[] startOn = new float[3];
        for (int j = 0; j < 3; j++)
        {
            startOff[j] = offSources[j].volume;
            startOn[j] = onSources[j].volume;
        }
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / 1f;
            for (int j = 0; j < 3; j++)
            {
                offSources[j].volume = Mathf.Lerp(startOff[j], 0f, i);
                onSources[j].volume = Mathf.Lerp(startOn[j], 0f, i);
            }
            yield return null;
        }
        for (int j = 0; j < 3; j++)
        {
            onSources[j].volume = 0;
            offSources[j].volume = 0;
            AudioClip offClip = null;
            AudioClip onClip = null;
            switch (j)
            {
                case 0:
                    offClip = drummyOff[song];
                    onClip = drummyOn[song];
                    break;
                case 1:
                    offClip = bassyOff[song];
                    onClip = bassyOn[song];
                    break;
                case 2:
                    offClip = keysyOff[song];
                    onClip = keysyOn[song];
                    break;
            }
            offSources[j].clip = offClip;
            offSources[j].loop = true;
            offSources[j].Play();
            offSources[j].time = 0;

            onSources[j].clip = onClip;
            onSources[j].loop = true;
            onSources[j].Play();
            onSources[j].time = 0;
        }
        time = 0;
        i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / 1f;
            for (int j = 0; j < 3; j++)
            {
                offSources[j].volume = Mathf.Lerp(0f, MusicVolume, i);
                onSources[j].volume = Mathf.Lerp(0f, MusicVolume, i);
            }
            yield return null;
        }
        for (int j = 0; j < 3; j++)
        {
            onSources[j].volume = MusicVolume;
            offSources[j].volume = MusicVolume;
        }
    }

    IEnumerator FadeOff(int character)
    {
        float startOff = offSources[character].volume;
        float startOn = onSources[character].volume;
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / 2f;
            offSources[character].volume = Mathf.Lerp(startOff, 0f, i);
            onSources[character].volume = Mathf.Lerp(startOn, 0f, i);
            yield return null;
        }
        offSources[character].volume = 0;
        onSources[character].volume = 0;
    }

    IEnumerator FadeFilter(int character)
    {
        float startOff = offSources[character].volume;
        float startOn = onSources[character].volume;
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / 2f;
            offSources[character].volume = Mathf.Lerp(startOff, MusicVolume, i);
            onSources[character].volume = Mathf.Lerp(startOn, 0f, i);
            yield return null;
        }
        offSources[character].volume = MusicVolume;
        onSources[character].volume = 0f;
    }

    IEnumerator FadeOn(int character)
    {
        float startOff = offSources[character].volume;
        float startOn = onSources[character].volume;
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / 2f;
            offSources[character].volume = Mathf.Lerp(startOff, 0f, i);
            onSources[character].volume = Mathf.Lerp(startOn, MusicVolume, i);
            yield return null;
        }
        offSources[character].volume = 0f;
        onSources[character].volume = MusicVolume;
    }
}
