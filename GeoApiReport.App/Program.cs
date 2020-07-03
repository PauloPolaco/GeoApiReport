using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GeoApiReport.App
{
	public sealed class Program
	{
		public static async Task Main(string[] args)
		{
			IoC.ConfigureServices();

			string result = await GeoReportRunner.Start()
				? $"GeoReportRunner completed all tasks successfully."
				: $"GeoReportRunner failed to complete some tasks.";
			
			IoC.Logger.LogDebug(result);

			IoC.DisposeServices();
		}
	}
}