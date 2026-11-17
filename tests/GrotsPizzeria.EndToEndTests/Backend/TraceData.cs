namespace GrotsPizzeria.EndToEndTests.Backend;

//// A trace as the backend has it: the spans that every application sent for it.

public sealed class TraceData(IReadOnlyList<TraceSpan> spans)
{
    public IEnumerable<TraceSpan> Roots => Spans.Where(s => ParentOf(s) is null);

    public IReadOnlyList<TraceSpan> Spans { get; } = spans;

    public IReadOnlySet<string> Services => Spans.Select(s => s.Service).ToHashSet();

    public TraceSpan? ParentOf(TraceSpan span) =>
        span.ParentSpanId is null ? null : Spans.FirstOrDefault(s => s.SpanId == span.ParentSpanId);

    public IReadOnlyList<TraceSpan> Ancestors(TraceSpan span)
    {
        var ancestors = new List<TraceSpan>();

        for (var parent = ParentOf(span); parent is not null; parent = ParentOf(parent))
        {
            ancestors.Add(parent);
        }

        return ancestors;
    }

    public bool IsComplete(IEnumerable<(string Service, string Route)> servers) =>
        Roots.Count() == 1 && servers.All(s => Spans.Any(x => x.IsServer && x.Service == s.Service && x.Attribute("http.route") == s.Route));

    public TraceSpan Server(string service, string route) =>
        Single($"server span of {service} for {route}", s => s.IsServer && s.Service == service && s.Attribute("http.route") == route);

    public TraceSpan Client(string service, string urlEndsWith) =>
        Single($"client span of {service} for a URL ending {urlEndsWith}", s => s.IsClient && s.Service == service && s.Attribute("url.full")?.EndsWith(urlEndsWith, StringComparison.Ordinal) is true);

    public string Describe() => string.Join(Environment.NewLine, Spans.OrderBy(s => s.Service).ThenBy(s => s.Name).Select(s => "  " + s));

    private TraceSpan Single(string what, Func<TraceSpan, bool> predicate)
    {
        var matches = Spans.Where(predicate).ToList();

        return matches.Count == 1
            ? matches[0]
            : throw new InvalidOperationException($"Expected one {what}, but found {matches.Count}. The trace:{Environment.NewLine}{Describe()}");
    }
}
