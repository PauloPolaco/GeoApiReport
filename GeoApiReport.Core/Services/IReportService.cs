using System.Collections.Generic;
using System.Threading.Tasks;
using GeoApiReport.Core.Models;

namespace GeoApiReport.Core.Services
{
	public interface IReportService
	{
		/// <summary>
		/// Retrieves a list of IP addresses by parsing a text file.
		/// </summary>
		/// <returns>List of IP addresses.</returns>
		Task<AddressModel> RetrieveIPAddressListFromFileAsync();

		/// <summary>
		/// Generates a report as a CSV file that contains user and geolocation data.
		/// </summary>
		/// <param name="reportModel">User data to be written into the CSV file.</param>
		/// <returns>Filename when the CSV file was committed to disk correctly.</returns>
		Task<string> GenerateReportAsCsvAsync(IList<UserModel> reportModel);
	}
}