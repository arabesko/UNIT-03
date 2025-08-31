using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class PuzzleFusibles : MonoBehaviour
{
    public List<Transform> fuseSlots;
    public TMP_Text percentText;
    public Transform door;
    public Transform doorOpenPosition;

    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;
    public AudioClip fuseInsertSound;
    public float openSpeed = 1f;

    private List<GameObject> insertedFuses = new List<GameObject>();
    private int totalPercent = 0;

    private void OnTriggerEnter(Collider other)
    {
        ElementPuzzle fuse = other.GetComponent<ElementPuzzle>();

        if (fuse != null && !insertedFuses.Contains(fuse.gameObject))
        {
            foreach (Transform slot in fuseSlots)
            {
                if (slot.childCount == 0)
                {
                    InsertFuse(fuse, slot);
                    break;
                }
            }
        }
    }

    private void InsertFuse(ElementPuzzle fuse, Transform slot)
    {
        insertedFuses.Add(fuse.gameObject);

        PlayerMovement player = fuse._player;
        if (player != null && player.colectables.Contains(fuse.gameObject))
        {
            player.colectables.Remove(fuse.gameObject);
            player.NoLevitate();
        }

        Rigidbody rb = fuse.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        fuse.transform.SetParent(slot);
        fuse.transform.localPosition = Vector3.zero;
        fuse.transform.localRotation = Quaternion.identity;
        fuse.transform.localScale = Vector3.one;

        fuse.Desactivate();

        
        if (doorAudioSource != null && fuseInsertSound != null)
        {
            doorAudioSource.PlayOneShot(fuseInsertSound);
        }

        totalPercent += fuse.MyReturnNumber();
        percentText.text = totalPercent.ToString() + "%";

        if (totalPercent >= 100)
        {
            StartCoroutine(OpenDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        float t = 0;
        Vector3 startPos = door.position;
        Vector3 endPos = doorOpenPosition.position;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
}