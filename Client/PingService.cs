using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Client
{
	public class PingService : BackgroundService
	{
		private readonly HubConnection _connection;
		private readonly ILogger _logger;

		const int pingTimeMs = 10000;

		public PingService(ILogger<PingService> logger)
		{
			_logger = logger;

			_connection = new HubConnectionBuilder()
				.WithUrl("https://localhost:44342/PingHub")
				.Build();

			_connection.Closed += async (error) =>
			{
				await Task.Delay(new Random().Next(0, 5) * 1000);
				await _connection.StartAsync();
			};
		}

		private async Task OnPing(string pinger)
		{
			_logger.LogDebug($"Ping received {pinger}!");
			await _connection.InvokeAsync("Pong", pinger, _connection.ConnectionId);
			_logger.LogInformation($"Hello {pinger}, I am {_connection.ConnectionId}. :)");
		}

		private void OnPong(string ponger)
		{
			_logger.LogInformation($"Pleased to meet you {ponger}!");
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"Service is starting.");

			stoppingToken.Register(() =>
				_logger.LogInformation($"Service is stopping."));

			_connection.On<string>("Pinged", OnPing);
			_connection.On<string>("Ponged", OnPong);

			_logger.LogDebug("Handlers registered!");

			try
			{
				await _connection.StartAsync();
				_logger.LogDebug("Connection started!");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Connection could not start!", ex);
			}

			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogDebug("Pinging...");
				await _connection.InvokeAsync("Ping", _connection.ConnectionId);

				_logger.LogInformation($"Hello folks, I am {_connection.ConnectionId}! Is anybody there?");
				await Task.Delay(pingTimeMs, stoppingToken);
			}

			_logger.LogInformation($"Service is stopping.");
		}
	}
}
