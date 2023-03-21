using Unity.Netcode;


public class GuardAnimator : BaseAnimator
{
    public override void PlayHitAnimation()
    {
        var index = GetRandomAnimationIndex();
        PlayHitAnimationClientRpc(index);
    }

    [ClientRpc]
    private void PlayHitAnimationClientRpc(byte index)
    {
        PlayHitAnimation(index);
    }
}
