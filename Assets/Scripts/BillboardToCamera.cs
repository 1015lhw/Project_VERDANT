using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public Transform cameraTransform;

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        transform.forward = cameraTransform.forward;
    }
}