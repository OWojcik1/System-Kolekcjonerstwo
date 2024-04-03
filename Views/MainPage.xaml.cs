using SystemKolekcjonerstwo.Views;

namespace SystemKolekcjonerstwo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private async void addCollection_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CollectionPage());
        }
    }
}