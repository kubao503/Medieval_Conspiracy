using UnityEngine;
using Unity.Netcode;


public abstract class BaseAnimator : NetworkBehaviour
{
    private Animator _animator;
    private int _animationsCount;

    public abstract void PlayHitAnimation();

    protected byte GetRandomAnimationIndex()
    {
        return (byte)Random.Range(0, _animationsCount);
    }

    protected void PlayHitAnimation(byte index)
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
