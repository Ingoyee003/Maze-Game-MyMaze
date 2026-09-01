using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Attach this to ANY Button GameObject (PlayButton, D-pad buttons, etc).
// Purely visual feedback - press = quick "punch" scale, hover = slight grow.
// No wiring needed beyond adding the component; it finds its own RectTransform.
[RequireComponent(typeof(RectTransform))]
public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float pressScale = 0.9f;
    [SerializeField] float hoverScale = 1.05f;
    [SerializeField] float animSpeed = 12f;

    RectTransform rt;
    Vector3 baseScale;
    Vector3 targetScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
        targetScale = baseScale;
    }

    void Update()
    {
        rt.localScale = Vector3.Lerp(rt.localScale, targetScale, Time.unscaledDeltaTime * animSpeed);
    }

    public void OnPointerDown(PointerEventData eventData) => targetScale = baseScale * pressScale;
    public void OnPointerUp(PointerEventData eventData) => targetScale = baseScale * hoverScale;
    public void OnPointerEnter(PointerEventData eventData) => targetScale = baseScale * hoverScale;
    public void OnPointerExit(PointerEventData eventData) => targetScale = baseScale;
}