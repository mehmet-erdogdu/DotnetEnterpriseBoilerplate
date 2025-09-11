namespace BlogApp.UnitTests;

[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(LicenseAcknowledgmentInitializer),
    nameof(LicenseAcknowledgmentInitializer.AcknowledgeSoftWarning))]
public static class LicenseAcknowledgmentInitializer
{
    public static void AcknowledgeSoftWarning()
    {
        FluentAssertions.License.Accepted = true;
    }
}
