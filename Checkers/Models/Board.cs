namespace Checkers.Models
{
    public class Board
    {
        public Piece[,] Grid { get; private set; }
        public const int BoardSize = 8;

        public Board()
        {
            Grid = new Piece[BoardSize, BoardSize];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Set up the pieces for the game
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (row < 3 && (row + col) % 2 != 0)  // Black pieces on first 3 rows
                    {
                        Grid[row, col] = new Piece(PieceType.Black);
                    }
                    else if (row > 4 && (row + col) % 2 != 0)  // White pieces on last 3 rows
                    {
                        Grid[row, col] = new Piece(PieceType.White);
                    }
                    else
                    {
                        Grid[row, col] = null;
                    }
                }
            }
        }

        public void MovePiece(int startX, int startY, int endX, int endY)
        {
            Piece piece = Grid[startX, startY];
            Grid[endX, endY] = piece;  // Place piece in new location
            Grid[startX, startY] = null;  // Empty the previous spot
        }

        public void RemovePiece(int row, int col)
        {
            Grid[row, col] = null;
        }

        // Accessor to get the piece at a specific position
        public Piece GetPieceAt(int row, int col)
        {
            return Grid[row, col];
        }
    }
}
