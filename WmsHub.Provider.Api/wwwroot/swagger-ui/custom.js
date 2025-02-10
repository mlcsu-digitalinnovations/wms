var callback = function () {

  var elem = document.createElement("div");
  elem.innerHTML = "<div><pre><code>" +
    "{" +
    "class Program" +
    "{" +
    'private static string _url = \"https://.........../ProviderAuth\";' +
    "private static readonly HttpClient _client = new HttpClient();" +
    "private static string _apiKey;" +
    "private static string _key;" +
    "private static string _token;" +
    "private static string _refreshToken;" +
    "private static bool _isExit;" +
    "static async Task Main(string[] args) {" +
    "try {" +
    'Console.WriteLine(\"Auhorisation Test Harness\");' +
    'Console.WriteLine(\"What is your API Key ? \");' +
    "_apiKey = Console.ReadLine();" +
    "await GetKey(_apiKey);" +
    'Console.WriteLine(\"Please provide SMS Key\");' +
    "_key = Console.ReadLine();" +
    "await GetToken(_apiKey, _key);" +
    "await RefreshToken(_apiKey, _refreshToken);" +
    'Console.WriteLine(\"Press any key to exit\");' +
    "}" +
    "catch (Exception ex)" +
    "{" +
    "Console.WriteLine(ex.Message);" +
    "}" +
    "Console.ReadKey();" +
    "}" +
    "static async Task GetKey(string apiKey)" +
    "{" +
    "_client.DefaultRequestHeaders.Accept.Clear();" +
    '_client.DefaultRequestHeaders.Add(\"x - api - key\", apiKey);' +
    "string msg = await _client.GetStringAsync(_url);" +
    "Console.WriteLine(msg);" +
    "}" +
    "static async Task GetToken(string apiKey, string smsKey)" +
    "{" +
    'string fullUrl = $\"{ _url } /{smsKey}\";' +
    "_client.DefaultRequestHeaders.Clear();" +
    '_client.DefaultRequestHeaders.Add(\"x - api - key\", apiKey);' +
    "string msg = await _client.GetStringAsync(fullUrl);" +
    "dynamic message = JObject.Parse(msg);" +
    "_refreshToken = message.refreshToken;" +
    'Console.WriteLine($\"Refresh Token: { _refreshToken }\");' +
    "_token = message.accessToken;" +
    'Console.WriteLine($\"Access Token: { _token }\");' +
    'Console.WriteLine(\"Your keys are: \");' +
    "Console.WriteLine(msg);" +
    "}" +
    "static async Task RefreshToken(string apiKey, string refreshToken)" +
    "{" +
    "string fullUrl =" +
    '$\"{ _url }?grant_type = refresh_token\" +' +
    '$\" & refresh_token={ refreshToken } \";' +
    "_client.DefaultRequestHeaders.Clear();" +
    '_client.DefaultRequestHeaders.Add(\"x - api - key\", apiKey);' +
    "HttpResponseMessage response = await _client.PostAsync(fullUrl, null);" +
    "var msg = await response.Content.ReadAsStringAsync();" +
    "dynamic message = JObject.Parse(msg);" +
    "_refreshToken = message.refreshToken;" +
    'Console.WriteLine($\"Refresh Token: { _refreshToken }\");' +
    "_token = message.accessToken;" +
    'Console.WriteLine($\"Access Token: { _token }\");' +
    'Console.WriteLine(\"Your keys are: \");' +
    "Console.WriteLine(msg);" +
    "}" +
    "}" +
    "</code></pre></div>";
  document.body.insertBefore(elem, document.body.firstChild);
};

if (document.readyState === "complete" || (document.readyState !== "loading" && !document.documentElement.doScroll)) {
  callback();
} else {
  document.addEventListener("DOMContentLoaded", callback);
}