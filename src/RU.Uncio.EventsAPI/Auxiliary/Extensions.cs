namespace RU.Uncio.EventsAPI
{
    internal static class Extensions
    {
        public static bool IsStrictlyGreaterThan(this DateTime dateTime, DateTime dateTimeToCompare)
        {
            if (dateTime.Date > dateTimeToCompare.Date)
                return true;
            else if(dateTime.Date == dateTimeToCompare.Date)
            {
                if (dateTime.Hour > dateTimeToCompare.Hour)
                    return true;
                else if (dateTime.Hour == dateTimeToCompare.Hour)
                {
                    if (dateTime.Minute > dateTimeToCompare.Minute)
                        return true;
                }                
            }

            return false;
        }
    }
}
