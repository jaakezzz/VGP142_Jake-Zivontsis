using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Transform target; // head bone
    public Vector3 offset = new Vector3(0, 2f, 0);
    public Image fill;
    Camera cam;

    void Start() { cam = Camera.main; }
    public void Bind(Health h)
    {
        h.onHealthChanged.AddListener((c, m) => { if (fill) fill.fillAmount = c / m; });
    }

    void LateUpdate()
    {
        if (!cam || !target) return;
        transform.position = target.position + offset;
        transform.forward = cam.transform.forward;
    }
}
