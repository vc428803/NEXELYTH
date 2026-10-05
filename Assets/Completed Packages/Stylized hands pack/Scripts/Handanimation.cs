using System.Collections;
using UnityEngine;

public class Handanimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator anim;

    [Header("Animation Index")]
    [SerializeField] private int idleIndex = 8;
    [SerializeField] private int castIndex = 0;

    [Header("Cast Timing")]
    [SerializeField] private float castDuration = 1.5f;

    private bool isCasting;

    private void Start()
    {
        SetAnimation(idleIndex);
    }

    public void PlayCast()
    {
        if (isCasting)
            return;

        StartCoroutine(CastRoutine());
    }

    private IEnumerator CastRoutine()
    {
        isCasting = true;

        SetAnimation(castIndex);

        yield return new WaitForSeconds(castDuration);

        SetAnimation(idleIndex);

        isCasting = false;
    }

    private void SetAnimation(int index)
    {
        if (anim == null)
            return;

        int safeIndex = Mathf.Clamp(index, 0, 14);

        anim.SetInteger("animationIndex", safeIndex);
    }
}