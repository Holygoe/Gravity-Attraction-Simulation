using UnityEngine;

public class ViewPoint : MonoBehaviour
{
    [SerializeField] private Type type;
    
    private Attraction _attraction;

    private void Start()
    {
        _attraction = FindObjectOfType<Attraction>();
    }

    private void Update()
    {
        transform.position = type switch
        {
            Type.MassCenter => _attraction.GravitySystem.MassCenter,
            Type.CoreCenter => _attraction.GravitySystem.CoreCenter,
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }

    private enum Type
    {
        MassCenter, CoreCenter
    }
}
