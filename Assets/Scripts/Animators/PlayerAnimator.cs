using UnityEngine;
using Unity.Netcode;


public class PlayerAnimator : NetworkBehaviour
{
    private Animator _animator;
    private int _animationsCount;

    public void PlayHitAnimation()
    {
        var index = GetRandomAnimationIndex();
        PlayHitAnimation(index);
        PlayHitAnimationServerRpc(index);
    }

    private byte GetRandomAnimationIndex()
    {
        return (byte)UnityEngine.Random.Range(0, _animationsCount);
    }

    private void PlayHitAnimation(byte index)
    {
        _animator.SetInteger("Index", index);
        _animator.SetTrigger("Play");
    }

    [ServerRpc]
    private void PlayHitAnimationServerRpc(byte index)
    {
        PlayHitAnimationClientRpc(index);
    }

    [ClientRpc]
    private void PlayHitAnimationClientRpc(byte index)
    {
        if (!IsOwner) // Owner has already played this animation
            PlayHitAnimation(index);
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animationsCount = _animator.runtimeAnimatorController.animationClips.Length;
    }
}
