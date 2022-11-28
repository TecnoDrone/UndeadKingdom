using UnityEngine;

[ExecuteInEditMode]
public class UIperfectAnchor : MonoBehaviour
{
  public float width;
  public float height;

  public float left;
  public float top;

  private RectTransform rectTransform;

  // Start is called before the first frame update
  void Start()
  {
    rectTransform = GetComponent<RectTransform>();
  }

  // Update is called once per frame
  void Update()
  {
    var GUI = GameObject.Find("GUI");
    var parentRectTransform = GUI.GetComponent<RectTransform>();

    var parentSize = parentRectTransform.sizeDelta;

    var minX = left / parentSize.x;
    var maxX = (left + width) / parentSize.x;

    var minY = (parentSize.y - (top + height)) / parentSize.y;
    var maxY = (parentSize.y - top) / parentSize.y;

    rectTransform.anchorMin = new Vector2(minX, minY);
    rectTransform.anchorMax = new Vector2(maxX, maxY);
  }
}
