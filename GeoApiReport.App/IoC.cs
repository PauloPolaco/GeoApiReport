using System;
using System.Threading.Tasks;
using GeoApiReport.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GeoApiReport.App
{
	/// <summary>
	/// Default .NET Core dependency injection wrapper.
	/// </summary>
	internal static class IoC
	{
		/// <summary>
		/// Dependency injection service provider.
		/// </summary>
		public static IServiceProvider ServiceProvider => s_serviceProvider;
		private static IServiceProvider s_serviceProvider;

		/// <summary>
		/// Dependency injection logger.
		/// </summary>
		public static ILogger<Program> Logger => s_logger;
		private static ILogger<Program> s_logger;

		/// <summary>
		/// Initializes the dependency injection service.
		/// </summary>
		internal static void ConfigureServices()
		{
			s_serviceProvider = new ServiceCollection()
				.AddLogging(builder => builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug))
				.AddSingleton<IReportService, ReportService>()
				.AddSingleton<IGeoService, GeoService>()
				.BuildServiceProvider();

			s_logger = s_serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

			s_logger.LogDebug("Dependency injection initialized successfully.");
		}

		/// <summary>
		/// Destroys the dependency injection service.
		/// </summary>
		internal static void DisposeServices()
		{
			if (s_serviceProvider == null)
			{
				s_logger.LogDebug("Cannot destroy dependency injection service provider ...");
			}
			else
			{
				if (s_serviceProvider is IDisposable)
				{
					((IDisposable)s_serviceProvider).Dispose();
				}
			}

			if (s_logger == null)
			{
				s_logger.LogDebug("Cannot destroy logger ...");
			}
			else
			{
				if (s_logger is IDisposable)
				{
					((IDisposable)s_logger).Dispose();
				}
			}
		}
	}
}