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
}
