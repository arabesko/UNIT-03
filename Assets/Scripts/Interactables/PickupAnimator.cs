using UnityEngine;

public class PickupAnimator : MonoBehaviour
{
    [Header("Rotación")]
    public Vector3 rotationPerSecond = new Vector3(0, 90, 0);

    [Header("Bobbing")]
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 1f;

    private Vector3 startLocalPos;

    void Start()
    {
        // Guardamos la posición LOCAL inicial, no la world
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        // 1) Rotar en Local Space para no alterar la posición padre
        transform.Rotate(rotationPerSecond * Time.deltaTime, Space.Self);

        // 2) Bobbing: usamos localPosition, sumando un offset Y al startLocalPos
        float newY = startLocalPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        Vector3 localPos = new Vector3(startLocalPos.x, newY, startLocalPos.z);
        transform.localPosition = localPos;
    }
}
