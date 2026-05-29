using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FishTools.App;

internal sealed class DiscordManager : ITool
{
    public string Id => "discord-manager";
    public string Name => "Discord Manager";
    public string Category => ToolCategories.SocialWeb;
    public string Description => "Automate Discord messaging, webhook management, and account profiling.";

    private static readonly HttpClient DiscordHttpClient = new();
    private string? _discordToken;

    public async Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        if (string.IsNullOrEmpty(_discordToken))
            _discordToken = ConsoleUi.Prompt("Enter Discord Token (leave blank for later entry)");

        while (true)
        {
            ConsoleUi.ResetScreen(Name);
            var choice = ConsoleUi.ShowMenu("Select Action", ["Send Message to Channel", "Send Message via Webhook", "Remove Webhook", "Scrape Group IDs", "Profile Current User", "List Account Guilds", "Back"]);
            if (choice == 6)
                break;

            try
            {
                switch (choice)
                {
                    case 0:
                        await SendChannelMessage();
                        break;
                    case 1:
                        await SendWebhookMessage();
                        break;
                    case 2:
                        await DeleteWebhook();
                        break;
                    case 3:
                        await ScrapeGroups(context);
                        break;
                    case 4:
                        await GetUserInfo();
                        break;
                    case 5:
                        await GetGuilds();
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUi.Error(ex.Message);
            }

            ConsoleUi.Pause();
        }
    }

    private async Task SendChannelMessage()
    {
        if (string.IsNullOrEmpty(_discordToken))
            _discordToken = ConsoleUi.PromptRequired("User Token");
        var chId = ConsoleUi.PromptRequired("Channel ID");
        var msg = ConsoleUi.PromptRequired("Content");
        var cnt = ConsoleUi.PromptInt("Repeat Amount", 1, 1, 100);

        DiscordHttpClient.DefaultRequestHeaders.Clear();
        DiscordHttpClient.DefaultRequestHeaders.Add("Authorization", _discordToken);

        for (int i = 0; i < cnt; i++)
        {
            var r = await DiscordHttpClient.PostAsJsonAsync($"https://discord.com/api/v9/channels/{chId}/messages", new { content = msg });
            if (r.IsSuccessStatusCode)
                ConsoleUi.Success($"Sent successfully [{i + 1}/{cnt}]");
            else
                ConsoleUi.Error($"Error: {r.StatusCode}");
        }
    }

    private async Task SendWebhookMessage()
    {
        var u = ConsoleUi.PromptRequired("URL");
        var m = ConsoleUi.PromptRequired("Message");
        var c = ConsoleUi.PromptInt("Count", 1, 1, 100);
        for (int i = 0; i < c; i++)
        {
            var r = await DiscordHttpClient.PostAsJsonAsync(u, new { content = m });
            if (r.IsSuccessStatusCode)
                ConsoleUi.Success($"Webhook sent {i + 1}/{c}");
            else
                ConsoleUi.Error($"Error: {r.StatusCode}");
        }
    }

    private async Task DeleteWebhook()
    {
        var u = ConsoleUi.PromptRequired("URL");
        var r = await DiscordHttpClient.DeleteAsync(u);
        if (r.IsSuccessStatusCode)
            ConsoleUi.Success("Webhook removed.");
        else
            ConsoleUi.Error($"Error: {r.StatusCode}");
    }

    private async Task ScrapeGroups(ToolContext context)
    {
        if (string.IsNullOrEmpty(_discordToken))
            _discordToken = ConsoleUi.PromptRequired("User Token");
        DiscordHttpClient.DefaultRequestHeaders.Clear();
        DiscordHttpClient.DefaultRequestHeaders.Add("Authorization", _discordToken);
        var r = await DiscordHttpClient.GetAsync("https://discord.com/api/v9/users/@me/channels");
        if (r.IsSuccessStatusCode)
        {
            var list = await r.Content.ReadFromJsonAsync<List<DiscordChannel>>();
            var groups = list?.Where(c => c.Type == 3).ToList() ?? [];
            ConsoleUi.Success($"Scraped {groups.Count} group channels.");
            foreach (var g in groups)
                ConsoleUi.Info($"- {g.Name ?? "Group"} ({g.Id})");
            if (groups.Count > 0 && ConsoleUi.Confirm("Export group IDs to JSON?"))
            {
                var p = context.CreateReportPath("discord_groups", ".json");
                await System.IO.File.WriteAllTextAsync(p, System.Text.Json.JsonSerializer.Serialize(groups, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                ConsoleUi.Success($"Exported to {p}");
            }
        }
        else
            ConsoleUi.Error($"Error: {r.StatusCode}");
    }

    private async Task GetUserInfo()
    {
        if (string.IsNullOrEmpty(_discordToken))
            _discordToken = ConsoleUi.PromptRequired("User Token");
        DiscordHttpClient.DefaultRequestHeaders.Clear();
        DiscordHttpClient.DefaultRequestHeaders.Add("Authorization", _discordToken);
        var r = await DiscordHttpClient.GetAsync("https://discord.com/api/v9/users/@me");
        if (r.IsSuccessStatusCode)
        {
            var u = await r.Content.ReadFromJsonAsync<DiscordUser>();
            if (u != null)
            {
                ConsoleUi.Success($"Identified as: {u.Username}#{u.Discriminator} ({u.Id})");
                ConsoleUi.Info($"Email: {u.Email}");
                ConsoleUi.Info($"MFA Status: {u.MfaEnabled}");
            }
        }
        else
            ConsoleUi.Error($"Error: {r.StatusCode}");
    }

    private async Task GetGuilds()
    {
        if (string.IsNullOrEmpty(_discordToken))
            _discordToken = ConsoleUi.PromptRequired("User Token");
        DiscordHttpClient.DefaultRequestHeaders.Clear();
        DiscordHttpClient.DefaultRequestHeaders.Add("Authorization", _discordToken);
        var r = await DiscordHttpClient.GetAsync("https://discord.com/api/v9/users/@me/guilds");
        if (r.IsSuccessStatusCode)
        {
            var list = await r.Content.ReadFromJsonAsync<List<DiscordGuild>>();
            if (list != null)
            {
                ConsoleUi.Success($"Acount linked to {list.Count} servers.");
                foreach (var g in list)
                    ConsoleUi.Info($"- {g.Name} ({g.Id})");
            }
        }
        else
            ConsoleUi.Error($"Error: {r.StatusCode}");
    }

    private record DiscordChannel([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("type")] int Type, [property: JsonPropertyName("name")] string? Name);

    private record DiscordUser(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("discriminator")] string Discriminator,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("mfa_enabled")] bool MfaEnabled
    );

    private record DiscordGuild([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("name")] string Name);
}
