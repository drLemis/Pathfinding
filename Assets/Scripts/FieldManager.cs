using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
	public Transform fieldTransform;
	public Dropdown distanceDropdown;
	public Text dataInfo;

	[HideInInspector]
	public List<List<FieldCell>> fieldCellMap;
	[HideInInspector]
	public FieldCell fieldCellStart;
	[HideInInspector]
	public FieldCell fieldCellFinish;
	[HideInInspector]
	public Pathfinding.PathData pathData;

	void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		GameManager.Instance.fieldManager = this;
		GameManager.Instance.currentCellTypeDraw = FieldCell.CellType.OBSTACLE;
		GameManager.Instance.pathfindingDistanceType = Pathfinding.DistanceType.MANHATTAN;

		fieldCellMap = new List<List<FieldCell>>();

		for (int y = 0; y < fieldTransform.childCount; y++)
		{
			Transform row = fieldTransform.GetChild(y);
			List<FieldCell> rowCells = new List<FieldCell>();

			for (int x = 0; x < row.childCount; x++)
			{
				FieldCell cell = row.GetChild(x).GetComponent<FieldCell>();
				cell.Initialize(x, y);
				rowCells.Add(cell);
			}

			fieldCellMap.Add(rowCells);
		}

		fieldCellStart.SetType(FieldCell.CellType.START);
		fieldCellFinish.SetType(FieldCell.CellType.FINISH);
	}

	public void SyncDropdownDistanceMode()
	{
		GameManager.Instance.pathfindingDistanceType = (Pathfinding.DistanceType)distanceDropdown.value;
	}

	private void SyncDataInfo(bool clear = false)
	{
		if (clear)
			dataInfo.text = "Distance: 0,00\nPath length: 0\nTime: 0ms";
		else
			dataInfo.text = "Distance: " + pathData.distance.ToString("0.00") + "\nPath length: " + pathData.path.Count + "\nTime: " + pathData.time + "ms";
	}

	public void SetCurrentCellDraw(int newType)
	{
		SetCurrentCellDraw((FieldCell.CellType)newType);
	}

	public void SetCurrentCellDraw(FieldCell.CellType newType)
	{
		GameManager.Instance.currentCellTypeDraw = newType;
	}

	public void FindPath()
	{
		if (pathData.path != null && pathData.path.Count > 0)
		{
			foreach (Pathfinding.CellData cellData in pathData.path)
			{
				if (cellData.cell.parameters.cellType == FieldCell.CellType.PATH)
					cellData.cell.SetType(FieldCell.CellType.EMPTY);
			}
		}

		if (fieldCellStart != null && fieldCellFinish != null && fieldCellMap.Count > 0 && fieldCellMap[0].Count > 0)
		{
			pathData = Pathfinding.FindPath(fieldCellMap, fieldCellStart, fieldCellFinish);

			foreach (Pathfinding.CellData cellData in pathData.path)
			{
				if (cellData.cell.parameters.cellType == FieldCell.CellType.EMPTY)
					cellData.cell.SetType(FieldCell.CellType.PATH);
			}

			SyncDataInfo();
		}
		else
			SyncDataInfo(true);
	}
}
