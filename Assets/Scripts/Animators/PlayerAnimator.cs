using Unity.Netcode;


public class PlayerAnimator : BaseAnimator
{
    public override void PlayHitAnimation()
    {
        var index = GetRandomAnimationIndex();
        PlayHitAnimation(index);
        PlayHitAnimationServerRpc(index);
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
}
