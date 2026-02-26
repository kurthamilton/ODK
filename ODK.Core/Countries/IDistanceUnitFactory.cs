namespace ODK.Core.Countries;

public interface IDistanceUnitFactory
{
    DistanceUnit Get(DistanceUnitType type);

    IReadOnlyCollection<DistanceUnit> GetAll();

    DistanceUnit GetDefault();
}