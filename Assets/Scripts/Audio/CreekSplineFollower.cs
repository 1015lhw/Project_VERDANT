using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class CreekSplineFollower : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public SplineContainer creekSpline;

    [Header("Movement")]
    public float followSpeed = 8f;

    void Update()
    {
        if (player == null || creekSpline == null)
            return;

        float3 localPlayerPos = creekSpline.transform.InverseTransformPoint(player.position);

        SplineUtility.GetNearestPoint(
            creekSpline.Spline,
            localPlayerPos,
            out float3 nearestLocalPoint,
            out float t
        );

        Vector3 nearestWorldPoint = creekSpline.transform.TransformPoint(nearestLocalPoint);

        transform.position = Vector3.Lerp(
            transform.position,
            nearestWorldPoint,
            Time.deltaTime * followSpeed
        );
    }
}