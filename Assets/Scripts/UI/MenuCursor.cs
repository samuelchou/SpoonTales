using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 選單游標：停在目前選取按鈕左側 offset 像素、垂直置中。
// 滑鼠移到按鈕上也會選取該按鈕；沒有選取時自動選取畫面上第一個可用的按鈕。
[RequireComponent(typeof(RectTransform))]
public class MenuCursor : MonoBehaviour
{
    [Tooltip("要搜尋按鈕的根物件；留空用所在的 Canvas")]
    [SerializeField] private Transform searchRoot;
    [Tooltip("游標右緣與按鈕左緣的距離（Canvas 參考解析度像素）")]
    [SerializeField] private float offset = 24f;

    private RectTransform rectTransform;
    private Graphic graphic;
    private Button[] buttons;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        graphic = GetComponent<Graphic>();
        if (searchRoot == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            searchRoot = canvas != null ? canvas.rootCanvas.transform : transform.parent;
        }

        buttons = searchRoot.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.GetComponent<SelectOnHover>() == null)
            {
                button.gameObject.AddComponent<SelectOnHover>();
            }
        }
    }

    private void LateUpdate()
    {
        EventSystem eventSystem = EventSystem.current;
        GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;

        if (eventSystem != null && !IsUsable(selected))
        {
            selected = FirstUsableButton();
            eventSystem.SetSelectedGameObject(selected);
        }

        bool visible = IsUsable(selected);
        if (graphic != null)
        {
            graphic.enabled = visible;
        }
        if (!visible)
        {
            return;
        }

        RectTransform target = (RectTransform)selected.transform;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners); // 0 左下, 1 左上
        Vector3 leftMiddle = (corners[0] + corners[1]) * 0.5f;

        rectTransform.pivot = new Vector2(1f, 0.5f);
        rectTransform.position = leftMiddle + Vector3.left * offset * rectTransform.lossyScale.x;
    }

    private bool IsUsable(GameObject go)
    {
        if (go == null || !go.activeInHierarchy)
        {
            return false;
        }
        Button button = go.GetComponent<Button>();
        return button != null && button.IsInteractable() && go.transform.IsChildOf(searchRoot);
    }

    private GameObject FirstUsableButton()
    {
        foreach (Button button in buttons)
        {
            if (IsUsable(button.gameObject))
            {
                return button.gameObject;
            }
        }
        return null;
    }

    private class SelectOnHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}
