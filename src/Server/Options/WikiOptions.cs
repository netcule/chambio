namespace Chambio.Server.Options;

public class WikiOptions
{
    public string[]? ExcludedCategories { get; set; }

    public Dictionary<string, string>? NationalReferrals { get; set; }

    public Dictionary<string, string>? InternationalReferrals { get; set; }
}
