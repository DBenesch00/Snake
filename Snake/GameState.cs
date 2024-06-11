using Snake;
using static Snake.MainWindow;
using static System.Formats.Asn1.AsnWriter;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePosition = new LinkedList<Position>();
        private readonly Random random = new Random();
        int count = 0;
        private bool soundsOn;

        // Erschaft alles
        public GameState(int rows, int columns, bool initialSoundsOn)
        {
            Rows = rows;
            Columns = columns;
            Grid = new GridValue[Rows, Columns];
            Dir = Direction.Right;
            soundsOn = initialSoundsOn;

            AddSnake();
            AddFood();
        }

        public void SetSoundsOn(bool isOn)
        {
            soundsOn = isOn;
        }

        // Schafft die Schlange
        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePosition.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> ObstaclePositon()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Obstacle)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        // Schafft die Hindernisse
        private void AddObstacles()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            List<Position> obstaclesPositions = new List<Position>(ObstaclePositon());

            int obstacleCount = obstaclesPositions.Count;

            // Bestimme, wie viele Hindernisse basierend auf dem aktuellen Score hinzugefügt oder entfernt werden sollen
            int targetObstacleCount = Score / 4;

            // Wenn die Anzahl der Hindernisse erhöht werden soll, füge ein Hindernis hinzu
            while (obstacleCount < targetObstacleCount)
            {
                Position pos = empty[random.Next(empty.Count)];
                Grid[pos.Row, pos.Column] = GridValue.Obstacle;
                // Entferne die Position aus der Liste der leeren Positionen
                empty.Remove(pos);
                // Füge die Position zur Liste der Hindernispositionen hinzu
                obstaclesPositions.Add(pos);
                obstacleCount++;
            }

            // Wenn die Anzahl der Hindernisse verringert werden soll, entferne ein Hindernis
            while (obstacleCount > targetObstacleCount)
            {
                // Wähle ein zufälliges Hindernis aus den aktuellen Hindernissen
                Position obstacleToRemove = obstaclesPositions[random.Next(obstaclesPositions.Count)];
                Grid[obstacleToRemove.Row, obstacleToRemove.Column] = GridValue.Empty;
                // Füge die Position zur Liste der leeren Positionen hinzu
                empty.Add(obstacleToRemove);
                // Entferne die Position aus der Liste der Hindernispositionen
                obstaclesPositions.Remove(obstacleToRemove);
                obstacleCount--;
            }
        }



        // Schafft das Essen
        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            double randomNumber = random.NextDouble();


            if (randomNumber < 0.25)
            {
                Grid[pos.Row, pos.Column] = GridValue.SuperFood;
            }
            else if (randomNumber < 0.5)
            {
                Grid[pos.Row, pos.Column] = GridValue.AntiFood;
            }
            else
            {
                Grid[pos.Row, pos.Column] = GridValue.Food;
            }
        }



        // Bestimmt, wo der Kopf ist
        public Position HeadPosition()
        {
            return snakePosition.First.Value;
        }

        // Bestimmt, wo der Schwanz ist
        public Position TailPosition()
        {
            return snakePosition.Last.Value;
        }

        public IEnumerable<Position> SnakePosition()
        {
            return snakePosition;
        }

        private void AddHead(Position pos)
        {
            snakePosition.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePosition.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePosition.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }

            return dirChanges.Last.Value;
        }

        // prüft ob die Richtung geändert werden kann
        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            // nur wenn der Richtungswechsel möglich ist wird diese gewechselt
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Column < 0 || pos.Column >= Columns;
        }

        // prüft was getroffen wird im nächsten Move
        public GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Column];
        }

        public void Move()
        {
            if (dirChanges.Count != 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            // GameOver wenn man sich selbst oder den Rand trifft
            if (hit == GridValue.Outside || hit == GridValue.Snake || hit == GridValue.Obstacle || hit == GridValue.AntiFood && Score == -2)
            {
                GameOver = true;
            }
            // normales Movement
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            // Wenn Frucht getroffen wird, wird einfach an der Stelle ein neuer Kopf erschaffen und neues Essen wird erschaffen
            else if (hit == GridValue.Food)
            {
                if (soundsOn)
                {
                    Sounds.EatSound.Play();
                }

                AddHead(newHeadPos);
                Score++;
                AddFood();
                count++;
                AddObstacles();
            }
            // Wenn das SuperFood getroffen wird werden zwei neue Köpfe erschaffen und neues Essen wird erschaffen
            else if (hit == GridValue.SuperFood)
            {
                if (soundsOn)
                {
                    Sounds.EatSound.Play();
                }

                AddHead(newHeadPos);
                AddHead(newHeadPos);
                Score = Score + 2;
                AddFood();
                count = count + 2;
                AddObstacles();
            }
            // Wenn das AntiFood getroffen wird wird der Schwanz entfernt und der Score um 1 verringert
            else if (hit == GridValue.AntiFood)
            {
                if (soundsOn)
                {
                    Sounds.EatSound.Play();
                }

                RemoveTail();
                RemoveTail();
                AddHead(newHeadPos);
                Score--;
                AddFood();
                count--;
                AddObstacles();
            }
        }

    }
}