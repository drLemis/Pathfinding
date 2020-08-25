using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
	private static readonly GameManager instance = new GameManager();

	public static GameManager Instance
	{
		get { return instance; }
	}

	protected GameManager() { }

	// ########

	public FieldManager fieldManager;
	public FieldCell.CellType currentCellTypeDraw;
	public Pathfinding.DistanceType pathfindingDistanceType;
}
