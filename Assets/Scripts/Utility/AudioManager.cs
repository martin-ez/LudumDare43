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

    private readonly float[] tempos = new float[] { 128f, 92f, 105f };
    private float fadeTime = 60f / 128f;
    private int chapter = -1;
    private int newLevelPlayers;
    private bool[] onPlayer;
    private bool[] deadPlayer;

    private static bool created = false;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;
        }

        onPlayer = new bool[3];
        deadPlayer = new bool[3];

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
    }

    public int GetChapter()
    {
        return chapter;
    }

    public void ChangeLevel(int players)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < players)
            {
                if (onPlayer[i])
                {
                    StartCoroutine(TogglePlayer(onSources[i], offSources[i], 0f));
                }
            }
            else
            {
                KillPlayer(i);
            }
        }
    }

    public void ChangeChapter(int chapter, int players)
    {
        this.chapter = chapter;
        newLevelPlayers = players;
        fadeTime = 60f * 4f / tempos[chapter];
        StartCoroutine(FadeOutFadeIn());
    }

    public void PlayFullSong()
    {
        if (chapter == -1) chapter = 0;
        fadeTime = 60f * 4f / tempos[chapter];
        newLevelPlayers = 3;
        SetupClips(true);
        StartCoroutine(FadeInFull());
    }

    public void TogglePlayer(int player, float delay)
    {
        if (!onPlayer[player])
        {
            onPlayer[player] = true;
            StartCoroutine(TogglePlayer(offSources[player], onSources[player], delay));
        }
    }

    public void KillPlayer(int player)
    {
        if (!deadPlayer[player])
        {
            deadPlayer[player] = true;
            if (onPlayer[player])
            {
                StartCoroutine(FadeOutPlayer(onSources[player]));
            }
            else
            {
                StartCoroutine(FadeOutPlayer(offSources[player]));
            }
        }
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

    private void SetupClips(bool on)
    {
        for (int j = 0; j < 3; j++)
        {
            onSources[j].volume = 0;
            offSources[j].volume = 0;
            AudioClip offClip = null;
            AudioClip onClip = null;
            switch (j)
            {
                case 0:
                    offClip = drummyOff[chapter];
                    onClip = drummyOn[chapter];
                    break;
                case 1:
                    offClip = bassyOff[chapter];
                    onClip = bassyOn[chapter];
                    break;
                case 2:
                    offClip = keysyOff[chapter];
                    onClip = keysyOn[chapter];
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

            onPlayer[j] = on;
            deadPlayer[j] = j >= newLevelPlayers;
        }
    }

    IEnumerator FadeInFull()
    {
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / (fadeTime / 2f);
            float val = Mathf.Lerp(0, MusicVolume, i);
            for (int j = 0; j < 3; j++)
            {
                onSources[j].volume = val;
            }
            yield return null;
        }
    }

    IEnumerator FadeOutFadeIn()
    {
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / (fadeTime / 2f);
            float val = Mathf.Lerp(MusicVolume, 0, i);
            for (int j = 0; j < 3; j++)
            {
                if (!deadPlayer[j])
                {
                    if (onPlayer[j])
                    {
                        offSources[j].volume = 0;
                        onSources[j].volume = val;
                    }
                    else
                    {
                        onSources[j].volume = 0;
                        offSources[j].volume = val;
                    }
                }
            }
            yield return null;
        }
        SetupClips(false);
        time = 0;
        i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / (fadeTime / 2f);
            float val = Mathf.Lerp(0, MusicVolume, i);
            for (int j = 0; j < 3; j++)
            {
                if (!deadPlayer[j])
                {
                    offSources[j].volume = val;
                }
            }
            yield return null;
        }
    }

    IEnumerator TogglePlayer(AudioSource sourceOut, AudioSource sourceIn, float delay)
    {
        yield return new WaitForSeconds(delay);
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / fadeTime;
            float val = Mathf.Lerp(0, MusicVolume, i);
            sourceIn.volume = val;
            sourceOut.volume = MusicVolume - val;
            yield return null;
        }
        sourceIn.volume = MusicVolume;
        sourceOut.volume = 0;
    }

    IEnumerator FadeInPlayer(AudioSource sourceIn)
    {
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / fadeTime;
            float val = Mathf.Lerp(0, MusicVolume, i);
            sourceIn.volume = val;
            yield return null;
        }
        sourceIn.volume = MusicVolume;
    }

    IEnumerator FadeOutPlayer(AudioSource sourceOut)
    {
        float time = 0;
        float i = 0;
        while (i < 1f)
        {
            time += Time.deltaTime;
            i = time / fadeTime;
            float val = Mathf.Lerp(MusicVolume, 0, i);
            sourceOut.volume = val;
            yield return null;
        }
        sourceOut.volume = 0;
    }
}
