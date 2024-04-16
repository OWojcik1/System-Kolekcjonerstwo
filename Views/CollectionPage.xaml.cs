using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using SystemKolekcjonerstwo.Utils;

namespace SystemKolekcjonerstwo.Views;

public partial class CollectionPage : ContentPage
{
    public string Name { get; set; }
    public ObservableCollection<Models.Element> Elements;
    public CollectionPage()
    {
        InitializeComponent();
        Elements = new ObservableCollection<Models.Element>();
        elementsView.ItemsSource = Elements;
        BindingContext = this;
    }

    public CollectionPage(string collectionName)
    {
        InitializeComponent();
        Name = collectionName;

        Elements = new ObservableCollection<Models.Element>();
        LoadCollection();

        elementsView.ItemsSource = Elements;

        BindingContext = this;
    }


    public void SaveCollection()
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Collections", Name, "elements.txt");
        StringBuilder sb = new();
        foreach (var element in Elements)
        {
            sb.Append($"{element.Name};{element.Price.ToString(CultureInfo.InvariantCulture)};{element.Status};{element.Rating};{element.Comment};{element.ImagePath}");
            sb.AppendLine();
        }
        File.WriteAllText(filePath, sb.ToString());
    }


    public void LoadCollection()
    {
        string collectionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Collections", Name);
        string filePath = Path.Combine(collectionFolder, "elements.txt");
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                Models.Element newElement = ParseElementFromString(line);
                if (newElement != null)
                {
                    Elements.Add(newElement);
                }
            }
        }
    }

    private async void AddElement_Clicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Tworzenie elementu", "Podaj nazwê elementu", "OK", "Anuluj");

        if (string.IsNullOrEmpty(name))
            return;

        if(Elements.Any(element => element.Name == name))
        {
            await DisplayAlert("B³¹d", "Element o tej nazwie ju¿ istnieje w kolekcji.", "OK");
            return;
        }

        FileResult file = await MediaPicker.Default.PickPhotoAsync();

        double price = await UserInputHelper.GetPriceAsync(0, this);
        string status = await UserInputHelper.GetStatusAsync("", this);
        int rating = await UserInputHelper.GetRatingAsync(1, this);
        string comment = await UserInputHelper.GetCommentAsync("", this);

        Elements.Add(new Models.Element { Name = name, Price = price, Status = status, Rating = rating, Comment = comment, ImagePath = (file != null) ? file.FullPath : string.Empty });

        SaveCollection();
    }

    private void RemoveElement_Clicked(object sender, EventArgs e)
    {
        if (elementsView.SelectedItem is Models.Element selectedItem && Elements.Contains(selectedItem))
            Elements.Remove(selectedItem);

        SaveCollection();
    }


    private async void EditElement_Clicked(object sender, EventArgs e)
    {
        if (elementsView.SelectedItem is not Models.Element selectedItem || !Elements.Contains(selectedItem))
            return;

        string newName = await DisplayPromptAsync("Edytowanie elementu", "Podaj now¹ nazwê elementu", "OK", "Anuluj", null, -1, null, selectedItem.Name);
        if (string.IsNullOrEmpty(newName))
            return;

        double newPrice = await UserInputHelper.GetPriceAsync(selectedItem.Price, this);
        string newStatus = await UserInputHelper.GetStatusAsync(selectedItem.Status, this);
        int newRating = await UserInputHelper.GetRatingAsync(selectedItem.Rating, this);
        string newComment = await UserInputHelper.GetCommentAsync(selectedItem.Comment, this);

        FileResult file = await MediaPicker.Default.PickPhotoAsync();

        Models.Element newElement = new()
        {
            Name = newName,
            Price = newPrice,
            Status = newStatus,
            Rating = newRating,
            Comment = newComment,
            ImagePath = (file != null) ? file.FullPath : selectedItem.ImagePath,
        };

        if (newStatus.ToLower() == "sprzedany")
        {
            Elements.Remove(selectedItem);
            Elements.Add(newElement);
        }
        else
        {
            Elements[Elements.IndexOf(selectedItem)] = newElement;
        }

        SaveCollection();
    }

    private async void ImportCollection_Clicked(object sender, EventArgs e)
    {
        await ImportCollection();
    }

    private async void ExportCollection_Clicked(object sender, EventArgs e)
    {
        await ExportCollection();
    }

    private async Task ImportCollection()
    {
        FileResult file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Wybierz plik do importu",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".txt" } }
        })
        });

        if (file != null)
        {
            string filePath = file.FullPath;
            string[] lines = await File.ReadAllLinesAsync(filePath);

            foreach (string line in lines)
            {
                Models.Element newElement = ParseElementFromString(line);
                if (newElement != null && !Elements.Any(element => element.Name == newElement.Name))
                {
                    Elements.Add(newElement);
                }
            }

            SaveCollection();
        }
    }

    private async Task ExportCollection()
    {
        StringBuilder sb = new();
        foreach (var element in Elements)
        {
            sb.AppendLine($"{element.Name};{element.Price.ToString(CultureInfo.InvariantCulture)};{element.Status};{element.Rating};{element.Comment};{element.ImagePath}");
            sb.AppendLine();
        }

        var file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Wybierz plik do zapisania",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".txt" } }
        })
        });

        if (file != null)
        {
            File.WriteAllText(file.FullPath, sb.ToString());
        }
    }


    private async void GenerateCollectionSummary_Clicked(object sender, EventArgs e)
    {
        int totalElements = Elements.Count;
        int soldElements = Elements.Count(element => element.Status.ToLower() == "sprzedany");
        int sellIntentionElements = Elements.Count(element => element.Status.ToLower() == "na sprzeda¿" || element.Status.ToLower() == "chcê kupiæ");

        string summaryText = $"Podsumowanie kolekcji '{Name}'\n\n";
        summaryText += $"* Liczba posiadanych przedmiotów: {totalElements}\n";
        summaryText += $"* Liczba sprzedanych przedmiotów: {soldElements}\n";
        summaryText += $"* Liczba przedmiotów przeznaczonych na sprzeda¿: {sellIntentionElements}\n";

        await DisplayAlert("Podsumowanie kolekcji", summaryText, "OK");
    }


    private static Models.Element ParseElementFromString(string line)
    {
        string[] values = line.Split(';');
        if (values.Length < 6)
            return null;

        Models.Element newElement = new()
        {
            Name = values[0],
            Price = Convert.ToDouble(values[1], CultureInfo.InvariantCulture),
            Status = values[2],
            Rating = Convert.ToInt32(values[3]),
            Comment = values[4],
            ImagePath = values[5]
        };

        return newElement;
    }
}