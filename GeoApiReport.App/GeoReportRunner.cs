using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoApiReport.Core.Enums;
using GeoApiReport.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using GeoApiReport.Core.Services;
using Microsoft.Extensions.Logging;

//using Microsoft.Extensions.Logging;

namespace GeoApiReport.App
{
	internal static class GeoReportRunner
	{
		private static readonly IGeoService s_geoSvc;
		private static readonly IReportService s_reportSvc;
		
		static GeoReportRunner()
		{
			s_geoSvc = IoC.ServiceProvider.GetService<IGeoService>();
			s_reportSvc = IoC.ServiceProvider.GetService<IReportService>();
		}

		public static async Task<bool> Start()
		{
			IoC.Logger.LogDebug("Starting report runner ...");

			// Step 1 - Address List

			AddressModel ipAddressListFromFile = await CreateAddressList();

			if (ipAddressListFromFile == null || ipAddressListFromFile.Addresses.Count < 1)
			{
				IoC.Logger.LogDebug($"Failed to retrieve IP address list from file.");

				return false;
			}

			// Step 2 - Geolocation

			IList<GeolocationModel> addressListWithGeolocations = await FindGeolocationForIPAddress(ipAddressListFromFile);

			if (addressListWithGeolocations == null || addressListWithGeolocations.Count < 1)
			{
				IoC.Logger.LogDebug($"Failed to fetch geolocation mappings for IP address list from REST service.");

				return false;
			}

			// Step 3 - User Data

			Dictionary<UserDataResourceSiteCodeEnum, AddressModel> addressesByGeolocation =
				await SortAddressesByGeographicLocation(addressListWithGeolocations);

			if (addressesByGeolocation == null || addressesByGeolocation.Count < 1)
			{
				IoC.Logger.LogDebug($"Failed to sort IP address list by geolocation.");

				return false;
			}

			IList<UserModel> userDataList = await FindUserDataForAddressesPerGeolocation(addressesByGeolocation);

			if (userDataList == null || userDataList.Count < 1)
			{
				IoC.Logger.LogDebug($"Failed to fetch user data based on IP addresses from REST service.");

				return false;
			}

			AttachLocaleToUserRecords(userDataList, addressListWithGeolocations);

			// Step 4 - Output File

			string reportFile = await CreateReport(userDataList);

			if (String.IsNullOrEmpty(reportFile))
			{
				IoC.Logger.LogDebug($"Failed to commit CSV file to disk.");

				return false;
			}

			LaunchDefaultSpreadsheetApp(reportFile);

			IoC.Logger.LogDebug($"{reportFile} has been generated successfully!");

			return true;
		}

		#region Step 1 - Address List

		private static async Task<AddressModel> CreateAddressList()
		{
			return await s_reportSvc.RetrieveIPAddressListFromFileAsync();
		}

		#endregion Step 1 - Address List

		#region Step 2 - Geolocation

		private static async Task<IList<GeolocationModel>> FindGeolocationForIPAddress(AddressModel ipAddressListFromFile)
		{
			return await s_geoSvc.RetrieveItemsFromResourceAsync<GeolocationModel>(
				s_geoSvc.GeolocationResource, ipAddressListFromFile, "geolocations");
		}

		#endregion Step 2 - Geolocation

		#region Step 3 - User Data

		private static async Task<Dictionary<UserDataResourceSiteCodeEnum, AddressModel>> SortAddressesByGeographicLocation(IList<GeolocationModel> addressListWithGeolocations)
		{
			Dictionary<UserDataResourceSiteCodeEnum, AddressModel> addressesByGeolocation =
				new Dictionary<UserDataResourceSiteCodeEnum, AddressModel>()
				{
					{UserDataResourceSiteCodeEnum.US, new AddressModel() {Addresses = new List<string>()}},
					{UserDataResourceSiteCodeEnum.EU, new AddressModel() {Addresses = new List<string>()}},
					{UserDataResourceSiteCodeEnum.AS, new AddressModel() {Addresses = new List<string>()}}
				};

			foreach (var geoItem in addressListWithGeolocations)
			{
				if (!String.IsNullOrEmpty(geoItem.Address))
				{
					UserDataResourceSiteCodeEnum? userDataLocation =
						s_geoSvc.GetUserDataResourceType(geoItem.Country);

					if (userDataLocation.HasValue)
					{
						addressesByGeolocation[userDataLocation.Value].Addresses.Add(geoItem.Address);
					}
				}
			}

			return addressesByGeolocation;
		}

		private static async Task<IList<UserModel>> FindUserDataForAddressesPerGeolocation(Dictionary<UserDataResourceSiteCodeEnum, AddressModel> addressesByGeolocation)
		{
			IList<UserModel> userModels = new List<UserModel>();

			// Find the user data records from the appropriate service

			foreach (var location in addressesByGeolocation)
			{
				Uri locationUrl = s_geoSvc.UserDataResources[location.Key];
				IList<UserModel> outputModel = await s_geoSvc.RetrieveItemsFromResourceAsync<UserModel>(
					locationUrl, location.Value, "userdata");
				((List<UserModel>)userModels).AddRange(outputModel);
			}

			return userModels;
		}

		private static void AttachLocaleToUserRecords(IList<UserModel> userModels, IList<GeolocationModel> geolocationModels)
		{
			ILookup<string, GeolocationModel> addressToLocaleLookup = 
				geolocationModels.ToLookup(g => g.Address);

			foreach (UserModel userModel in userModels)
			{
				userModel.Country = addressToLocaleLookup.Contains(userModel.Address)
					? addressToLocaleLookup[userModel.Address].First().Country : "unknown";
			}
		}

		#endregion Step 3 - User Data

		#region Step 4 - Output File

		private static async Task<string> CreateReport(IList<UserModel> userModels)
		{
			return await s_reportSvc.GenerateReportAsCsvAsync(userModels);
		}

		private static void LaunchDefaultSpreadsheetApp(string reportFile)
		{
			new Process
			{
				StartInfo = new ProcessStartInfo(reportFile)
				{
					UseShellExecute = true
				}
			}.Start();
		}

		#endregion Step 4 - Output File
	}
}