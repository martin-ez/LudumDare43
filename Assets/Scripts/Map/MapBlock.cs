using System;
using System.Collections;
using UnityEngine;

public abstract class MapBlock : MonoBehaviour
{
    public Material[] playerMaterials;

    protected MapController map;

    protected int player;
    protected string code;
    protected bool destroy;
    protected float distance;

    public Action OnReady;
    public Action OnDestroy;

    protected Interactible interactible;
    protected bool character = false;

    void Awake()
    {
        map = FindObjectOfType<MapController>();
    }

    public virtual void Setup(int player, string code)
    {
        this.player = player;
        this.code = code;
        destroy = false;
        distance = (transform.position - Vector3.up * transform.position.y).sqrMagnitude;
    }

    public virtual void Init()
    {
        if (transform.Find("Model/Paint") != null)
        {
            Renderer[] rend = transform.Find("Model/Paint").GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rend.Length; i++)
            {
                rend[i].material = playerMaterials[player];
            }
        }      
    }

    public virtual void Appear()
    {
        StartCoroutine(AppearAnimation(0.5f + transform.position.sqrMagnitude / 10000f));
    }

    public virtual void Destroy(float threshold)
    {

        if (threshold < distance && !destroy)
        {
            if (OnDestroy != null) OnDestroy();
            destroy = true;
            StartCoroutine(DestroyAnimation(2f));
        }
    }

    public virtual void Leave()
    {
        return;
    }

    public void AddInteractible(Interactible interactible)
    {
        this.interactible = interactible;
    }

    public void RemoveInteractible()
    {
        interactible = null;
    }

    public abstract bool Action(int player, bool isInteractible);

    IEnumerator AppearAnimation(float time)
    {
        float i = 0;
        float currentTime = 0;
        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, 0, start.z);
        while (i < 1)
        {
            currentTime += Time.deltaTime;
            i = currentTime / time;
            transform.position = Vector3.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        transform.position = end;
        if (OnReady != null) OnReady();
    }

    IEnumerator DestroyAnimation(float time)
    {
        float i = 0;
        float currentTime = 0;
        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, -400, start.z);
        while (i < 1)
        {
            currentTime += Time.deltaTime;
            i = currentTime / time;
            transform.position = Vector3.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        transform.position = end;
        transform.Find("Model").gameObject.SetActive(false);
    }
}
