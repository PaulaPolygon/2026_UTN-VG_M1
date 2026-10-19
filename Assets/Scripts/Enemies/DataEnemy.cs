using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Bunny Hazard/Enemies/Enemy Data")]
public class DataEnemy : ScriptableObject
{
	public string nombreEnemigo;
	public TipoArmaEnemigo arma;
	public int vidasQueQuita;
}

public enum TipoArmaEnemigo
{
	Escopeta,
	Cuchillo,
	CollarElectrico,
	Jeringa
}
