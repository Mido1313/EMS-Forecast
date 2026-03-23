namespace WetterdatenImporter.Services;

public sealed class ImportRunSummary
{
    public int MeasurementPointsProcessed { get; set; }
    public int Saved { get; private set; }
    public int Skipped { get; private set; }
    public int Errors { get; private set; }

    public void IncrementSaved()
    {
        Saved++;
    }

    public void IncrementSkipped()
    {
        Skipped++;
    }

    public void IncrementErrors()
    {
        Errors++;
    }
}
