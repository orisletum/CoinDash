using UnityEngine;
using UnityEngine.UIElements;
public enum JoystickSide
{
    Left,
    Right 
};
public class JoystickHandler : MonoBehaviour
{
    [SerializeField] protected string   _areaName = "joystickArea";
    [SerializeField] protected string   _backgroundName = "joystickBackground";
    [SerializeField] protected string   _joystickName = "joystick";
    private VisualElement               _joystickArea;
    private VisualElement               _joystickBackground;
    private VisualElement               _joystick;

    private Vector2                     _joystickBackgroundStartPosition;
    private Vector2                     _joystickStartPosition;
    

    [SerializeField] private Color      _inActiveJoystickColor;
    [SerializeField] private Color      _activeJoystickColor;

    private bool                        _isActiveJoistick = false;
    public Vector2                      InputVector;
    public JoystickSide                 JoystickSide = JoystickSide.Left;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _joystickArea = root.Q<VisualElement>(_areaName);
        _joystickBackground = root.Q<VisualElement>(_backgroundName);
        _joystick = root.Q<VisualElement>(_joystickName);

        _joystickBackgroundStartPosition = new Vector2(
            _joystickBackground.style.left.value.value,
            _joystickBackground.style.top.value.value
        );

        _joystickStartPosition = new Vector2(
            _joystickBackground.layout.width / 2,
            _joystickBackground.layout.height / 2
        );

        _joystickArea.RegisterCallback<PointerDownEvent>(OnPointerDown);
        _joystickArea.RegisterCallback<PointerMoveEvent>(OnDrag);
        _joystickArea.RegisterCallback<PointerUpEvent>(OnPointerUp);

        ClickEffect(_isActiveJoistick);
    }

    private void ClickEffect(bool state)
    {
        _joystick.style.backgroundColor = state ? _activeJoystickColor : _inActiveJoystickColor;
        _isActiveJoistick = state;
        _joystickBackground.visible = _isActiveJoistick;
    }


    private void OnDrag(PointerMoveEvent evt)
    {
        if(!_isActiveJoistick) return;
        UpdateJoystickPosition(evt.position);
    }

    private bool CheckJoystickPosition(Vector3 clickPos) => 
        (JoystickSide == JoystickSide.Left && clickPos.x < _joystickArea.layout.width / 2) || 
        (JoystickSide == JoystickSide.Right && clickPos.x >= _joystickArea.layout.width / 2) ;

    private void OnPointerDown(PointerDownEvent evt)
    {
        _isActiveJoistick = CheckJoystickPosition(evt.position);
      
        if (!_isActiveJoistick) return;
        ClickEffect(_isActiveJoistick);
        InputVector = Vector2.zero;
        _joystickBackground.style.left = evt.position.x;
        _joystickBackground.style.top = evt.position.y;
        _joystick.style.left = 65;
        _joystick.style.top = 65;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_isActiveJoistick) return;
        _joystick.style.left = _joystickStartPosition.x;
        _joystick.style.top = _joystickStartPosition.y;

        InputVector = Vector2.zero;
        ClickEffect(false);
    }

    private void UpdateJoystickPosition(Vector3 evtposition)
    {
        Vector2 worldPosition = _joystickBackground.worldBound.position;
        Vector2 localPosition = new Vector2(
            evtposition.x - worldPosition.x,
            evtposition.y - worldPosition.y
        );

        float width = _joystickBackground.layout.width;
        float height = _joystickBackground.layout.height;

        float normalizedX = (localPosition.x - width / 2) / (width / 2);
        float normalizedY = (localPosition.y - height / 2) / (height / 2);

        InputVector = new Vector2(normalizedX, normalizedY);
        InputVector = InputVector.magnitude > 1f ? InputVector.normalized : InputVector;

        _joystick.style.left = (InputVector.x * (width / 2)) + (width / 2);
        _joystick.style.top = (InputVector.y * (height / 2)) + (height / 2);
    }
}