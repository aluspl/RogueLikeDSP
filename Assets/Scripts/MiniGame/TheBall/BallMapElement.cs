namespace MiniGame.TheBall
{
    public enum BallMapElement
    {



        /*
           0. Empty
           1. Horizontal Wall
           2. Vertical Wall
           3-6 Corner
           7. End Game
           8. Start Game
              */
        //THeBall Elements
        WallVertical=2,
        WallHorizontal=1,
        BottomLeftCorner=3,
        BottomRightCorner=4,
        TopLeftCorner=6,
        TopRightCorner=5,
        Empty = 0,
        FinishPoint=7,
        StartPoint=8,
    }
}