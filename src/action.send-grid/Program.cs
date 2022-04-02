using System;
using System.Linq;

namespace Github.Action.ActiwareSendGrid;

public class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("TOKEN");
        var templateId = Environment.GetEnvironmentVariable("TEMPLATE_ID");
        var sender = Environment.GetEnvironmentVariable("SENDER_MAIL");
        var sendTo = Environment.GetEnvironmentVariable("SEND_TO");

        var templateData = Environment.GetEnvironmentVariable("TEMPLATE_DATA");

        var data = new System.Collections.Generic.Dictionary<string, string>();

        Console.WriteLine("[INF]: Preparing mail data...");

        if (string.IsNullOrWhiteSpace(templateData) == false)
        {
            var values = templateData.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (values == null ||
                values.Length == 0)
            {
                Console.WriteLine("[WRN]: No template data was given.");
            }
            else
            {
                foreach (var value in values)
                {
                    var parts = value.Split('=');

                    if (parts == null ||
                        parts.Length != 2)
                    {
                        Console.WriteLine($"[WRN]: Invalid template data: {value}");
                        continue;
                    }

                    data.Add(parts[0], parts[1] ?? "");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(sendTo) == true)
        {
            Console.WriteLine("[ERR]: No receipients were given.");

            Environment.Exit(1);
            return;
        }

        var receipients = sendTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

        // Create new message
        var message = new SendGrid.Helpers.Mail.SendGridMessage();

        // Set template id
        message.SetTemplateId(templateId);
        // Set template data
        message.SetTemplateData(data);

        // Set from and public to address
        message.SetFrom(new SendGrid.Helpers.Mail.EmailAddress(sender));
        message.AddTo(new SendGrid.Helpers.Mail.EmailAddress(sender));

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

        var client = new SendGrid.SendGridClient(token);

        var response = await client.SendEmailAsync(message);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"[ERR]: Sending mail failed.");
            Console.WriteLine($"[ERR]: Status code: {response?.StatusCode}");
            Console.WriteLine($"[ERR]: Body: {response?.Body}");

            Environment.Exit(1);
            return;
        }

        Console.WriteLine("[INF]: Mail sent.");
    }
}