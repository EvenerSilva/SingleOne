using System.Collections.Generic;
using System;

namespace SingleOne.Util
{
    public class TimeZoneMapper
    {
        private static readonly Dictionary<string, string> LocaleToTimeZoneMap = new Dictionary<string, string>
    {
        { "pt-BR", "America/Sao_Paulo" },
        { "en-US", "America/New_York" },
        { "fr-FR", "Europe/Paris" },
        // Adicione mais mapeamentos conforme necessário
    };

        private static DateTime ConvertToLocaleDateTime(DateTime utcDateTime, string locale)
        {
            if (!LocaleToTimeZoneMap.TryGetValue(locale, out var timeZoneId))
            {
                throw new ArgumentException($"Fuso horário não encontrado para a localidade: {locale}", nameof(locale));
            }

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }

        public static DateTime GetDateTimeNow(string locale = "pt-BR")
        {
            DateTime utcNow = DateTime.UtcNow;
            return TimeZoneMapper.ConvertToLocaleDateTime(utcNow, locale);
        }
    }
}
