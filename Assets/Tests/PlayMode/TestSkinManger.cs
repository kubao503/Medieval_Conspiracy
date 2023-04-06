using UnityEngine;
using NUnit.Framework;


public class TestSkinManger
{
    private SkinManager _skinManger;
    private int _skinsLeft;

    [Test]
    public void TestEmptySkinManagerSkinsLeft()
    {
        GivenThereIsEmptySkinManager();

        WhenSkinsLeftIsCalled();

        ThenSkinsLeftEqualsZero();
    }

    [Test]
    public void TestEmptySkinManagerGetRandomSkin()
    {
        GivenThereIsEmptySkinManager();

        ThenGetRandomSkinThrowsOutOfSkinsException();
    }

    private void GivenThereIsEmptySkinManager()
    {
        var obj = new GameObject("SkinManager");
        this._skinManger = obj.AddComponent<SkinManager>();
    }

    private void WhenSkinsLeftIsCalled()
    {
        this._skinsLeft = _skinManger.SkinsLeft;
    }

    private void ThenSkinsLeftEqualsZero()
    {
        Assert.Zero(this._skinsLeft);
    }

    private void ThenGetRandomSkinThrowsOutOfSkinsException()
    {
        Assert.Throws<OutOfSkinsException>(() => _skinManger.GetRandomSkin());
    }
}
