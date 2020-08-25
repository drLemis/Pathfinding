using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Diagnostics;

public static class Pathfinding
{
	public struct PathData
	{
		public List<CellData> path;
		public float time;
		public double distance;
	}

	public enum DistanceType
	{
		MANHATTAN,
		EUCLIDIAN
	}

	// info about every virtual cell
	public class CellData
	{
		public FieldCell cell;
		public CellData lastCell;
		public double scoreGuess;
		public double score;
		public int index;

		public CellData(FieldCell cell)
		{
			this.cell = cell;
		}
	}


	// messy but pretty efficient
	private static List<CellData> GetNeighbours(CellData cellData, List<List<FieldCell>> grid)
	{
		List<CellData> result = new List<CellData>(8);

		int x = cellData.cell.parameters.x;
		int y = cellData.cell.parameters.y;

		int indexNorthY = y - 1;
		int indexSouthY = y + 1;
		int indexEastX = x + 1;
		int indexWestX = x - 1;

		// straight 0+1
		if (indexNorthY >= 0 && !grid[indexNorthY][x].IsObstacle())
			result.Add(new CellData(grid[indexNorthY][x]));
		if (indexEastX < grid[0].Count && !grid[y][indexEastX].IsObstacle())
			result.Add(new CellData(grid[y][indexEastX]));
		if (indexSouthY < grid.Count && !grid[indexSouthY][x].IsObstacle())
			result.Add(new CellData(grid[indexSouthY][x]));
		if (indexWestX >= 0 && !grid[y][indexWestX].IsObstacle())
			result.Add(new CellData(grid[y][indexWestX]));

		// diagonals 1+1
		if (indexEastX < grid[0].Count)
		{
			if (indexNorthY >= 0 && !grid[indexNorthY][indexEastX].IsObstacle())
				result.Add(new CellData(grid[indexNorthY][indexEastX]));
			if (indexSouthY < grid.Count && !grid[indexSouthY][indexEastX].IsObstacle())
				result.Add(new CellData(grid[indexSouthY][indexEastX]));
		}

		if (indexWestX >= 0)
		{
			if (indexNorthY >= 0 && !grid[indexNorthY][indexWestX].IsObstacle())
				result.Add(new CellData(grid[indexNorthY][indexWestX]));
			if (indexSouthY < grid.Count && !grid[indexSouthY][indexWestX].IsObstacle())
				result.Add(new CellData(grid[indexSouthY][indexWestX]));
		}

		return result;
	}

	private static double Distance(CellData start, CellData end)
	{
		switch (GameManager.Instance.pathfindingDistanceType)
		{
			case DistanceType.EUCLIDIAN:
				return DistanceEuclid(start, end);
			case DistanceType.MANHATTAN:
			default:
				return DistanceManhattan(start, end);
		}
	}

	// prioritize straight movement, slower but better for grids
	private static double DistanceManhattan(CellData start, CellData end)
	{
		return Math.Abs(start.cell.parameters.x - end.cell.parameters.x) + Math.Abs(start.cell.parameters.y - end.cell.parameters.y);
	}

	// prioritize diagonal movement, faster but ugly on grid
	private static double DistanceEuclid(CellData start, CellData end)
	{
		var x = start.cell.parameters.x - end.cell.parameters.x;
		var y = start.cell.parameters.y - end.cell.parameters.y;

		return Math.Sqrt(x * x + y * y);
	}


	// https://en.wikipedia.org/wiki/A*_search_algorithm
	// not so great for concave places as it seems
	// wrote Dijkstra once, so maybe my implementation somewhere in between
	public static PathData FindPath(List<List<FieldCell>> grid, FieldCell startCell, FieldCell finishCell)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		PathData pathData = new PathData();
		pathData.path = new List<CellData>();
		pathData.distance = DistanceEuclid(new CellData(startCell), new CellData(finishCell));

		// works much better from left to right, so I'm just reversing points in this case
		// TODO: check top-down
		bool wasReversed = false;
		if (startCell.parameters.x > finishCell.parameters.x)
		{
			FieldCell temp = startCell;
			startCell = finishCell;
			finishCell = temp;
			wasReversed = true;
		}

		int cols = grid[0].Count;
		int rows = grid.Count;
		int maxDistanceEver = cols * rows;

		// open grid nodes
		List<CellData> cellsTesting = new List<CellData>();
		cellsTesting.Add(new CellData(startCell));
		cellsTesting[0].scoreGuess = 0;
		cellsTesting[0].score = 0;
		cellsTesting[0].index = startCell.parameters.x + startCell.parameters.y * cols;

		List<int> checkedCells = new List<int>();

		double distanceS;
		double distanceE;

		double maxDistance;
		int minIndex;

		List<CellData> neighboursCell;
		CellData targetCell;

		CellData endCell = new CellData(finishCell);
		endCell.index = finishCell.parameters.x + finishCell.parameters.y * cols;

		do
		{
			// safety measure
			if (checkedCells.Count > maxDistanceEver)
			{
				stopwatch.Stop();
				pathData.time = stopwatch.ElapsedMilliseconds;

				return pathData;
			}

			maxDistance = maxDistanceEver;
			minIndex = 0;

			// narrowing search to direction
			for (int i = 0; i < cellsTesting.Count; i++)
			{
				if (cellsTesting[i].scoreGuess < maxDistance)
				{
					maxDistance = cellsTesting[i].scoreGuess;
					minIndex = i;
				}
			}

			// got further than finish
			if (minIndex >= cellsTesting.Count)
			{
				stopwatch.Stop();
				pathData.time = stopwatch.ElapsedMilliseconds;
				return pathData;
			}

			CellData current = cellsTesting[minIndex];
			cellsTesting.RemoveAt(minIndex);

			// not on finish yet
			if (current.index != endCell.index)
			{
				neighboursCell = GetNeighbours(current, grid);

				for (int i = 0; i < neighboursCell.Count; ++i)
				{
					targetCell = neighboursCell[i];
					targetCell.lastCell = current;
					targetCell.scoreGuess = 0;
					targetCell.score = 0;
					targetCell.index = targetCell.cell.parameters.x + targetCell.cell.parameters.y * cols;

					bool alreadyTested = false;

					foreach (int key in checkedCells)
					{
						if (targetCell.index == key)
						{
							alreadyTested = true;
							break;
						}
					}

					if (!alreadyTested)
					{
						distanceS = Distance(targetCell, current);
						distanceE = Distance(targetCell, endCell);

						// heuristic relaxing
						// TODO: add actual weight instead of score, to simulate difficult path like water or snow 
						targetCell.score = current.score + distanceS;
						targetCell.scoreGuess = targetCell.score + distanceE;

						cellsTesting.Add(targetCell);
						checkedCells.Add(targetCell.index);
					}
				}
			}
			// got on finish, no need to check further
			else
			{
				do
				{
					pathData.path.Add(current);
					current = current.lastCell;
				}
				while (current != null);

				if (!wasReversed)
					pathData.path.Reverse();

				stopwatch.Stop();
				pathData.time = stopwatch.ElapsedMilliseconds;

				return pathData;
			}
		}
		while (true);
	}
}