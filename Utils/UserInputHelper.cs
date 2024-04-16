namespace SystemKolekcjonerstwo.Utils
{
    public class UserInputHelper
    {
        public static async Task<double> GetPriceAsync(double currentPrice, Page page)
        {
            string priceString = await page.DisplayPromptAsync("Tworzenie elementu", "Podaj cenę elementu", "OK", "Anuluj", null, -1, null, currentPrice.ToString());

            return double.TryParse(priceString, out double price) ? price : 0;
        }

        public static async Task<string> GetStatusAsync(string currentStatus, Page page)
        {
            string status = await page.DisplayActionSheet("Tworzenie elementu", "Anuluj", null, "Nowy", "Użyty", "Na sprzedaż", "Sprzedany", "Chcę kupić");
            return status == "Anuluj" ? currentStatus : status;
        }

        public static async Task<int> GetRatingAsync(int currentRating, Page page)
        {
            string ratingString = await page.DisplayPromptAsync("Tworzenie elementu", "Podaj ocenę elementu (1-10)", "OK", "Anuluj", null, -1, null, currentRating.ToString());
            return int.TryParse(ratingString, out int rating) ? (rating >= 1 && rating <= 10) ? rating : 0 : 0;
        }

        public static async Task<string> GetCommentAsync(string currentComment, Page page)
        {
            string comment = await page.DisplayPromptAsync("Tworzenie elementu", "Dodaj komentarz", "OK", "Anuluj", null, -1, null, currentComment.ToString());
            return comment;
        }
    }
}
