namespace Checkers.UI
{
    using Checkers.GameEngine;
    using Checkers.Models;
    using System.Windows.Forms;
    using System.Drawing;

    public partial class Form1 : Form
    {
        private GameEngine gameEngine;
        private Button[,] buttons;
        private Panel boardPanel;
        private int[]? selectedPiecePos;
        private string errorMessage;
        private TextBox errorTextBox;
        private Button endTurnBtn;


        public Form1()
        {
            InitializeComponent();
            gameEngine = new GameEngine();
            InitializeBoardUI(); 
        }

        private void InitializeBoardUI()
        {
            int buttonSize = 75;
            int outerPadding = 1;

            boardPanel = new Panel
            {
                Width = buttonSize * Board.BoardSize + outerPadding*2,
                Height = buttonSize * Board.BoardSize + outerPadding*2,
                BackColor = Color.LightSlateGray,
                Location = new Point(0,0),
            };

            buttons = new Button[Board.BoardSize, Board.BoardSize];

            gameEngine.TraverseBoard((row, col) =>
            {
                int xPosition = col * (buttonSize);
                int yPosition = row * (buttonSize);

                Button btn = new Button
                {
                    Width = buttonSize,
                    Height = buttonSize,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Location = new Point(xPosition + outerPadding, yPosition + outerPadding),
                    Font = new Font(FontFamily.GenericMonospace, buttonSize / 5),
                };
                
                if ((row + col) % 2 == 1)
                {
                    btn.BackColor = Color.BurlyWood;  
                }
                else
                {
                    btn.BackColor = Color.White;
                }

                btn.Click += (s, e) => HandleBoardClick(row, col);
                this.Controls.Add(boardPanel);
                boardPanel.Controls.Add(btn);

                buttons[row, col] = btn;

                UpdateButton(row, col); 
            });

            int formWidth = Board.BoardSize * buttonSize + outerPadding*2 + 300;
            int formHeight = Board.BoardSize * buttonSize + outerPadding*2;
            this.ClientSize = new Size(formWidth, formHeight);

            errorTextBox = new TextBox
            {
                Location = new Point(Board.BoardSize * buttonSize + outerPadding * 2 + 20, 20),
                ForeColor = Color.Black,
                Width = 200,
                ReadOnly = true // Make it read-only to prevent user input
            };

            endTurnBtn = new Button
            {
                Width = buttonSize,
                Height = buttonSize,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Location = new Point(Board.BoardSize * buttonSize + outerPadding * 2 + 20, 50),
                Font = new Font(FontFamily.GenericMonospace, buttonSize / 5),
                Text = "End turn",
            };
            endTurnBtn.Click += EndTurn;

            this.Controls.Add(errorTextBox);
            this.Controls.Add(endTurnBtn);
        }


        // Update piece/button visually
        private void UpdateButton(int row, int col)
        {
            Piece piece = gameEngine.GameBoard.GetPieceAt(row, col);

            if (piece != null)
            {
                if (piece.Type == PieceType.Black)
                {
                    buttons[row, col].Text = piece.IsKing ? "BK" : "B";
                    buttons[row, col].ForeColor = Color.Black;
                }
                else
                {
                    buttons[row, col].Text = piece.IsKing ? "WK" : "W";
                    buttons[row, col].ForeColor = Color.White;
                }
                
            }
            else
            {
                buttons[row, col].Text = "";  // Empty square
            }
        }

        private void HandleBoardClick(int row, int col)
        {
            Piece clickedPiece = gameEngine.GameBoard.GetPieceAt(row, col);

            if (selectedPiecePos != null)
            {
                int selectedRow = selectedPiecePos[0];
                int selectedCol = selectedPiecePos[1];

                // Deselect the piece if the same position is clicked again
                if (selectedRow == row && selectedCol == col)
                {
                    DeselectPiece(selectedRow, selectedCol);
                    return;
                }

                if (gameEngine.ProcessMove(selectedRow, selectedCol, row, col, out errorMessage))
                {
                    UpdateButton(selectedRow, selectedCol);
                    UpdateButton(row, col); 
                    foreach (var piecePos in gameEngine.piecesAlongPathPositions)
                    {
                        int pieceRow = piecePos[0];
                        int pieceCol = piecePos[1];
                        UpdateButton(pieceRow, pieceCol);
                    }
                    DeselectPiece(selectedRow, selectedCol);
                }

                errorTextBox.Text = errorMessage;

            }
            else if (clickedPiece != null)
            {
                SelectPiece(row, col);
                errorTextBox.Text = string.Empty; 
            }
        }

        private void SelectPiece(int row, int col)
        {
            buttons[row, col].FlatStyle = FlatStyle.Flat;
            buttons[row, col].FlatAppearance.BorderSize = 1;  // Set the border size
            buttons[row, col].FlatAppearance.BorderColor = Color.Red;  // Set the border color to red
            selectedPiecePos = new int[] { row, col }; 
        }

        private void DeselectPiece(int row, int col)
        {
            buttons[row, col].FlatStyle = FlatStyle.Flat;
            buttons[row, col].FlatAppearance.BorderSize = 0;  // Reset the border size
            buttons[row, col].FlatAppearance.BorderColor = Color.Black;  // Set the border color back to black
            selectedPiecePos = null;
        }

        private void EndTurn(object sender, EventArgs e)
        {
            gameEngine.CurrentPlayer = (gameEngine.CurrentPlayer == PieceType.Black) ? PieceType.White : PieceType.Black;
        }
    }
}