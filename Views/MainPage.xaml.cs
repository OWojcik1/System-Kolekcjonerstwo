using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using SystemKolekcjonerstwo.Models;
using SystemKolekcjonerstwo.Views;

namespace SystemKolekcjonerstwo
{
    public partial class MainPage : ContentPage
    {

        public ObservableCollection<CollectionPage> CollectionPages = new();

        public MainPage()
        {
            InitializeComponent();
            LoadCollections();
            collectionPagesView.ItemsSource = CollectionPages;
        }

        private void LoadCollections()
        {
            string collectionsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Collections");
            if (Directory.Exists(collectionsFolder))
            {
                string[] collectionDirectories = Directory.GetDirectories(collectionsFolder);
                foreach (var collectionDirectory in collectionDirectories)
                {
                    string collectionName = new DirectoryInfo(collectionDirectory).Name;
                    CollectionPage collectionPage = new()
                    {
                        Name = collectionName
                    };

                    collectionPage.LoadCollection();
                    CollectionPages.Add(collectionPage);
                }
            }

            Debug.WriteLine($"Lokalizacja: {collectionsFolder}");
        }

        private async void AddCollection_Clicked(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Tworzenie kolekcji", "Podaj nazwe kolekcji");

            CollectionPage collectionPage = new()
            {
                Name = name
            };

            CollectionPages.Add(collectionPage);
            CreateCollectionFolder(name);
            await Navigation.PushAsync(collectionPage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            collectionPagesView.SelectedItem = null;
            foreach (var collectionPage in CollectionPages)
            {
                collectionPage.LoadCollection();
            }
        }

        private static void CreateCollectionFolder(string collectionName)
        {
            string collectionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Collections", collectionName);
            Directory.CreateDirectory(collectionFolder);
        }

        private async void GoToCollection_Clicked(object sender, EventArgs e)
        {
            if (collectionPagesView.SelectedItem != null)
            {
                CollectionPage selectedCollection = collectionPagesView.SelectedItem as CollectionPage;
                await Navigation.PushAsync(new CollectionPage(selectedCollection.Name));
            }
            else
            {
                await DisplayAlert("Błąd", "Wybierz Kolekcję", "Anuluj");
            }
        }

        private async void RemoveCollection_Clicked(object sender, EventArgs e)
        {
            if (collectionPagesView.SelectedItem != null)
            {
                CollectionPage selectedCollection = collectionPagesView.SelectedItem as CollectionPage;
                string collectionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Collections", selectedCollection.Name);
                if (Directory.Exists(collectionFolder))
                {
                    string filePath = Path.Combine(collectionFolder, "elements.txt");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    Directory.Delete(collectionFolder, true);
                    CollectionPages.Remove(selectedCollection);
                }
            }
            else
            {
                await DisplayAlert("Błąd", "Wybierz kolekcję do usunięcia", "Anuluj");
            }
        }
    }
}