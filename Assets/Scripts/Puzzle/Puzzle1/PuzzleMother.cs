using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleMother : MonoBehaviour, IPuzzlesElements
{
    [SerializeField] protected bool _activateRotation;
    [SerializeField] protected float _speedRotation;
    [SerializeField] protected List<MeshRenderer> _MyParts;
    [SerializeField] protected List<Material> _MyPartsBackup;

    [SerializeField] protected Material _myMaterialLight;
    public virtual void ActionPuzzle()
    {
    }

    public virtual void Activate()
    {
    }

    public virtual void Desactivate()
    {
    }

    public virtual int MyReturnNumber()
    {
        return 0;
    }
}
