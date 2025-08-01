static async System.Threading.Tasks.Task StartActionAsync(ActionInputs inputs)
{
    Console.WriteLine("[DBG]: Environment variables:");
    Console.WriteLine("-> TOKEN: *********");
    Console.WriteLine($"-> TEMPLATE_ID: {inputs?.TemplateId}");
    Console.WriteLine($"-> SENDER_MAIL: {inputs?.SenderMail}");
    Console.WriteLine($"-> SEND_TO: {inputs?.SendsTo}");
    Console.WriteLine($"-> POWER_AUTOMATE_FLOW: {inputs?.PowerAutomateFlow}");

    var flowAddress = inputs?.PowerAutomateFlow;

    if (inputs?.Data != null)
    {
        foreach (var item in inputs.Data)
        {
            Console.WriteLine($"-> {item.Key}: {item.Value}");
        }
    }

    Console.WriteLine("[INF]: Preparing mail data...");

    if (string.IsNullOrWhiteSpace(inputs?.PowerAutomateFlow) == true)
    {
        Console.WriteLine("[INF]: Use default power automate flow.");
        flowAddress = "https://defaultd973cdf80eb442efac5b76642b0418.86.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/6440ca54561449febe4c8ab340c4a396/triggers/manual/paths/invoke/?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=OxcFCKwtPhseqAnU1BckfL_COZhF9VddlwoNhVUWL0o";
    }

    var client = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Post, flowAddress);
    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(inputs?.Data), null, "application/json");
    request.Content = content;

    var response = await client.SendAsync(request);

    if (response.IsSuccessStatusCode == false)
    {
        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[ERR]: Sending mail failed.");
        Console.WriteLine($"[ERR]: Status code: {response?.StatusCode}");
        Console.WriteLine($"[ERR]: Body: {body ?? ""}");

        Environment.Exit(1);
        return;
    }

    Console.WriteLine("[INF]: Mail sent.");
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(errors =>
{
    Console.WriteLine($"[Err]: {string.Join(Environment.NewLine, errors.Select(error => error.ToString()))}");

    Environment.Exit(2);
});

await parser.WithParsedAsync(options => StartActionAsync(options));