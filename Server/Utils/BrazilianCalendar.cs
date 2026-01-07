namespace ArxFlow.Server.Utils;

/// <summary>
/// Tipo de calendario brasileiro
/// </summary>
public enum BrazilianCalendarType
{
    /// <summary>Calendario ANBIMA (Settlement) - dias de liquidacao</summary>
    Settlement,

    /// <summary>Calendario B3 (Exchange) - dias de pregao</summary>
    Exchange,
}

/// <summary>
/// Calendário de feriados brasileiros baseado em QuantLib
/// </summary>
public static class BrazilianCalendar
{
    /// <summary>
    /// Verifica se uma data é dia útil no Brasil
    /// </summary>
    public static bool IsBusinessDay(DateTime date, BrazilianCalendarType type = BrazilianCalendarType.Settlement)
    {
        return type switch
        {
            BrazilianCalendarType.Settlement => IsSettlementBusinessDay(date),
            BrazilianCalendarType.Exchange => IsExchangeBusinessDay(date),
            _ => throw new ArgumentException("Tipo de calendário desconhecido", nameof(type))
        };
    }

    /// <summary>
    /// Verifica se é dia útil para Settlement (ANBIMA)
    /// </summary>
    private static bool IsSettlementBusinessDay(DateTime date)
    {
        DayOfWeek w = date.DayOfWeek;
        int d = date.Day;
        int m = (int)date.Month;
        int y = date.Year;
        int dd = date.DayOfYear;
        int em = EasterMonday(y);

        if (IsWeekend(w)
            // Ano Novo
            || (d == 1 && m == 1)
            // Dia de Tiradentes
            || (d == 21 && m == 4)
            // Dia do Trabalho
            || (d == 1 && m == 5)
            // Independência do Brasil
            || (d == 7 && m == 9)
            // Nossa Senhora Aparecida
            || (d == 12 && m == 10)
            // Finados
            || (d == 2 && m == 11)
            // Proclamação da República
            || (d == 15 && m == 11)
            // Dia da Consciência Negra (a partir de 2024)
            || (d == 20 && m == 11 && y >= 2024)
            // Natal
            || (d == 25 && m == 12)
            // Paixão de Cristo (Sexta-feira Santa)
            || (dd == em - 3)
            // Carnaval (segunda e terça)
            || (dd == em - 49 || dd == em - 48)
            // Corpus Christi
            || (dd == em + 59)
            )
            return false;

        return true;
    }

    /// <summary>
    /// Verifica se é dia útil para Exchange (B3)
    /// </summary>
    private static bool IsExchangeBusinessDay(DateTime date)
    {
        DayOfWeek w = date.DayOfWeek;
        int d = date.Day;
        int m = (int)date.Month;
        int y = date.Year;
        int dd = date.DayOfYear;
        int em = EasterMonday(y);

        if (IsWeekend(w)
            // Ano Novo
            || (d == 1 && m == 1)
            // Aniversário de São Paulo (até 2021)
            || (d == 25 && m == 1 && y < 2022)
            // Dia de Tiradentes
            || (d == 21 && m == 4)
            // Dia do Trabalho
            || (d == 1 && m == 5)
            // Revolução Constitucionalista (até 2021)
            || (d == 9 && m == 7 && y < 2022)
            // Independência do Brasil
            || (d == 7 && m == 9)
            // Nossa Senhora Aparecida
            || (d == 12 && m == 10)
            // Finados
            || (d == 2 && m == 11)
            // Proclamação da República
            || (d == 15 && m == 11)
            // Dia da Consciência Negra (a partir de 2007, exceto 2022 e 2023)
            || (d == 20 && m == 11 && y >= 2007 && y != 2022 && y != 2023)
            // Véspera de Natal
            || (d == 24 && m == 12)
            // Natal
            || (d == 25 && m == 12)
            // Paixão de Cristo (Sexta-feira Santa)
            || (dd == em - 3)
            // Carnaval (segunda e terça)
            || (dd == em - 49 || dd == em - 48)
            // Corpus Christi
            || (dd == em + 59)
            // Último dia útil do ano
            || (m == 12 && (d == 31 || (d >= 29 && w == DayOfWeek.Friday)))
            )
            return false;

        return true;
    }

    /// <summary>
    /// Verifica se é final de semana
    /// </summary>
    private static bool IsWeekend(DayOfWeek w)
    {
        return w == DayOfWeek.Saturday || w == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Calcula o dia do ano da Segunda-feira de Páscoa
    /// Usa o algoritmo de Meeus/Jones/Butcher
    /// </summary>
    private static int EasterMonday(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        // Domingo de Páscoa
        DateTime easter = new DateTime(year, month, day);

        // Segunda-feira de Páscoa
        DateTime easterMonday = easter.AddDays(1);

        return easterMonday.DayOfYear;
    }

    /// <summary>
    /// Calcula o número de dias úteis entre duas datas
    /// </summary>
    public static int BusinessDaysBetween(DateTime startDate, DateTime endDate, BrazilianCalendarType type = BrazilianCalendarType.Settlement)
    {
        if (startDate > endDate)
        {
            return -BusinessDaysBetween(endDate, startDate, type);
        }

        int businessDays = 0;
        DateTime current = startDate;

        while (current < endDate)
        {
            current = current.AddDays(1);
            if (IsBusinessDay(current, type))
            {
                businessDays++;
            }
        }

        return businessDays;
    }

    /// <summary>
    /// Adiciona dias úteis a uma data
    /// </summary>
    public static DateTime AddBusinessDays(DateTime date, int businessDays, BrazilianCalendarType type = BrazilianCalendarType.Settlement)
    {
        DateTime result = date;
        int daysToAdd = Math.Abs(businessDays);
        int direction = businessDays >= 0 ? 1 : -1;

        while (daysToAdd > 0)
        {
            result = result.AddDays(direction);
            if (IsBusinessDay(result, type))
            {
                daysToAdd--;
            }
        }

        return result;
    }

    /// <summary>
    /// Ajusta uma data para o próximo dia útil se não for dia útil
    /// </summary>
    public static DateTime AdjustToBusinessDay(DateTime date, BrazilianCalendarType type = BrazilianCalendarType.Settlement)
    {
        while (!IsBusinessDay(date, type))
        {
            date = date.AddDays(1);
        }
        return date;
    }

    /// <summary>
    /// Retorna o próximo dia útil
    /// </summary>
    public static DateTime NextBusinessDay(DateTime date, BrazilianCalendarType type = BrazilianCalendarType.Settlement)
    {
        date = date.AddDays(1);
        while (!IsBusinessDay(date, type))
            date = date.AddDays(1);
        return date;
    }

    /// <summary>
    /// Retorna o dia útil anterior
    /// </summary>
    public static DateTime PreviousBusinessDay(DateTime date, BrazilianCalendarType type = BrazilianCalendarType.Settlement)
    {
        date = date.AddDays(-1);
        while (!IsBusinessDay(date, type))
            date = date.AddDays(-1);
        return date;
    }
}
