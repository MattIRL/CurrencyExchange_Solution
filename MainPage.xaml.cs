using System.Text.Json;

namespace CurrencyExchange
{
    public partial class MainPage : ContentPage
    {
    private List<string> currencyList = new List<string>
        {"AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN", "BAM", "BBD", "BDT", "BGN",
        "BHD", "BIF", "BMD", "BND", "BOB", "BRL", "BSD", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHF",
        "CLP", "CNY", "COP", "CRC", "CUP", "CVE", "CZK", "DJF", "DKK", "DOP", "DZD", "EGP", "ERN", "ETB",
        "EUR", "FJD", "FKP", "FOK", "GBP", "GEL", "GGP", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD", "HNL",
        "HRK", "HTG", "HUF", "IDR", "ILS", "IMP", "INR", "IQD", "IRR", "ISK", "JEP", "JMD", "JOD", "JPY",
        "KES", "KGS", "KHR", "KID", "KMF", "KRW", "KWD", "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL",
        "LYD", "MAD", "MDL", "MGA", "MKD", "MMK", "MNT", "MOP", "MRU", "MUR", "MVR", "MWK", "MXN", "MYR",
        "MZN", "NAD", "NGN", "NIO", "NOK", "NPR", "NZD", "OMR", "PAB", "PEN", "PGK", "PHP", "PKR", "PLN",
        "PYG", "QAR", "RON", "RSD", "RUB", "RWF", "SAR", "SBD", "SCR", "SDG", "SEK", "SGD", "SHP", "SLE",
        "SLL", "SOS", "SRD", "SSP", "STN", "SYP", "SZL", "THB", "TJS", "TMT", "TND", "TOP", "TRY", "TTD",
        "TVD", "TWD", "TZS", "UAH", "UGX", "USD", "UYU", "UZS", "VES", "VND", "VUV", "WST", "XAF", "XCD",
        "XCG", "XDR", "XOF", "XPF", "YER", "ZAR", "ZMW", "ZWL"};

        private readonly HttpClient httpClient = new HttpClient();
        private const string ApiKey = "c4961e9f3d777a718be0b652";

        public MainPage()
        {
            InitializeComponent();
            FromCurrencyPicker.ItemsSource = currencyList;
            ToCurrencyPicker.ItemsSource = currencyList;

            apiResponseEditor.IsVisible = false;
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            // Parse decimal from User Entry
            if (!decimal.TryParse(ValueDecimal.Text, out decimal amount))
            {
                CurrencyResult.Text = "Invalid decimal value. Please try again.";
                return;
            }

            // Get selected currencies
            string? fromCurrency = FromCurrencyPicker.SelectedItem?.ToString();
            string? toCurrency = ToCurrencyPicker.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
            {
                CurrencyResult.Text = "Please select both currencies.";
                return;
            }


            // Live API Call (async)

            try
            {
                string url = $"https://v6.exchangerate-api.com/v6/{ApiKey}/latest/{fromCurrency}";
                // string url = $"https://open.er-api.com/v6/latest/{fromCurrency}"; // For open API calls (no key)
                var response = await httpClient.GetStringAsync(url);

                // Show raw JSON in collapsible editor
                apiResponseEditor.Text = JsonSerializer.Serialize(
                    JsonDocument.Parse(response).RootElement,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                //string pairKey = $"{toCurrency}";
                decimal liveRate = root.GetProperty("conversion_rates").GetProperty(toCurrency).GetDecimal();
                string? Provider = root.GetProperty("documentation").GetString();
                decimal convertedAmount = amount * liveRate;

                // Update UI with real data
                CurrencyResult.Text = $"{amount:F2} {fromCurrency} is worth {convertedAmount:F2} {toCurrency}.";
                AdditionalInfo.Text = $"\nThis is an Exchange rate of 1 : {liveRate:F4}" +
                    $"\nThis information is provided by\n{Provider}";

                roboImage.Source = $"https://www.robohash.org/{convertedAmount}{toCurrency}.png";
                roboName.Text = $"This robot is named '{convertedAmount:F2} {toCurrency}'.";
            }
            catch (Exception ex)
            {
                CurrencyResult.Text = $"Error retrieving exchange rate: {ex.Message}";
            }
        }

        private void OnToggleJsonClicked(object sender, EventArgs e)
        {
            apiResponseEditor.IsVisible = !apiResponseEditor.IsVisible;
        }
    }
}
