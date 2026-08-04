namespace RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

public static class UndesirableEffectErrors
{
    public static readonly string LabelTextTooLong =
        $"Label text must be {UndesirableEffect.LabelTextMaxLength} characters or fewer.";

    public const string FrequencyNotRecognised =
        "That frequency is not recognised.";
}
