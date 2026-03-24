namespace Application.DTO.MeasurementDTOs
{
    public record MeasurementDto(
        Guid Id,
        Guid DeviceId,
        DateTime Timestamp,
        float Temperature,
        float Humidity,
        float CO2,
        bool RelayState,
        bool HeaterState,
        bool CoolerState
    );
}
