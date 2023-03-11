using UnityEngine;


public interface IInput
{
    enum Axis
    {
        Vertical,
        Horizontal
    }

    bool GetKeyDown(KeyCode key);
    bool GetKeyUp(KeyCode key);
    bool GetLeftMouseButtonDown();
    Vector2 GetKeyAxis();
    Vector2 GetMouseAxis();
    bool IsCursorLocked();
}


public class InputAdapter : IInput
{
    public static InputAdapter Instance = new InputAdapter();

    public bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public bool GetKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key);
    }

    public bool GetLeftMouseButtonDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    public Vector2 GetKeyAxis()
    {
        return new Vector2()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };
    }

    public Vector2 GetMouseAxis()
    {
        return new Vector2()
        {
            x = Input.GetAxis("Mouse X"),
            y = Input.GetAxis("Mouse Y")
        };
    }

    public bool IsCursorLocked()
    {
        return Cursor.lockState != CursorLockMode.None;
    }
}


public class FakeInputAdapter : IInput
{
    private Vector2 _keyAxis;
    private Vector2 _mouseAxis;
    private bool _keyPress;

    public FakeInputAdapter(Vector2 keyAxis, Vector2 mouseAxis, bool keyPress)
    {
        this._keyAxis = keyAxis;
        this._mouseAxis = mouseAxis;
        this._keyPress = keyPress;
    }

    public Vector2 GetKeyAxis()
    {
        return _keyAxis;
    }

    public bool GetKeyDown(KeyCode key)
    {
        return _keyPress;
    }

    public bool GetKeyUp(KeyCode key)
    {
        return _keyPress;
    }

    public bool GetLeftMouseButtonDown()
    {
        return _keyPress;
    }

    public Vector2 GetMouseAxis()
    {
        return _mouseAxis;
    }

    public bool IsCursorLocked()
    {
        return true;
    }
}
