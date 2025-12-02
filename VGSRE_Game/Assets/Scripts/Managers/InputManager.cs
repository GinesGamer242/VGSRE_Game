using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    bool m_IsDragging = false;
    Vector3 m_CurrentMousePosition = new Vector3(0f, 0f, -1f);
    Vector3 m_OriginalMousePosition = Vector3.zero;
    Vector3 m_OriginalCameraPosition = Vector3.zero;

    [SerializeField]
    float cameraMoveSpeed;

    private void Update()
    {
        if (m_IsDragging)
        {
            Vector3 cameraOffset = new Vector3((m_CurrentMousePosition.x - m_OriginalMousePosition.x), 0f, 0f) * cameraMoveSpeed;
            Vector3 newCameraPos = m_OriginalCameraPosition - cameraOffset;

            if (GameManager.instance.WillBeWithinScreenLimits(newCameraPos))
            {
                Camera.main.transform.position = newCameraPos;
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        var rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit) return;
        else if (rayHit.collider.gameObject.TryGetComponent<SourceBehaviour>(out SourceBehaviour thisSourceBehaviour))
        {
            Debug.Log($"'{rayHit.collider.gameObject.name}' was clicked");
            GameManager.instance.GuessSound(rayHit.collider.gameObject);
        }
    }

    public void OnDrag(InputAction.CallbackContext context)
    {
        m_IsDragging = context.performed;

        m_OriginalMousePosition = m_CurrentMousePosition;
        m_OriginalCameraPosition = Camera.main.transform.position;
    }

    public void SetMousePosition(InputAction.CallbackContext context)
    {
        m_CurrentMousePosition = new Vector3(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y, 0f);
    }
}
