using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MapSelector : MonoBehaviour
{
    [System.Serializable]
    public class MapOption
    {
        public Button button;
        public string sceneName;
    }

    [Header("Map Options")]
    [SerializeField] private MapOption[] maps = new MapOption[5];

    [Header("Position References")]
    [SerializeField] private RectTransform leftFarPosition;
    [SerializeField] private RectTransform leftPosition;
    [SerializeField] private RectTransform centerPosition;
    [SerializeField] private RectTransform rightPosition;
    [SerializeField] private RectTransform rightFarPosition;

    [Header("Scale")]
    [SerializeField] private float farScale = 0.5f;
    [SerializeField] private float sideScale = 0.75f;
    [SerializeField] private float centerScale = 1f;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.25f;

    private int selectedIndex = 0;
    private bool isAnimating = false;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
            return;
        }

        if (maps.Length != 5)
        {
            Debug.LogWarning("MapSelector requires exactly 5 map options.");
            return;
        }

        for (int i = 0; i < maps.Length; i++)
        {
            int index = i;

            if (maps[i].button != null)
            {
                maps[i].button.onClick.AddListener(() => SelectMap(index));
            }
        }

        UpdatePositionsInstant();
    }

    public void SelectNext()
    {
        if (isAnimating)
            return;

        selectedIndex++;

        if (selectedIndex >= maps.Length)
            selectedIndex = 0;

        StartCoroutine(AnimatePositions());
    }

    public void SelectPrevious()
    {
        if (isAnimating)
            return;

        selectedIndex--;

        if (selectedIndex < 0)
            selectedIndex = maps.Length - 1;

        StartCoroutine(AnimatePositions());
    }

    private void SelectMap(int index)
    {
        if (isAnimating)
            return;

        selectedIndex = index;

        StartCoroutine(AnimatePositions());
    }

    public string GetSelectedScene()
    {
        if (maps == null || maps.Length == 0)
            return "";

        return maps[selectedIndex].sceneName;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    private IEnumerator AnimatePositions()
    {
        isAnimating = true;

        Vector2[] startPositions = new Vector2[5];
        Vector3[] startScales = new Vector3[5];

        RectTransform[] transforms = GetMapTransforms();

        for (int i = 0; i < 5; i++)
        {
            startPositions[i] = transforms[i].anchoredPosition;
            startScales[i] = transforms[i].localScale;
        }

        Vector2[] targetPositions =
        {
            leftFarPosition.anchoredPosition,
            leftPosition.anchoredPosition,
            centerPosition.anchoredPosition,
            rightPosition.anchoredPosition,
            rightFarPosition.anchoredPosition
        };

        float[] targetScales =
        {
            farScale,
            sideScale,
            centerScale,
            sideScale,
            farScale
        };

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < 5; i++)
            {
                transforms[i].anchoredPosition =
                    Vector2.Lerp(startPositions[i], targetPositions[i], t);

                float scale = Mathf.Lerp(
                    startScales[i].x,
                    targetScales[i],
                    t
                );

                transforms[i].localScale = Vector3.one * scale;
            }

            yield return null;
        }

        for (int i = 0; i < 5; i++)
        {
            transforms[i].anchoredPosition = targetPositions[i];
            transforms[i].localScale = Vector3.one * targetScales[i];
        }

        UpdateSiblingOrder();

        isAnimating = false;
    }

    private void UpdatePositionsInstant()
    {
        RectTransform[] transforms = GetMapTransforms();

        Vector2[] positions =
        {
            leftFarPosition.anchoredPosition,
            leftPosition.anchoredPosition,
            centerPosition.anchoredPosition,
            rightPosition.anchoredPosition,
            rightFarPosition.anchoredPosition
        };

        float[] scales =
        {
            farScale,
            sideScale,
            centerScale,
            sideScale,
            farScale
        };

        for (int i = 0; i < 5; i++)
        {
            transforms[i].anchoredPosition = positions[i];
            transforms[i].localScale = Vector3.one * scales[i];
        }

        UpdateSiblingOrder();
    }

    private RectTransform[] GetMapTransforms()
    {
        RectTransform[] result = new RectTransform[5];

        for (int position = 0; position < 5; position++)
        {
            int mapIndex = GetMapIndexAtPosition(position);

            result[position] = maps[mapIndex].button.GetComponent<RectTransform>();
        }

        return result;
    }

    private int GetMapIndexAtPosition(int position)
    {
        int index = selectedIndex + position - 2;

        if (index < 0)
            index += maps.Length;

        if (index >= maps.Length)
            index -= maps.Length;

        return index;
    }

    private void UpdateSiblingOrder()
    {
        RectTransform[] transforms = GetMapTransforms();

        // Far left
        transforms[0].SetSiblingIndex(1);

        // Left
        transforms[1].SetSiblingIndex(0);

        // Right
        transforms[3].SetSiblingIndex(3);

        // Far right
        transforms[4].SetSiblingIndex(2);

        // Center LAST = rendered on top
        transforms[2].SetAsLastSibling();
    }
}