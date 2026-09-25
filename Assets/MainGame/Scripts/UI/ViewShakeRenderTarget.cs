using UnityEngine;
using UnityEngine.UI;

// Plain component, like OnHitEffect. Makes a "view shake" possible (see
// Feel's Toaster demo): the gameplay camera renders into a RenderTexture
// instead of the screen, and a full-screen RawImage shows that texture, so
// shaking the RawImage (an MMPositionShaker on it) moves the whole finished
// image rather than the camera. The texture is created here at runtime and
// kept at the screen's size, so the picture is never stretched or letterboxed
// whatever the window's aspect ratio.
//
// The display camera and RawImage are authored disabled and only switched on
// here, at runtime: in edit mode the display camera would otherwise draw an
// empty image over the gameplay camera. Disabling this component hands the
// screen straight back to the capturing camera.
public class ViewShakeRenderTarget : MonoBehaviour
{
    [Tooltip("The gameplay camera, rendered into the texture instead of the screen.")]
    [SerializeField] private Camera capturingCamera;

    [Tooltip("Camera that draws only the display canvas to the screen. Author it disabled; this enables it at runtime.")]
    [SerializeField] private Camera displayCamera;

    [Tooltip("Full-screen RawImage that shows the texture — the thing the view shake moves. Author it disabled; this enables it at runtime.")]
    [SerializeField] private RawImage display;

    [Tooltip("Depth buffer bits for the texture (0, 16, 24 or 32).")]
    [SerializeField] private int depthBits = 24;

    private RenderTexture _texture;

    private void OnEnable()
    {
        Rebuild();
        display.enabled = true;
        displayCamera.enabled = true;
    }

    private void OnDisable()
    {
        if (capturingCamera != null)
        {
            capturingCamera.targetTexture = null;
        }

        if (displayCamera != null)
        {
            displayCamera.enabled = false;
        }

        if (display != null)
        {
            display.texture = null;
            display.enabled = false;
        }

        ReleaseTexture();
    }

    private void Update()
    {
        if (_texture == null || _texture.width != Screen.width || _texture.height != Screen.height)
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        // Unassigned before releasing: a camera must never render into a destroyed texture.
        capturingCamera.targetTexture = null;
        ReleaseTexture();

        _texture = new RenderTexture(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height), depthBits)
        {
            name = "ViewShakeRenderTexture"
        };
        _texture.Create();

        capturingCamera.targetTexture = _texture;
        display.texture = _texture;
    }

    private void ReleaseTexture()
    {
        if (_texture == null)
        {
            return;
        }

        _texture.Release();
        Destroy(_texture);
        _texture = null;
    }
}
