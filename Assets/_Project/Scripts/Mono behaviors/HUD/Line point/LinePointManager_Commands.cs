using QFSW.QC;

public partial class LinePointManager
{
    [Command("HUD_LinePoint_Play")]
    private void Debug_LinePoint (string key)
    {
        ShowFeedback(key);
    }
}