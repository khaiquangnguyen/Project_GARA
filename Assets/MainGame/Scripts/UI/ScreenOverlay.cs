using UnityEngine;

// Keeps a SpriteRenderer centred on the main camera and scaled to cover its
// view, so a screen-wide effect spawned under a character still fills the
// screen wherever that character stands.
[RequireComponent(typeof(SpriteRenderer))]
public class ScreenOverlay : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic || _spriteRenderer.sprite == null)
        {
            return;
        }

        var viewHeight = mainCamera.orthographicSize * 2f;
        var viewWidth = viewHeight * mainCamera.aspect;
        var spriteSize = _spriteRenderer.sprite.bounds.size;
        var cover = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);
        var parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(cover / parentScale.x, cover / parentScale.y, 1f);

        var cameraPosition = mainCamera.transform.position;
        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);
    }
}
