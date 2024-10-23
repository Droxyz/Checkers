namespace Checkers.GameEngine
{
    using Checkers.Models;

    public class GameEngine
    {
        public Board GameBoard { get; private set; }
        public bool IsGameOver { get; private set; }
        public PieceType CurrentPlayer { get; set; }

        public List<int[]> piecesAlongPathPositions = new List<int[]>();

        public GameEngine()
        {
            GameBoard = new Board();
            IsGameOver = false;
            CurrentPlayer = PieceType.White;
        }

        // Action (delegate) = void (return nothing). "Do stuff before (or after) do passed action"
        // <int,int> types required in passed function
        // action = passed function funcionality
        // Here we do action for each cell of board
        public void TraverseBoard(Action<int, int> action)
        {
            for (int row = 0; row < Board.BoardSize; row++)
            {
                for (int col = 0; col < Board.BoardSize; col++)
                {
                    action(row, col);
                }
            }
        }

        public bool ProcessMove(int startRow, int startCol, int endRow, int endCol, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (IsGameOver)
            {
                errorMessage = "The game is over.";
                return false;
            }

            Piece movingPiece = GameBoard.Grid[startRow, startCol];

            if (movingPiece != null && movingPiece.Type != CurrentPlayer)
            {
                errorMessage = $"It's {CurrentPlayer}'s turn";
                return false;
            }

            if (IsValidMove(startRow, startCol, endRow, endCol, out string moveError))
            {
                GameBoard.MovePiece(startRow, startCol, endRow, endCol);
                HandlePostMoveLogic(endRow, endCol);
                return true;
            }
            else
            {
                errorMessage = moveError;
                return false;
            }
        }

        private bool IsValidMove(int startRow, int startCol, int endRow, int endCol, out string errorMessage)
        {
            errorMessage = string.Empty;
            Piece piece = GameBoard.GetPieceAt(startRow, startCol);

            if (piece == null)
            {
                errorMessage = "No piece at the starting position.";
                return false;
            }

            int colDiff = endCol - startCol; // Horizontal (X-axis) movement
            int rowDiff = endRow - startRow; // Vertical (Y-axis) movement

            if (!IsDiagonalMove(colDiff, rowDiff, out errorMessage))
            {
                return false;
            }

            if (!IsValidDirection(piece, rowDiff, out errorMessage))
            {
                return false;
            }

            if (!IsEndPositionEmpty(endRow, endCol, out errorMessage))
            {
                return false;
            }

            getPath(startRow, startCol, colDiff, rowDiff);

            if (!PathAndDistanceValid(piece, startRow, rowDiff, out errorMessage))
            {
                return false;
            }

            return true;
        }

        private bool IsDiagonalMove(int colDiff, int rowDiff, out string errorMessage)
        {
            if (Math.Abs(colDiff) != Math.Abs(rowDiff))
            {
                errorMessage = "Move must be diagonal.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        private bool IsValidDirection(Piece piece, int rowDiff, out string errorMessage)
        {
            if (!piece.IsKing)
            {
                if (piece.Type == PieceType.White && rowDiff >= 0)
                {
                    errorMessage = "Regular white pieces can only move up the board.";
                    return false;
                }
                if (piece.Type == PieceType.Black && rowDiff <= 0)
                {
                    errorMessage = "Regular black pieces can only move down the board.";
                    return false;
                }
            }
            errorMessage = string.Empty;
            return true;
        }

        private bool IsEndPositionEmpty(int endRow, int endCol, out string errorMessage)
        {
            Piece destinationPiece = GameBoard.GetPieceAt(endRow, endCol);
            if (destinationPiece != null)
            {
                errorMessage = "End position must be empty.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        private void getPath(int startRow, int startCol, int colDiff, int rowDiff)
        {
            piecesAlongPathPositions.Clear();  // Clear any previous path

            int stepX = colDiff / Math.Abs(colDiff);  // +1 or -1 for direction
            int stepY = rowDiff / Math.Abs(rowDiff);

            for (int i = 1; i < Math.Abs(colDiff); i++)
            {
                int currentX = startCol + i * stepX;
                int currentY = startRow + i * stepY;
                Piece pieceOnPath = GameBoard.GetPieceAt(currentY, currentX);

                if (pieceOnPath != null)
                {
                    piecesAlongPathPositions.Add(new int[] { currentY, currentX });
                }
            }
        }

        private bool PathAndDistanceValid(Piece piece, int startRow, int rowDiff, out string errorMessage)
        {
            errorMessage = string.Empty;
            int distanceToEnd = Math.Abs(rowDiff);

            if (piecesAlongPathPositions.Count > 1)
            {
                errorMessage = "Can't jump over more than 1 piece";
                return false;
            }

            if (piecesAlongPathPositions.Count == 0)
            {
                if (distanceToEnd > 1 && !piece.IsKing)
                {
                    errorMessage = "Non-king pieces can't move that far!";
                    return false;
                }
            }

            if (piecesAlongPathPositions.Count == 1)
            {
                int[] nextPiecePosition = piecesAlongPathPositions[0];
                int nextPieceRow = nextPiecePosition[0];
                int distanceToNextPiece = Math.Abs(startRow - nextPieceRow);
                

                if (distanceToNextPiece != 1 && !piece.IsKing)
                {
                    errorMessage = "Non-king pieces must jump exactly 1 square over an opponent.";
                    return false;
                }

                if (distanceToEnd - distanceToNextPiece != 1)
                {
                    errorMessage = "You can only capture a piece by jumping 1 square behind it";
                    return false;
                }
            }


            return true;
        }

        private void HandlePostMoveLogic(int row, int col)
        {
            removePathPieces();

            Piece piece = GameBoard.GetPieceAt(row, col);
            if (ShouldBeKinged(row, col, piece)) 
            {
                piece.IsKing = true;
            }

            if (!PieceCanCapture(row, col))
            {
                CurrentPlayer = CurrentPlayer == PieceType.Black ? PieceType.White : PieceType.Black;
            }
        }

        private bool PieceCanCapture(int row, int col)
        {
            Piece movedPiece = GameBoard.Grid[row, col];

            int boardSize = Board.BoardSize - 1; // -1 because positions start at 0

            string errormessage = "";

            if (!movedPiece.IsKing)
            {
                int[,] directions = { { -2, -2 }, { -2, 2 }, { 2, -2 }, { 2, 2 } }; // All possible jumps (top-left, top-right, bottom-left, bottom-right)

                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    int newRow = row + directions[i, 0];
                    int newCol = col + directions[i, 1];

                    // If in bounds
                    if (newRow >= 0 && newRow <= boardSize && newCol >= 0 && newCol <= boardSize)
                    {
                        int midRow = (row + newRow) / 2;
                        int midCol = (col + newCol) / 2;
                        Piece middlePiece = GameBoard.GetPieceAt(midRow, midCol);

                        if (IsValidMove(row, col, newRow, newCol, out errormessage))
                        {
                            return true;

                        }
                    }
                }
            }
            else 
            {
                int[,] directions = { { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } };  // All diagonal directions

                for (int i = 1; i <= boardSize; i++)  // Kings can move multiple squares
                {
                    for (int j = 0; j < directions.GetLength(0); j++)
                    {
                        int newRow = row + i * directions[j, 0];
                        int newCol = col + i * directions[j, 1];
                        if (newRow >= 0 && newRow <= boardSize && newCol >= 0 && newCol <= boardSize)
                        {
                            if (IsValidMove(row, col, newRow, newCol, out errormessage))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        // Method to determine if the piece should be kinged (when it reaches the opponent's side)
        private bool ShouldBeKinged(int row, int col, Piece piece)
        {
            if (!piece.IsKing && piece.Type == PieceType.White && row == 0)  // White reaches the top
            {
                return true;
            }
            if (!piece.IsKing && piece.Type == PieceType.Black && row == Board.BoardSize - 1)  // Black reaches the bottom
            {
                return true;
            }
            return false;
        }

        private void removePathPieces()
        {
            for (int i = 0; i < piecesAlongPathPositions.Count; i++)
            {
                int[] position = piecesAlongPathPositions[i];
                int row = position[0];
                int col = position[1];

                GameBoard.RemovePiece(row, col);
            }
        }
    }
}