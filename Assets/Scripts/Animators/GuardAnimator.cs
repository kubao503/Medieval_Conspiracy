using UnityEngine;
using Unity.Netcode;


public class GuardAnimator : NetworkBehaviour
{
    private Animator _animator;
    private int _animationsCount;

    public void PlayHitAnimation()
    {
        var index = GetRandomHitAnimationIndex();
        PlayHitAnimationClientRpc(index);
    }

    private byte GetRandomHitAnimationIndex()
    {
        return (byte)Random.Range(0, _animationsCount);
    }

    [ClientRpc]
    private void PlayHitAnimationClientRpc(byte index)
    {
        _animator.SetInteger("Index", index);
        _animator.SetTrigger("Play");
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animationsCount = _animator.runtimeAnimatorController.animationClips.Length;
    }
}
