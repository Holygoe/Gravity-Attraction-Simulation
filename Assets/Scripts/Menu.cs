using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public const string SYSTEM_SIZE_PREF = "SYSTEM_SIZE";

    [SerializeField] private Attraction attraction;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI initialSystemSizeText;
    [SerializeField] private TextMeshProUGUI systemSizeText;
    [SerializeField] private TextMeshProUGUI coreSizeText;
    [SerializeField] private TextMeshProUGUI totalMassText;
    [SerializeField] private TextMeshProUGUI coreRadiusText;
    [SerializeField] private TextMeshProUGUI coreMassText;

    private void Start()
    {
        slider.onValueChanged.AddListener(SetCount);
    }
    
    private void OnEnable()
    {
        var systemSize = PlayerPrefs.GetInt(SYSTEM_SIZE_PREF, Attraction.DEFAULT_SYSTEM_SIZE);
        slider.value = systemSize;
        SetCount(systemSize);

        totalMassText.text = $"Total mass: {attraction.SystemData.TotalMass}";
    }

    private void Update()
    {
        systemSizeText.text = $"The system size: {attraction.SystemData.Size}";
        coreSizeText.text = $"The core size: {attraction.SystemData.CoreSize}";
        coreRadiusText.text = $"The core radius: {attraction.SystemData.CoreRadius}";
        //coreMassText.text = $"The core mass: {attraction.SystemData.CoreMass}";
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
