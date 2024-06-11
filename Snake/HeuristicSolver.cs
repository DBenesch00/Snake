using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Snake
{
    public class HeuristicSolver
    {
        private readonly GameState gameState;
        private bool running;

        public HeuristicSolver(GameState gameState)
        {
            this.gameState = gameState;
            this.running = false;
        }

        public void ToggleRunning()
        {
            running = !running;
        }

        public bool IsRunning()
        {
            return running;
        }

        public Direction GetNextMove()
        {
            Position head = gameState.HeadPosition();
            Position tail = gameState.TailPosition();
            List<Direction> possibleDirections = new List<Direction>
            {
                Direction.Up,
                Direction.Down,
                Direction.Left,
                Direction.Right
            };

            // Filter out unsafe directions
            possibleDirections = possibleDirections
                .Where(dir => IsSafeDirection(head, dir))
                .ToList();

            if (!possibleDirections.Any())
            {
                return gameState.Dir;
            }

            // Calculate heuristic values for possible moves
            var heuristics = possibleDirections
                .Select(dir => new { Direction = dir, Value = Heuristic(head.Translate(dir)) })
                .OrderBy(h => h.Value)
                .ToList();

            // Select the direction with the lowest heuristic value
            return heuristics.First().Direction;
        }

        private int Heuristic(Position cell)
        {
            int size = gameState.Rows * gameState.Columns * 2;
            int xMax = gameState.Columns - 1;
            int yMax = gameState.Rows - 1;
            Position head = gameState.HeadPosition();
            Position tail = gameState.TailPosition();
            List<Position> snake = gameState.SnakePosition().ToList();
            Position food = GetFoodPosition();

            if (!Adjacencies(head).Contains(cell))
            {
                return 0;
            }

            var pathToPoint = Search(cell, food, xMax, yMax, snake);

            if (pathToPoint != null)
            {
                var snakeAtPoint = Shift(snake, pathToPoint, true);

                foreach (var next in Difference(Adjacencies(food), snakeAtPoint))
                {
                    if (Search(next, tail, xMax, yMax, snakeAtPoint) != null)
                    {
                        return pathToPoint.Count;
                    }
                }
            }

            var pathToTail = Search(cell, tail, xMax, yMax, snake);

            if (pathToTail != null)
            {
                return size - pathToTail.Count;
            }

            return size * 2;
        }

        private List<Position> Search(Position start, Position end, int xMax, int yMax, List<Position> snake)
        {
            var queue = new Queue<Position>();
            var paths = new Dictionary<Position, List<Position>>();
            queue.Enqueue(start);
            paths[start] = new List<Position> { start };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var snakeShifted = Shift(snake, paths[current]);

                if (current.Equals(end))
                {
                    return paths[current];
                }

                foreach (var next in Difference(Adjacencies(current), snakeShifted))
                {
                    if (!paths.ContainsKey(next))
                    {
                        queue.Enqueue(next);
                        paths[next] = new List<Position>(paths[current]) { next };
                    }
                }
            }

            return null;
        }

        private List<Position> Adjacencies(Position a)
        {
            var adj = new List<Position>
            {
                new Position(a.Row, a.Column - 1),
                new Position(a.Row + 1, a.Column),
                new Position(a.Row, a.Column + 1),
                new Position(a.Row - 1, a.Column)
            };

            return adj.Where(b => b.Row >= 0 && b.Column >= 0 && b.Row < gameState.Rows && b.Column < gameState.Columns).ToList();
        }

        private bool Equals(Position a, Position b)
        {
            return a.Row == b.Row && a.Column == b.Column;
        }

        private bool Includes(List<Position> a, Position b)
        {
            return a.Any(pos => Equals(pos, b));
        }

        private List<Position> Difference(List<Position> a, List<Position> b)
        {
            return a.Where(pos => !Includes(b, pos)).ToList();
        }

        private List<Position> Shift(List<Position> a, List<Position> b, bool collect = false)
        {
            var shifted = b.Concat(a).Take(b.Count + (a.Count - b.Count + (collect ? 1 : 0))).ToList();
            return shifted;
        }

        private bool IsSafeDirection(Position head, Direction dir)
        {
            Position newPosition = head.Translate(dir);
            GridValue hit = gameState.WillHit(newPosition);
            return hit != GridValue.Outside && hit != GridValue.Snake && hit != GridValue.Obstacle;
        }

        private Position GetFoodPosition()
        {
            for (int r = 0; r < gameState.Rows; r++)
            {
                for (int c = 0; c < gameState.Columns; c++)
                {
                    if (gameState.Grid[r, c] == GridValue.Food ||
                        gameState.Grid[r, c] == GridValue.SuperFood ||
                        gameState.Grid[r, c] == GridValue.AntiFood)
                    {
                        return new Position(r, c);
                    }
                }
            }
            return null;
        }
    }
}