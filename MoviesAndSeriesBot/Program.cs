using System.Text.Json.Nodes;
using Discord;
using Discord.WebSocket;
using RestSharp;
using RestClient = RestSharp.RestClient;
using TokenType = Discord.TokenType;

namespace MoviesAndSeriesBot
{
    internal class Program
    {
        private DiscordSocketClient _client;

        private readonly string _tmdbApiKey =
            "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIxZmY0MjcwZDU0NjMwMGIwZGQwMWQwZDQxZDMxNzI4NSIsIm5iZiI6MTc0ODE3NDA3OS41MjgsInN1YiI6IjY4MzMwNGZmZmE3YTc5YjgxMjgzNzk3ZiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.-shmT8xJanU1-5b5Y4cODGB7mlU8Va6sVNjME2EUHeI";

        static async Task Main(string[] args)
        {
            var myBot = new Program();
            await myBot.StartBotAsync();
        }

        public async Task StartBotAsync()
        {
            _client = new DiscordSocketClient();
            _client.Ready += LoginAndScrape;
            _client.Log += LogFuncAsync;
            await this._client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await this._client.StartAsync();
            await Task.Delay(-1);
            async Task LogFuncAsync(LogMessage message) => Console.WriteLine(message.ToString());
        }
        
        public async Task LoginAndScrape()
        {
            DeleteTrendingMovieMessages();
            Console.WriteLine($"");
            DeleteTrendingSeriesMessages();
            Console.WriteLine($"");
            CallPopularMoviesAPI();
            Console.WriteLine($"");
            CallPopularSeriesAPI();
            Console.WriteLine($"");
            Environment.Exit(0);
        }

        private void DeleteTrendingMovieMessages()
        {
            ulong moviesTrendingChannel = 1376177846954754058;
            IMessageChannel channel = _client.GetChannel(moviesTrendingChannel) as IMessageChannel;
            var messages = channel.GetMessagesAsync(100).FlattenAsync();
            foreach (var message in messages.Result) 
            {
                channel.DeleteMessageAsync((message)); 
                Thread.Sleep(5000); 
            }
        }
        
        private void DeleteTrendingSeriesMessages()
        {
            ulong moviesTrendingChannel = 1376178433016594543;
            IMessageChannel channel = _client.GetChannel(moviesTrendingChannel) as IMessageChannel;
            var messages = channel.GetMessagesAsync(100).FlattenAsync();
            foreach (var message in messages.Result)
            {
                channel.DeleteMessageAsync((message));   
                Thread.Sleep(5000);
            }
        }

        private void CallPopularMoviesAPI()
        {
            var options = new RestClientOptions("https://api.themoviedb.org/3/trending/movie/day?language=en-US");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {_tmdbApiKey}");
            var response = client.Get(request);
            var content = response.Content;
            JsonNode node = JsonNode.Parse(content);
            JsonArray results = node["results"].AsArray();

            foreach (var result in results)
            {
                if (result["original_language"].ToString() == "en")
                {
                    PostMethod(result.AsObject());
                    Thread.Sleep(5000);
                }
            }
            ulong moviesTrendingChannel = 1376170892760973424;
            IMessageChannel channel = _client.GetChannel(moviesTrendingChannel) as IMessageChannel;
            channel.SendMessageAsync("Trending Movies Posted");
        }
        
        private void CallPopularSeriesAPI()
        {
            var options = new RestClientOptions("https://api.themoviedb.org/3/trending/tv/day?language=en-US");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {_tmdbApiKey}");
            var response = client.Get(request);
            var content = response.Content;
            JsonNode node = JsonNode.Parse(content);
            JsonArray results = node["results"].AsArray();

            foreach (var result in results)
            {
                if (result["original_language"].ToString() == "en")
                {
                    SeriesPostMethod(result.AsObject());
                    Thread.Sleep(5000);
                }
            }
            ulong moviesTrendingChannel = 1376170892760973424;
            IMessageChannel channel = _client.GetChannel(moviesTrendingChannel) as IMessageChannel;
            channel.SendMessageAsync("Trending Series Posted");
        }

        private void PostMethod(JsonObject jsonObject)
        {
            var builder = new EmbedBuilder()
            {
                //Optional color
                Color = Color.Blue,
                Title = $"***TRENDING MOVIE*** \r\n \r\n **MOVIE TITLE**: {jsonObject["original_title"].ToString()}",
                Description = $"**MOVIE RATING**: {jsonObject["vote_average"].ToString()} \r\n \r\n **OVERVIEW**: {jsonObject["overview"].ToString()}",
                ImageUrl = $"http://image.tmdb.org/t/p/w185/{jsonObject["poster_path"].ToString()}"
            };
            
            ulong moviesTrendingChannel = 1376177846954754058;
            IMessageChannel channel = _client.GetChannel(moviesTrendingChannel) as IMessageChannel;
            channel.SendMessageAsync("", false, builder.Build());
        }
        
        private void SeriesPostMethod(JsonObject jsonObject)
        {
            var builder = new EmbedBuilder()
            {
                //Optional color
                Color = Color.Blue,
                Title = $"***TRENDING SERIES*** \r\n \r\n **SERIES TITLE**: {jsonObject["original_name"].ToString()}",
                Description = $"**SERIES RATING**: {jsonObject["vote_average"].ToString()} \r\n \r\n **OVERVIEW**: {jsonObject["overview"].ToString()}",
                ImageUrl = $"http://image.tmdb.org/t/p/w185/{jsonObject["poster_path"].ToString()}"
            };
            
            ulong moviesTrendingChannel = 1376178433016594543;
            IMessageChannel channel = _client.GetChannel(moviesTrendingChannel) as IMessageChannel;
            channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
