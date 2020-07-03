using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using GeoApiReport.Core.Mappers;
using GeoApiReport.Core.Models;

namespace GeoApiReport.Core.Services
{
	public sealed class ReportService : IReportService
	{
		private const string s_inputFile = "Resources\\IPAddresses.txt";
		private const string s_outputFile = "Report.csv";

		/// <summary>
		/// Retrieves a list of IP addresses by parsing a text file.
		/// </summary>
		/// <returns>List of IP addresses.</returns>
		public async Task<AddressModel> RetrieveIPAddressListFromFileAsync()
		{
			if (!File.Exists(s_inputFile))
			{
				throw new FileNotFoundException();
			}

			AddressModel addressList = new AddressModel() { Addresses = new List<string>() };
			string[] fileAsLines = await FileEx.ReadAllLinesAsync(s_inputFile, Encoding.Default);

			if (fileAsLines != null && fileAsLines.Length > 0)
			{
				foreach (string address in fileAsLines)
				{
					if (IsAddressValid(address.Trim()))
					{
						addressList.Addresses.Add(address);
					}
				}
			}

			return addressList;
		}

		/// <summary>
		/// Generates a report as a CSV file that contains user and geolocation data.
		/// </summary>
		/// <param name="reportModel">User data to be written into the CSV file.</param>
		/// <returns>Filename when the CSV file was committed to disk correctly.</returns>
		public async Task<string> GenerateReportAsCsvAsync(IList<UserModel> reportModel)
		{
			if (reportModel == null || reportModel.Count < 1)
			{
				return null; // don't bother writing an empty file
			}

			using (StreamWriter streamWriter = new StreamWriter(s_outputFile, false, new UTF8Encoding(true)))
			{
				using (CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.CurrentCulture, false))
				{
					csvWriter.WriteHeader<UserModel>();
					await csvWriter.NextRecordAsync();
					await csvWriter.WriteRecordsAsync<UserModel>(reportModel);
				}
			}

			if (!File.Exists(s_outputFile))
			{
				return null; // couldn't write to file
			}

			return s_outputFile;
		}

		private bool IsAddressValid(string ipAddress)
		{
			IPAddress ip4;
			return IPAddress.TryParse(ipAddress, out ip4);
		}
	}
}