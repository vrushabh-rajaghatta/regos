namespace RegOS.Labeling.Domain.Aggregates.Contraindications;

public static class ContraindicationErrors
{
    public static readonly string LabelTextTooLong =
        $"Label text must be {Contraindication.LabelTextMaxLength} characters or fewer.";
}
