using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cameraTransform;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 targetPos = cameraTransform.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, 0.9f);
    }
}