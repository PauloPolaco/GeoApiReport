using CsvHelper.Configuration;
using GeoApiReport.Core.Models;

namespace GeoApiReport.Core.Mappers
{
	public sealed class UserModelMap : ClassMap<UserModel>
	{
		public UserModelMap()
		{
			Map(m => m.Address).Name("IPv4 Address");
			Map(m => m.Country).Name("Country of Origin");
			Map(m => m.Name).Name("Name");
			Map(m => m.Gender).Name("Gender");
			Map(m => m.Age).Name("Age");
		}
	}
}