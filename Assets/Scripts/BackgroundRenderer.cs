using UnityEngine;

public class BackgroundController : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private RectTransform rectTransform;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rectTransform = GetComponent<RectTransform>();
        spriteRenderer.size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
    }
}