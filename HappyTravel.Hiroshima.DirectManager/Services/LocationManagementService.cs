using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.GeoData.Enums;
using HappyTravel.Geography;
using HappyTravel.Hiroshima.Common.Constants;
using HappyTravel.Hiroshima.Common.Infrastructure.Extensions;
using HappyTravel.Hiroshima.Common.Infrastructure.Utilities;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Common.Models.Accommodations;
using HappyTravel.Hiroshima.Common.Models.Locations;
using HappyTravel.Hiroshima.Data;
using HappyTravel.Hiroshima.Data.Extensions;
using HappyTravel.Hiroshima.DirectManager.RequestValidators;
using LocationNameNormalizer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Hiroshima.DirectManager.Services
{
    public class LocationManagementService : ILocationManagementService
    {
        public LocationManagementService(DirectContractsDbContext dbContext, ILocationNameNormalizer locationNameNormalizer)
        {
            _dbContext = dbContext;
            _locationNameNormalizer = locationNameNormalizer;
        }


        public Task<Result<Models.Responses.Location>> Add(Models.Requests.Location location)
        {
            return ValidationHelper.Validate(location, new LocationValidator())
                .Bind(async () => await GetCountry(location.Country))
                .Map(country => AddLocation(country, location.Locality, location.Zone))
                .Map(locationAndCountry => Build(locationAndCountry.location, locationAndCountry.country));
        }

        
        public async Task<List<Models.Responses.Location>> Get(int top = 100, int skip = 0)
        {
              var locations = await _dbContext.Locations.Join( _dbContext.Countries, location => location.CountryCode, country => country.Code, (location, country) => new {location, country})
                  .OrderBy(locationAndCounty=> locationAndCounty.location.Id)
                  .Skip(skip).Take(top)
                  .ToListAsync();

              return locations.Select(location => Build(location.location, location.country)).ToList();
        }


        public async Task<List<EdoContracts.GeoData.Location>> Get(DateTime modified, LocationTypes locationType, int skip = 0, int take = 10000) => locationType switch
        {
            LocationTypes.Accommodation => (await _dbContext.Accommodations.Include(accommodation => accommodation.Location).ThenInclude(location => location.Country)
                    .Where(accommodation => accommodation.Modified > modified)
                    .OrderBy(accommodation => accommodation.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync())
                .Select(accommodation => new EdoContracts.GeoData.Location(JsonConvert.SerializeObject(accommodation.Name),
                    JsonConvert.SerializeObject(accommodation.Location.Locality), JsonConvert.SerializeObject(accommodation.Location.Country.Name),
                    new GeoPoint(accommodation.Coordinates), default, PredictionSources.Interior, LocationTypes.Accommodation))
                .ToList(),
            LocationTypes.Location => (await _dbContext.Accommodations.Include(accommodation => accommodation.Location).ThenInclude(location => location.Country)
                    .Where(accommodation => accommodation.Modified > modified)
                    .OrderBy(accommodation => accommodation.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync())
                .Select(accommodation => new EdoContracts.GeoData.Location(string.Empty, JsonConvert.SerializeObject(accommodation.Location.Locality),
                    JsonConvert.SerializeObject(accommodation.Location.Country.Name), default, default, PredictionSources.Interior, LocationTypes.Location))
                .ToList(),
            LocationTypes.Unknown => new List<EdoContracts.GeoData.Location>(0),
            LocationTypes.Destination => new List<EdoContracts.GeoData.Location>(0),
            LocationTypes.Landmark => new List<EdoContracts.GeoData.Location>(0),
            _ => throw new ArgumentOutOfRangeException(nameof(locationType), locationType, null)
        };
        
        
        public Task<List<string>> GetCountryNames(int top = 100, int skip = 0)
            => _dbContext.Countries.OrderBy(country => country.Code)
                .Skip(skip)
                .Take(top)
                .Select(country => country.Name.En)
                .ToListAsync();


        private async Task<(Location location, Country country)> AddLocation(Country country, string localityName, string zoneName)
        {
            var normalizedLocality = _locationNameNormalizer.GetNormalizedLocalityName(country.Name.En, localityName);
            
            var location = await GetLocation();
            
            if (location != null)
                return (location, country);
            
            location = new Location
            {
                CountryCode = country.Code,
                Locality = new MultiLanguage<string> {En = normalizedLocality},
            };
            
            if (!string.IsNullOrEmpty(zoneName))
                location.Zone = new MultiLanguage<string> {En = zoneName};
            
            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync();

            _dbContext.DetachEntry(location);
            
            return (location, country);
            
            
            async Task<Location> GetLocation()
            {
                Expression<Func<Location, bool>> countryAndLocalityExistExpression = l 
                    => l.CountryCode == country.Code &&
                    l.Locality.En == normalizedLocality;
            
                var location = _dbContext.Locations.Where(countryAndLocalityExistExpression);
                
                if (!string.IsNullOrEmpty(zoneName))
                {
                    return await location.Where(l 
                        => l.Zone.En.ToUpper() == zoneName.ToUpper()).SingleOrDefaultAsync();
                }

                var locations = await location.ToListAsync();
                
                return locations.FirstOrDefault(loc => loc.Zone.GetAll().Any());
            }
        }


        private async Task<Result<Country>> GetCountry(string name)
        {
            var country = await _dbContext.Countries.SingleOrDefaultAsync(country
                => country.Name.En == name);
            
            return country == null
                ? Result.Failure<Country>($"Failed to retrieve country '{name}'")
                : Result.Success(country);
        }


        private Models.Responses.Location Build(Location location, Country country) => new Models.Responses.Location(location.Id, location.CountryCode, country.Name.En, location.Locality.En, location.Zone.En);

        
        private readonly DirectContractsDbContext _dbContext;
        private readonly ILocationNameNormalizer _locationNameNormalizer;
    }
}