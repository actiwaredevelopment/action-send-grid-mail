static async System.Threading.Tasks.Task StartActionAsync(ActionInputs inputs)
{
    Console.WriteLine("[DBG]: Environment variables:");
    Console.WriteLine("-> TOKEN: *********");
    Console.WriteLine($"-> TEMPLATE_ID: {inputs?.TemplateId}");
    Console.WriteLine($"-> SENDER_MAIL: {inputs?.SenderMail}");
    Console.WriteLine($"-> SEND_TO: {inputs?.SendsTo}");

    if (inputs?.Data != null)
    {
        foreach (var item in inputs.Data)
        {
            Console.WriteLine($"-> {item.Key}: {item.Value}");
        }
    }

    Console.WriteLine("[INF]: Preparing mail data...");

    if (string.IsNullOrWhiteSpace(inputs?.Token) == true)
    {
        Console.WriteLine("[ERR]: No token were given.");

        Environment.Exit(1);
        return;
    }

    if (string.IsNullOrWhiteSpace(inputs?.SendsTo) == true)
    {
        Console.WriteLine("[ERR]: No receipients were given.");

        Environment.Exit(1);
        return;
    }

    var receipients = inputs?.SendsTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

    // Create new message
    var message = new SendGrid.Helpers.Mail.SendGridMessage();

    // Set template id
    message.SetTemplateId(inputs?.TemplateId);
    // Set template data
    message.SetTemplateData(inputs?.Data);

    // Set from and public to address
    message.SetFrom(new SendGrid.Helpers.Mail.EmailAddress(inputs?.SenderMail));
    message.AddTo(new SendGrid.Helpers.Mail.EmailAddress(inputs?.SenderMail));

    if (receipients == null ||
        receipients.Length == 0)
    {
        Console.WriteLine("[ERR]: No receipients were given.");

        Environment.Exit(1);
        return;
    }

    // Set receipients as bcc
    foreach (var receipient in receipients)
    {
        if (string.IsNullOrWhiteSpace(receipient) == true ||
            receipient.Contains("@") == false)
        {
            Console.WriteLine($"[WRN]: Invalid receipient: {receipient}");
            continue;
        }

        message.AddBcc(new SendGrid.Helpers.Mail.EmailAddress(receipient));
    }

    var client = new SendGrid.SendGridClient(inputs?.Token);

    var response = await client.SendEmailAsync(message);

    if (response.IsSuccessStatusCode == false)
    {
        var body = await response?.Body?.ReadAsStringAsync();

        Console.WriteLine($"[ERR]: Sending mail failed.");
        Console.WriteLine($"[ERR]: Status code: {response?.StatusCode}");
        Console.WriteLine($"[ERR]: Body: {body ?? ""}");

        Environment.Exit(1);
        return;
    }

    Console.WriteLine("[INF]: Mail sent.");
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
            {
                Console.WriteLine($"[Err]: {string.Join(Environment.NewLine, errors.Select(error => error.ToString()))}");

                Environment.Exit(2);
            });

await parser.WithParsedAsync(options => StartActionAsync(options));