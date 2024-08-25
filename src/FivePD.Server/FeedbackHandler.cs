using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.Extensions;
using Newtonsoft.Json;

namespace FivePD.Server;

#pragma warning disable

public static class FeedbackHandler
{
    private static HttpClient _httpClient;

    public static async Task SendFeedback(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        _httpClient ??= new HttpClient(new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        }, false);

        try
        {
            dynamic obj = expandoObject;

            ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;

            var sb = new StringBuilder();
            sb.Append($"\n**{obj.Name}**\n");
            sb.Append(obj.Message);
            sb.Append("\n\n");
            sb.Append("*NUI*\n```");
            sb.Append($"{JsonConvert.SerializeObject(obj.NuiErrors)}");
            sb.Append("```");

            var contentObject = new { content = sb.ToString() };
            var jsonContent = JsonConvert.SerializeObject(contentObject);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                "https://discord.com/api/webhooks/1247121240578199574/mn3VqaRbQE1vitFLtT48R09aKdfIlXEhn8LMKhMDoU8RmIEoInf5ykAwcKgNnFr16Qv3",
                httpContent);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                networkCallbackDelegate.InvokeWithResponse(true, "Successful send", "Your feedback has been successfully sent. Thanks!");
            }
            else
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }
        catch (Exception e)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }
}