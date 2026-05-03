using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDefinition", menuName = "Game/Game Definition")]
public class GameDefinition : ScriptableObject
{
    [Tooltip("All duck types in the game — include unowned ones. Order = tab order.")]
    public List<DuckDefinition> allDucks = new List<DuckDefinition>();
}