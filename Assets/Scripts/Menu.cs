using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public const string SYSTEM_SIZE_PREF = "SYSTEM_SIZE";

    [SerializeField] private Simulation simulation;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI initialSystemSizeText;
    [SerializeField] private TextMeshProUGUI systemSizeText;

    private void Start()
    {
        slider.onValueChanged.AddListener(SetCount);
    }
    
    private void OnEnable()
    {
        var systemSize = PlayerPrefs.GetInt(SYSTEM_SIZE_PREF, Simulation.DEFAULT_SYSTEM_SIZE);
        slider.value = systemSize;
        SetCount(systemSize);
    }

    private void Update()
    {
        systemSizeText.text = $"System size: {simulation.CurrentSystemSize}";
    }

    private void SetCount(float value)
    {
        initialSystemSizeText.text = $"The initial system size: {(int) value}";
    }

    // ReSharper disable once UnusedMember.Local
    private void OnRestart()
    {
        PlayerPrefs.SetInt(SYSTEM_SIZE_PREF, (int) slider.value);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }
}
