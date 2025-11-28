using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Desconect : MonoBehaviour
{
    [SerializeField] CharacterJoint _myJoint;
    public ElementPuzzle myElementPuzzle;

    private void OnCollisionEnter(Collision collision)
    {
        Bullet myBullet = collision.gameObject.GetComponent<Bullet>();
        if (myBullet != null)
        {
            myElementPuzzle.isLevitable = true;
            Destroy(_myJoint);
            print("cae");
        }
    }
}
