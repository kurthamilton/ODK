namespace ODK.Core.Countries;

public interface IDistanceUnitFactory
{
    DistanceUnit Get(DistanceUnitType type);

    IReadOnlyCollection<DistanceUnit> GetAll();

    DistanceUnit GetByCountry(Country country);

    DistanceUnit GetDefault();
}