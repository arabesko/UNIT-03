public class MapModel
{
    public int CurrentPieces { get; private set; }
    public int TotalPieces { get; private set; }

    public bool HasStarted => CurrentPieces > 0;
    public bool IsComplete => CurrentPieces >= TotalPieces;

    public MapModel(int totalPieces)
    {
        TotalPieces = totalPieces;
        CurrentPieces = 0;
    }

    public void AddPiece()
    {
        if (!IsComplete)
        {
            CurrentPieces++;
        }
    }
}