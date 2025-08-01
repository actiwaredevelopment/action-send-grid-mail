namespace Github.Action.ActiwareSendGrid;

public class ActionInputs
{
    string _templateData = null!;
    Dictionary<string, string> _data = null;

    public ActionInputs()
    {
        if (Environment.GetEnvironmentVariable(
            "GREETINGS") is { Length: > 0 } greetings)
        {
            Console.WriteLine(greetings);
        }
    }

    [Option('t', "token",
        Required = false,
        HelpText = "The api token, for the twillio send grid.")]
    public string Token { get; set; } = null!;

    [Option('i', "template-id",
            Required = false,
            HelpText = "The identifier of the template.")]
    public string TemplateId { get; set; } = null!;

    [Option('f', "flow",
            Required = false,
            HelpText = "The microsoft power automate flow address.")]
    public string PowerAutomateFlow { get; set; } = null!;

    [Option('m', "sender-mail",
            Required = false,
            HelpText = "The sender for the mail.")]
    public string SenderMail { get; set; } = null!;

    [Option('r', "send-to",
            Required = false,
            HelpText = "The receipients comma-separated.")]
    public string SendsTo { get; set; } = null!;

    [Option('d', "template-data",
            Required = true,
            HelpText = "The data as key=value semicolon-separated.")]
    public string TemplateData
    {
        get => _templateData;
        set => parseTemplateData(value, str => _templateData = str, data => _data = data);
    }

    public Dictionary<string, string> Data
    {
        get => _data ?? new();
    }

    static void parseTemplateData(string? value, Action<string> assign, Action<Dictionary<string, string>> assignData)
    {
        if (value is { Length: > 0 })
        {
            if (assign is not null)
            {
                assign(value);
            }

            if (assignData is not null)
            {
                var data = new Dictionary<string, string>();
                var items = value.Split(';', StringSplitOptions.RemoveEmptyEntries);

                if (items == null ||
                    items.Length == 0)
                {
                    Console.WriteLine("[WRN]: No template data was given.");
                }
                else
                {
                    foreach (var item in items)
                    {
                        var parts = item.Split('=');

                        if (parts == null ||
                            parts.Length != 2)
                        {
                            Console.WriteLine($"[WRN]: Invalid template data: {item}");
                            continue;
                        }

                        if (data.ContainsKey(parts[0]) == false)
                        {
                            data.Add(parts[0], parts[1] ?? "");
                        }
                    }
                }

                assignData(data);
            }
        }
    }
}