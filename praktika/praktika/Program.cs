using var httpClient = new HttpClient();
        
const string url = $"https://api.vk.ru/method/users.get?user_id=1&fields=personal&v=5.131&access_token=f6f150e4f6f150e4f6f150e4cef5ca8f07ff6f1f6f150e49e0625ac4a4be5e856cbf858";

var response = await httpClient.GetAsync(url);
if (response.IsSuccessStatusCode)
{
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}