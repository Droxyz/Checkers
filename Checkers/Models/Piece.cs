namespace Checkers.Models
{
    public class Piece
    {
        public PieceType Type { get; private set; }
        public bool IsKing { get; set; }

        public Piece(PieceType type)
        {
            Type = type;
            IsKing = false;
        }
    }

    // Enum is basically like string or bool, but with predifined values
    // Makes it more readable and easier to use 
    public enum PieceType
    {
        Black,
        White
    }
}