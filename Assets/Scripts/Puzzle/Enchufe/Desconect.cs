using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Desconect : MonoBehaviour
{
    [SerializeField] CharacterJoint _myJoint;
    [SerializeField] ElementPuzzle _myElementPuzzle;

    private void OnCollisionEnter(Collision collision)
    {
        Bullet myBullet = collision.gameObject.GetComponent<Bullet>();
        if (myBullet != null)
        {
            _myElementPuzzle.isLevitable = true;
            Destroy(_myJoint);
        }
    }
}
