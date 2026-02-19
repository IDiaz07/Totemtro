using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingBookUI : MonoBehaviour
{
    public CraftingSystem craftingSystem;

    [Header("Visual States")]
    public GameObject openBookVisual;
    public GameObject closedBookFront;
    public GameObject closedBookBack;

    [Header("Navigation")]
    public Button nextButton;
    public Button prevButton;

    [Header("Pages")]
    public Transform pagesContainer;
    public GameObject pagePrefab;
    public GameObject recipeSlotPrefab;
    public CraftingRecipe[] recipes;

    private List<GameObject> pages = new List<GameObject>();
    private int currentPageIndex = 0;

    private enum BookState { ClosedFront, Open, ClosedBack }
    public float animationDuration = 0.3f;
    bool isTurningPage = false;
    public float pageTurnDuration = 0.25f;
    public CraftingBookAnimator bookAnimator;

    void Start()
    {
        GenerateBook();
        ShowClosedFront();
    }

    // ========================
    // GENERATE PAGES
    // ========================

    void GenerateBook()
    {
        foreach (Transform child in pagesContainer)
            Destroy(child.gameObject);

        pages.Clear();
        currentPageIndex = 0;

        int recipesPerPage = 2;

        for (int i = 0; i < recipes.Length; i += recipesPerPage)
        {
            GameObject page = Instantiate(pagePrefab, pagesContainer);

            RectTransform rt = page.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;

            pages.Add(page);

            Transform slotContainer = page.transform.GetChild(0);

            for (int j = 0; j < recipesPerPage; j++)
            {
                int recipeIndex = i + j;
                GameObject slot = Instantiate(recipeSlotPrefab, slotContainer);

                if (recipeIndex < recipes.Length)
                {
                    slot.GetComponent<CraftingRecipeSlotUI>()
                        .Setup(recipes[recipeIndex], craftingSystem);
                }
                else
                {
                    slot.SetActive(false);
                }
            }

            page.SetActive(false);
        }
    }

    // ========================
    // PAGE VISIBILITY
    // ========================

    void UpdateVisiblePages()
    {
        foreach (var page in pages)
            page.SetActive(false);

        if (currentPageIndex < pages.Count)
            pages[currentPageIndex].SetActive(true);

        if (currentPageIndex + 1 < pages.Count)
            pages[currentPageIndex + 1].SetActive(true);
    }

    // ========================
    // NAVIGATION
    // ========================

    public void NextPage()
    {
        if (isTurningPage) return;

        if (currentPageIndex + 2 < pages.Count)
        {
            StartCoroutine(PageTurn(true));
        }
        else
        {
            ShowClosedBack();
        }
    }

    public void PrevPage()
    {
        if (isTurningPage) return;

        if (currentPageIndex - 2 >= 0)
        {
            StartCoroutine(PageTurn(false));
        }
        else
        {
            ShowClosedFront();
        }
    }


    // ========================
    // STATE CONTROL
    // ========================

    void ShowClosedFront()
    {
        StartCoroutine(CloseAnimation(true));
    }

    void ShowClosedBack()
    {
        StartCoroutine(CloseAnimation(false));
    }


    public void OpenFromFront()
    {
        currentPageIndex = 0;
        OpenBook();
    }

    public void OpenFromBack()
    {
        if (pages.Count == 0)
            return;

        int lastIndex = pages.Count - 1;

        // Hacerlo par (inicio de doble página)
        if (lastIndex % 2 != 0)
            lastIndex--;

        currentPageIndex = Mathf.Max(0, lastIndex);

        OpenBook();
    }

    void OpenBook()
    {
        StartCoroutine(OpenAnimation());
    }

    System.Collections.IEnumerator CloseAnimation(bool front)
    {
        RectTransform rt = openBookVisual.GetComponent<RectTransform>();

        float t = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(0.8f, 1f, 1f);

        // 🔊 SONIDO AQUÍ
        if (bookAnimator != null)
            bookAnimator.PlayCloseBook();

        while (t < animationDuration)
        {
            t += Time.unscaledDeltaTime;
            float ease = t / animationDuration;

            rt.localScale = Vector3.Lerp(startScale, endScale, ease);
            rt.localRotation = Quaternion.Euler(0, front ? -10f : 10f, 0);

            yield return null;
        }

        openBookVisual.SetActive(false);
        closedBookFront.SetActive(front);
        closedBookBack.SetActive(!front);

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

    }

    System.Collections.IEnumerator OpenAnimation()
    {
        closedBookFront.SetActive(false);
        closedBookBack.SetActive(false);
        openBookVisual.SetActive(true);

        RectTransform rt = openBookVisual.GetComponent<RectTransform>();

        rt.localScale = new Vector3(0.8f, 1f, 1f);
        rt.localRotation = Quaternion.Euler(0, 10f, 0);

        float t = 0f;

        // 🔊 SONIDO AQUÍ
        if (bookAnimator != null)
            bookAnimator.PlayOpenBook();

        while (t < animationDuration)
        {
            t += Time.unscaledDeltaTime;
            float ease = t / animationDuration;

            rt.localScale = Vector3.Lerp(new Vector3(0.8f, 1f, 1f), Vector3.one, ease);
            rt.localRotation = Quaternion.Lerp(
                Quaternion.Euler(0, 10f, 0),
                Quaternion.identity,
                ease
            );

            yield return null;
        }

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        UpdateVisiblePages();

    }

    System.Collections.IEnumerator PageTurn(bool forward)
    {
        isTurningPage = true;

        RectTransform rt = openBookVisual.GetComponent<RectTransform>();

        float t = 0f;

        Quaternion startRot = Quaternion.identity;
        Quaternion midRot = Quaternion.Euler(0, forward ? -20f : 20f, 0);
        Quaternion endRot = Quaternion.identity;

        // Primera mitad: girar
        while (t < pageTurnDuration / 2f)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / (pageTurnDuration / 2f);

            rt.localRotation = Quaternion.Lerp(startRot, midRot, lerp);
            yield return null;
        }

        // CAMBIO REAL DE PÁGINAS EN MITAD
        currentPageIndex += forward ? 2 : -2;
        UpdateVisiblePages();

        // 🔊 SONIDO AQUÍ
        if (bookAnimator != null)
            bookAnimator.PlayPageTurn();


        t = 0f;

        // Segunda mitad: volver
        while (t < pageTurnDuration / 2f)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / (pageTurnDuration / 2f);

            rt.localRotation = Quaternion.Lerp(midRot, endRot, lerp);
            yield return null;
        }

        rt.localRotation = Quaternion.identity;

        isTurningPage = false;
    }

}
