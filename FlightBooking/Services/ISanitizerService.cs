namespace FlightBooking.Services
{
    public interface ISanitizerService
    {
        string SanitizeInput(string dirtyInput);
    }
}
