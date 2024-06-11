using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Snake
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            {GridValue.Empty, Images.Empty },
            {GridValue.Snake, Images.Body },
            {GridValue.Food, Images.Food },
            {GridValue.SuperFood, Images.SuperFood },
            {GridValue.AntiFood, Images.AntiFood },
            {GridValue.Obstacle, Images.Obstacle },
        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            {Direction.Up, 0},
            {Direction.Down, 180},
            {Direction.Left, 270},
            {Direction.Right, 90},
        };

        private readonly int rows = 20, cols = 20;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        private HeuristicSolver heuristicSolver;
        private bool heuristicRunning;
        private bool KeyLock = false;
        private bool soundsOn = true;
        private bool pause = false;

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols, soundsOn);
            heuristicSolver = new HeuristicSolver(gameState);
            PauseOverlay.Visibility = Visibility.Hidden;
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols, soundsOn);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            if (pause)
            {
                pause = false;
                PauseOverlay.Visibility = Visibility.Hidden;
                return;
            }

            if (e.Key == Key.H)
            {
                KeyLock = !KeyLock;
                heuristicRunning = KeyLock;
                heuristicSolver.ToggleRunning();
                return;
            }

            if (e.Key == Key.M)
            {
                soundsOn = !soundsOn;
                gameState.SetSoundsOn(soundsOn);
                return;
            }

            if (e.Key == Key.Escape)
            {
                pause = true;
                PauseOverlay.Visibility = Visibility.Visible;
                return;
            }

            if (KeyLock)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.A:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                case Key.D:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                case Key.W:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                case Key.S:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                while (pause)
                {
                    await Task.Delay(100);
                }

                if (heuristicRunning)
                {
                    await Task.Delay(100);
                    Direction nextMove = heuristicSolver.GetNextMove();
                    gameState.ChangeDirection(nextMove);
                }
                else
                {
                    await Task.Delay(100);
                }

                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5),
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Column];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePosition());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Column].Source = source;
                await Task.Delay(50);
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }

            ReloadHeuristic();
        }

        private async Task ShowGameOver()
        {
            if (soundsOn)
            {
                Sounds.GameOverSound.Play();
            }

            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO START";
        }

        private void ReloadHeuristic()
        {
            heuristicSolver = new HeuristicSolver(gameState);
        }
    }
}