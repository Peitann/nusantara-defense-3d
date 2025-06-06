using Godot;
using System.Collections.Generic;

public partial class PathGenerator : Node
{
	private int _loopCount;
	private PathGeneratorConfig pathConfig;
	private List<Vector2I> _pathRoute = new List<Vector2I>();

	public PathGeneratorConfig path_config => pathConfig;

	public PathGenerator()
	{
		pathConfig = GD.Load<PathGeneratorConfig>("res://resources/basic_path_config.res");
		GeneratePath(pathConfig.AddLoops);

		while (_pathRoute.Count < pathConfig.MinPathSize || 
			   _pathRoute.Count > pathConfig.MaxPathSize ||
			   _loopCount < pathConfig.MinLoops || 
			   _loopCount > pathConfig.MaxLoops)
		{
			GeneratePath(pathConfig.AddLoops);
		}
	}

	public void GeneratePath(bool addLoops = false)
	{
		_pathRoute.Clear();
		GD.Randomize();
		_loopCount = 0;
		
		int x = 0;
		int y = (int)(pathConfig.MapHeight / 2.0);
		
		while (x < pathConfig.MapLength)
		{
			if (!_pathRoute.Contains(new Vector2I(x, y)))
			{
				_pathRoute.Add(new Vector2I(x, y));
			}
			
			int choice = GD.RandRange(0, 2);

			if (choice == 0 || x < 2 || x % 2 == 0 || x == pathConfig.MapLength - 1)
			{
				x += 1;
			}
			else if (choice == 1 && y < pathConfig.MapHeight - 2 && !_pathRoute.Contains(new Vector2I(x, y + 1)))
			{
				y += 1;
			}
			else if (choice == 2 && y > 1 && !_pathRoute.Contains(new Vector2I(x, y - 1)))
			{
				y -= 1;
			}
		}
		
		if (addLoops)
		{
			_AddLoops();
		}
	}

	public int get_tile_score(int index)
	{
		int score = 0;
		int x = _pathRoute[index].X;
		int y = _pathRoute[index].Y;
		
		score += _pathRoute.Contains(new Vector2I(x, y - 1)) ? 1 : 0;
		score += _pathRoute.Contains(new Vector2I(x + 1, y)) ? 2 : 0;
		score += _pathRoute.Contains(new Vector2I(x, y + 1)) ? 4 : 0;
		score += _pathRoute.Contains(new Vector2I(x - 1, y)) ? 8 : 0;
		
		return score;
	}

	public Godot.Collections.Array get_path_route()
	{
		var godotArray = new Godot.Collections.Array();
		foreach (var point in _pathRoute)
		{
			godotArray.Add(point);
		}
		return godotArray;
	}

	public Vector2I get_path_tile(int index)
	{
		return _pathRoute[index];
	}

	private void _AddLoops()
	{
		bool loopsGenerated = true;
		
		while (loopsGenerated && _loopCount < pathConfig.MaxLoops)
		{
			loopsGenerated = false;
			for (int i = 0; i < _pathRoute.Count; i++)
			{
				var loop = _IsLoopOption(i);
				if (loop.Count > 0)
				{
					loopsGenerated = true;
					for (int j = 0; j < loop.Count; j++)
					{
						_pathRoute.Insert(i + 1 + j, loop[j]);
					}
					break; // Break to restart the loop check
				}
			}
		}
	}

