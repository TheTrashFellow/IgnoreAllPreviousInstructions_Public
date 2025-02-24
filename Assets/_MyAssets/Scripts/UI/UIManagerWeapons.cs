using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerWeapons : MonoBehaviour
{
    [SerializeField] private CanvasGroup _ballesPanel = default;
    [SerializeField] private CanvasGroup _weaponsPanel = default;

    private CanvasGroup _activePanel;

    private void Start()
    {
        _activePanel = _weaponsPanel;
        _ballesPanel.alpha = 0;
    }

    public void OnWeaponsClick()
    {
        if (_activePanel != _weaponsPanel)
        {
            StartCoroutine(FadePanels(_activePanel, _weaponsPanel));
            _activePanel = _weaponsPanel;
        }
    }

    public void OnBallesClick()
    {
        if (_activePanel != _ballesPanel)
        {
            StartCoroutine(FadePanels(_activePanel, _ballesPanel));
            _activePanel = _ballesPanel;
        }
    }

    private IEnumerator FadePanels(CanvasGroup fromPanel, CanvasGroup toPanel)
    {
        yield return StartCoroutine(FadeOut(fromPanel));

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(FadeIn(toPanel));
    }

    private IEnumerator FadeOut(CanvasGroup panel)
    {
        panel.interactable = false;
        panel.blocksRaycasts = false;
        for (float t = 1; t >= 0; t -= Time.deltaTime)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.alpha = 0;
    }

    private IEnumerator FadeIn(CanvasGroup panel)
    {
        panel.interactable = true;
        panel.blocksRaycasts = true;

        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.alpha = 1;
    }
}
