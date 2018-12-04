using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 25f;
    public float zoomSpeed = 25f;
    public float centerTime = 0.5f;

    Camera cam;
    Character character;

    private bool dragging = false;
    private Vector3 mousePos;

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

        if ((Input.GetAxis("Mouse ScrollWheel") > 0f && Vector3.Distance(transform.position, cam.transform.position) > 50) || (Input.GetAxis("Mouse ScrollWheel") < 0f && Vector3.Distance(transform.position, cam.transform.position) < 400))
        {
            cam.transform.Translate(Vector3.forward * Time.deltaTime * 100 * zoomSpeed * Input.GetAxis("Mouse ScrollWheel"));
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
            i = time / centerTime;
            transform.localPosition = Vector3.Lerp(start, Vector3.zero, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
    }

    IEnumerator ZoomAnimation()
    {
        float distance = 1;
        while (distance > 0)
        {
            distance = Vector3.Distance(transform.position, cam.transform.position) - 60;
            cam.transform.Translate(Vector3.forward * Time.deltaTime * 2 * distance);
            yield return null;
        }
    }
}
