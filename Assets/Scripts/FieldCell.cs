using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FieldCell : MonoBehaviour
{
	public enum CellType
	{
		NULL,
		EMPTY,
		OBSTACLE,
		START,
		FINISH,
		PATH
	}

	[System.Serializable]
	public struct Parameters
	{
		public int x;
		public int y;
		public CellType cellType;
	}

	public Color[] colors;

	public Image cellImage;

	public Parameters parameters;

	public bool IsObstacle()
	{
		return parameters.cellType == CellType.OBSTACLE;
	}

	public void Initialize(int x, int y)
	{
		parameters.x = x;
		parameters.y = y;

		if (parameters.cellType == CellType.NULL)
			SetType(CellType.EMPTY);
		else
			SetType(parameters.cellType);
	}

	public void CellClick()
	{
		if (Input.GetMouseButton(0))
		{
			SetType(GameManager.Instance.currentCellTypeDraw);
		}
	}

	public void SetType(CellType newType = CellType.NULL)
	{
		switch (newType)
		{
			case CellType.START:
				if (GameManager.Instance.fieldManager.fieldCellStart != null)
					GameManager.Instance.fieldManager.fieldCellStart.SetType(CellType.EMPTY);
				GameManager.Instance.fieldManager.fieldCellStart = this;
				if (GameManager.Instance.fieldManager.fieldCellFinish == this)
					GameManager.Instance.fieldManager.fieldCellFinish = null;
				break;
			case CellType.FINISH:
				if (GameManager.Instance.fieldManager.fieldCellFinish != null)
					GameManager.Instance.fieldManager.fieldCellFinish.SetType(CellType.EMPTY);
				GameManager.Instance.fieldManager.fieldCellFinish = this;
				if (GameManager.Instance.fieldManager.fieldCellStart == this)
					GameManager.Instance.fieldManager.fieldCellStart = null;
				break;
			case CellType.OBSTACLE:
			case CellType.EMPTY:
				if (this.parameters.cellType == CellType.START)
					GameManager.Instance.fieldManager.fieldCellStart = null;
				if (this.parameters.cellType == CellType.FINISH)
					GameManager.Instance.fieldManager.fieldCellFinish = null;
				break;
			case CellType.NULL:
				newType = GameManager.Instance.currentCellTypeDraw;
				break;
			case CellType.PATH:
			default:
				break;
		}

		parameters.cellType = newType;
		cellImage.color = colors[(int)parameters.cellType];
	}
}
