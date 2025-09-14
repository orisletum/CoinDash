using UnityEngine;

public class JoystickForMovement : JoystickHandler
{
    public Vector3 ReturnVectorDirection()
    {
        if (InputVector.x != 0 || InputVector.y != 0)
        {
            return new Vector3(InputVector.x, 0, InputVector.y);
        }
        else
        {
            return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        }
    }
}