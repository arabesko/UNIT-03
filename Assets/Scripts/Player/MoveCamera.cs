using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform _target;

    [Header("Ajustes")]
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private float _xOffset = 2f;
    [SerializeField] private float _yOffset = 8.3f;
    [SerializeField] private float _zOffset = -10f;

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desiredPosition = new Vector3(
            _target.position.x + _xOffset,
            _target.position.y + _yOffset,
            _target.position.z + _zOffset
        );

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}