	private List<Vector2I> _IsLoopOption(int index)
	{
		int x = _pathRoute[index].X;
		int y = _pathRoute[index].Y;
		var returnPath = new List<Vector2I>();

		// Yellow loop
		if (x < pathConfig.MapLength - 1 && y > 1 &&
			_TileLocFree(x, y - 3) && _TileLocFree(x + 1, y - 3) && _TileLocFree(x + 2, y - 3) &&
			_TileLocFree(x - 1, y - 2) && _TileLocFree(x, y - 2) && _TileLocFree(x + 1, y - 2) && _TileLocFree(x + 2, y - 2) && _TileLocFree(x + 3, y - 2) &&
			_TileLocFree(x - 1, y - 1) && _TileLocFree(x, y - 1) && _TileLocFree(x + 1, y - 1) && _TileLocFree(x + 2, y - 1) && _TileLocFree(x + 3, y - 1) &&
			_TileLocFree(x + 1, y) && _TileLocFree(x + 2, y) && _TileLocFree(x + 3, y) &&
			_TileLocFree(x + 1, y + 1) && _TileLocFree(x + 2, y + 1))
		{
			returnPath = new List<Vector2I>
			{
				new Vector2I(x + 1, y), new Vector2I(x + 2, y), new Vector2I(x + 2, y - 1),
				new Vector2I(x + 2, y - 2), new Vector2I(x + 1, y - 2), new Vector2I(x, y - 2), new Vector2I(x, y - 1)
			};

			if (index > 0 && _pathRoute[index - 1].Y > y)
			{
				returnPath.Reverse();
			}

			_loopCount += 1;
			returnPath.Add(new Vector2I(x, y));
		}
		// Blue loop
		else if (x > 2 && y > 1 &&
				 _TileLocFree(x, y - 3) && _TileLocFree(x - 1, y - 3) && _TileLocFree(x - 2, y - 3) &&
				 _TileLocFree(x - 1, y) && _TileLocFree(x - 2, y) && _TileLocFree(x - 3, y) &&
				 _TileLocFree(x + 1, y - 1) && _TileLocFree(x, y - 1) && _TileLocFree(x - 2, y - 1) && _TileLocFree(x - 3, y - 1) &&
				 _TileLocFree(x + 1, y - 2) && _TileLocFree(x, y - 2) && _TileLocFree(x - 1, y - 2) && _TileLocFree(x - 2, y - 2) && _TileLocFree(x - 3, y - 2) &&
				 _TileLocFree(x - 1, y + 1) && _TileLocFree(x - 2, y + 1))
		{
			returnPath = new List<Vector2I>
			{
				new Vector2I(x, y - 1), new Vector2I(x, y - 2), new Vector2I(x - 1, y - 2),
				new Vector2I(x - 2, y - 2), new Vector2I(x - 2, y - 1), new Vector2I(x - 2, y), new Vector2I(x - 1, y)
			};

			if (index > 0 && _pathRoute[index - 1].X > x)
			{
				returnPath.Reverse();
			}

			_loopCount += 1;
			returnPath.Add(new Vector2I(x, y));
		}
		// Red loop
		else if (x < pathConfig.MapLength - 1 && y < pathConfig.MapHeight - 2 &&
				 _TileLocFree(x, y + 3) && _TileLocFree(x + 1, y + 3) && _TileLocFree(x + 2, y + 3) &&
				 _TileLocFree(x + 1, y - 1) && _TileLocFree(x + 2, y - 1) &&
				 _TileLocFree(x + 1, y) && _TileLocFree(x + 2, y) && _TileLocFree(x + 3, y) &&
				 _TileLocFree(x - 1, y + 1) && _TileLocFree(x, y + 1) && _TileLocFree(x + 2, y + 1) && _TileLocFree(x + 3, y + 1) &&
				 _TileLocFree(x - 1, y + 2) && _TileLocFree(x, y + 2) && _TileLocFree(x + 1, y + 2) && _TileLocFree(x + 2, y + 2) && _TileLocFree(x + 3, y + 2))
		{
			returnPath = new List<Vector2I>
			{
				new Vector2I(x + 1, y), new Vector2I(x + 2, y), new Vector2I(x + 2, y + 1),
				new Vector2I(x + 2, y + 2), new Vector2I(x + 1, y + 2), new Vector2I(x, y + 2), new Vector2I(x, y + 1)
			};

			if (index > 0 && _pathRoute[index - 1].Y < y)
			{
				returnPath.Reverse();
			}

			_loopCount += 1;
			returnPath.Add(new Vector2I(x, y));
		}
		// Brown loop
		else if (x > 2 && y < pathConfig.MapHeight - 2 &&
				 _TileLocFree(x, y + 3) && _TileLocFree(x - 1, y + 3) && _TileLocFree(x - 2, y + 3) &&
				 _TileLocFree(x - 1, y - 1) && _TileLocFree(x - 2, y - 1) &&
				 _TileLocFree(x - 1, y) && _TileLocFree(x - 2, y) && _TileLocFree(x - 3, y) &&
				 _TileLocFree(x + 1, y + 1) && _TileLocFree(x, y + 1) && _TileLocFree(x - 2, y + 1) && _TileLocFree(x - 3, y + 1) &&
				 _TileLocFree(x + 1, y + 2) && _TileLocFree(x, y + 2) && _TileLocFree(x - 1, y + 2) && _TileLocFree(x - 2, y + 2) && _TileLocFree(x - 3, y + 2))
		{
			returnPath = new List<Vector2I>
			{
				new Vector2I(x, y + 1), new Vector2I(x, y + 2), new Vector2I(x - 1, y + 2),
				new Vector2I(x - 2, y + 2), new Vector2I(x - 2, y + 1), new Vector2I(x - 2, y), new Vector2I(x - 1, y)
			};

			if (index > 0 && _pathRoute[index - 1].X > x)
			{
				returnPath.Reverse();
			}

			_loopCount += 1;
			returnPath.Add(new Vector2I(x, y));
		}

		return returnPath;
	}

	private bool _TileLocTaken(int x, int y)
	{
		return _pathRoute.Contains(new Vector2I(x, y));
	}

	private bool _TileLocFree(int x, int y)
	{
		return !_pathRoute.Contains(new Vector2I(x, y));
	}

	public int GetLoopCount()
	{
		return _loopCount;
	}
}
