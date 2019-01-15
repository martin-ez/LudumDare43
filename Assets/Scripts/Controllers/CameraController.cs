using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 25f;
    public float zoomSpeed = 25f;

    Camera cam;
    Character character;

    private bool dragging = false;
    private Vector3 mousePos;
    private float currentZoom = 60f;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (dragging)
            {
                Vector3 diff = Input.mousePosition - mousePos;
                transform.eulerAngles += Vector3.up * Time.deltaTime * rotationSpeed * diff.x;
            }
            else
            {
                dragging = true;
            }
            mousePos = Input.mousePosition;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            dragging = false;
        }

        if ((Input.GetAxis("Mouse ScrollWheel") > 0f && currentZoom > 40) || (Input.GetAxis("Mouse ScrollWheel") < 0f && currentZoom < 200))
        {
            cam.transform.Translate(Vector3.forward * Time.deltaTime * 100 * zoomSpeed * Input.GetAxis("Mouse ScrollWheel"));
            currentZoom = Vector3.Distance(transform.position, cam.transform.position);
        }
    }

    public void ChangeCharacter(Character character)
    {
        StopAllCoroutines();
        this.character = character;
        if (character.IsReady())
        {
            transform.SetParent(character.transform);
            StartCoroutine(CenterAnimation());
            StartCoroutine(ZoomAnimation());
        }
        else
        {
            character.OnReady += CharacterReady;
        }
    }

    private void CharacterReady()
    {
        character.OnReady -= CharacterReady;
        transform.SetParent(character.transform);
        StartCoroutine(CenterAnimation());
        StartCoroutine(ZoomAnimation());
    }

    IEnumerator CenterAnimation()
    {
        Vector3 start = transform.localPosition;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / 0.75f;
            transform.localPosition = Vector3.Lerp(start, Vector3.zero, Easing.Ease(i, Easing.Functions.CubicEaseOut));
            yield return null;
        }
    }

    IEnumerator ZoomAnimation()
    {
        float distance = Vector3.Distance(transform.position, cam.transform.position) - currentZoom;
        while (distance > 0)
        {
            distance = Vector3.Distance(transform.position, cam.transform.position) - currentZoom;
            cam.transform.Translate(Vector3.forward * Time.deltaTime * 80);
            yield return null;
        }
    }
}
