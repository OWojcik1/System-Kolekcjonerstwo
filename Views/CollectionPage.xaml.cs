using System.Collections.ObjectModel;
using System.Text;

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
            sb.Append($"{element.Name},{element.Price},{element.Status},{element.Rating},{element.Comment}, {element.ImagePath}");
            foreach (var property in element.AdditionalProperties)
            {
                if (property.Value is Tuple<List<string>, string> tupleProperty)
                {
                    sb.Append($",{property.Key}:{string.Join(";", tupleProperty.Item1)}:{tupleProperty.Item2}");
                }
                else
                {
                    sb.Append($",{property.Key}:{property.Value}");
                }
            }
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
                string[] values = line.Split(',');
                if (values.Length >= 6)
                {
                    Models.Element newElement = new()
                    {
                        Name = values[0],
                        Price = Convert.ToDouble(values[1]),
                        Status = values[2],
                        Rating = Convert.ToInt32(values[3]),
                        Comment = values[4],
                        ImagePath = values[5]
                    };
                    for (int i = 6; i < values.Length; i++)
                    {
                        string[] propertyPair = values[i].Split(':');
                        if (propertyPair.Length == 3)
                        {
                            string propertyName = propertyPair[0];
                            string[] propertyValues = propertyPair[1].Split(';');
                            string selectedValue = propertyPair[2];

                            if (propertyValues.Length > 0)
                            {
                                List<string> predefinedValues = new(propertyValues);
                                newElement.SetProperty(propertyName, new Tuple<List<string>, string>(predefinedValues, selectedValue));
                            }
                            else
                            {
                                newElement.SetProperty(propertyName, selectedValue);
                            }
                        }
                        else if (propertyPair.Length == 2)
                        {
                            string propertyName = propertyPair[0];
                            string propertyValue = propertyPair[1];
                            newElement.SetProperty(propertyName, propertyValue);
                        }
                    }
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

        double price = await GetPriceAsync(0);
        string status = await GetStatusAsync("");
        int rating = await GetRatingAsync(1);
        string comment = await GetCommentAsync("");

        Elements.Add(new Models.Element { Name = name, Price = price, Status = status, Rating = rating, Comment = comment, ImagePath = (file != null) ? file.FullPath : string.Empty });

        SaveCollection();
    }

    private void RemoveElement_Clicked(object sender, EventArgs e)
    {
        var selectedItem = elementsView.SelectedItem as Models.Element;

        if (selectedItem != null && Elements.Contains(selectedItem))
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

        double newPrice = await GetPriceAsync(selectedItem.Price);
        string newStatus = await GetStatusAsync(selectedItem.Status);
        int newRating = await GetRatingAsync(selectedItem.Rating);
        string newComment = await GetCommentAsync(selectedItem.Comment);

        FileResult file = await MediaPicker.Default.PickPhotoAsync();


        Dictionary<string, object> editedProperties = new(selectedItem.AdditionalProperties);
        foreach (var property in selectedItem.AdditionalProperties)
        {
            if (property.Value is string || property.Value is double)
            {
                string newValue = await DisplayPromptAsync("Edycja w³aœciwoœci", $"Aktualna wartoœæ dla w³aœciwoœci '{property.Key}': {property.Value}", "OK", "Anuluj", null, -1, null, property.Value.ToString());
                if (newValue != null)
                    editedProperties[property.Key] = property.Value is double ? double.Parse(newValue) : newValue;
            }
            else if (property.Value is Tuple<List<string>, string> tuple)
            {

                while (true)
                {
                    string newListValue = await DisplayPromptAsync("Edycja wartoœci", $"Aktualne wartoœci dla w³aœciwoœci '{property.Key}': {string.Join(", ", tuple.Item1)}.\nPodaj now¹ wartoœæ lub kliknij Anuluj, aby zakoñczyæ", "OK", "Anuluj");

                    if (newListValue == null)
                        break;

                    if (!tuple.Item1.Contains(newListValue))
                        tuple.Item1.Add(newListValue);
                }
                string newValue = await DisplayActionSheet("Wybierz wartoœæ", null, null, tuple.Item1.ToArray());

                if(newValue == tuple.Item2)
                {
                    editedProperties[property.Key] = tuple;
                    return;
                }

                List<string> tempList = tuple.Item1;

                Tuple<List<string>, string> newTuple = new(tempList, newValue);
                editedProperties[property.Key] = newTuple;
            }
        }

        Models.Element newElement = new()
        {
            Name = newName,
            Price = newPrice,
            Status = newStatus,
            Rating = newRating,
            Comment = newComment,
            ImagePath = (file!= null) ? file.FullPath : string.Empty,
            AdditionalProperties = editedProperties
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


    private async void AddProperty_Clicked(object sender, EventArgs e)
    {
        if (elementsView.SelectedItem == null)
        {
            await DisplayAlert("B³¹d", "Proszê wybraæ element.", "OK");
            return;
        }

        Models.Element selectedElement = elementsView.SelectedItem as Models.Element;

        string propertyName = await DisplayPromptAsync("Nowa w³aœciwoœæ", "Podaj nazwê nowej w³aœciwoœci", "OK", "Anuluj");

        if (propertyName == null)
            return;

        string[] propertyTypes = { "Tekstowa", "Liczbowa", "Zestaw wartoœci do wyboru" };
        string propertyType = await DisplayActionSheet("Wybierz typ w³aœciwoœci", "Anuluj", null, propertyTypes);

        if (propertyType == "Anuluj")
            return;

        object propertyValue = null;

        if (propertyType == "Tekstowa")
        {
            propertyValue = await DisplayPromptAsync("Nowa w³aœciwoœæ", $"Podaj wartoœæ dla w³aœciwoœci '{propertyName}'", "OK", "Anuluj");
        }
        else if (propertyType == "Liczbowa")
        {
            string propertyValueString = await DisplayPromptAsync("Nowa w³aœciwoœæ", $"Podaj wartoœæ liczbow¹ dla w³aœciwoœci '{propertyName}'", "OK", "Anuluj");

            double numericValue;

            if (double.TryParse(propertyValueString, out numericValue))
            {
                propertyValue = numericValue;
            }
            else
            {
                await DisplayAlert("B³¹d", "Podana wartoœæ nie jest liczb¹.", "OK");
                return;
            }
        }
        else if (propertyType == "Zestaw wartoœci do wyboru")
        {
            List<string> predefinedValues = new List<string>();

            while (true)
            {
                string newValue = await DisplayPromptAsync("Nowa wartoœæ", "Podaj now¹ wartoœæ lub kliknij Anuluj, aby zakoñczyæ", "OK", "Anuluj");

                if (newValue == null)
                    break;

                predefinedValues.Add(newValue);
            }

            if (predefinedValues.Count == 0)
                return;

            string selectedValue = await DisplayActionSheet("Wybierz wartoœæ", null, null, predefinedValues.ToArray());

            if (selectedValue == null || selectedValue == "")
                return;

            propertyValue = new Tuple<List<string>, string>(predefinedValues, selectedValue);
        }

        if (propertyValue == null)
            return;

        selectedElement.SetProperty(propertyName, propertyValue);

        await DisplayAlert("Sukces", $"Nowa w³aœciwoœæ '{propertyName}' zosta³a dodana do elementu.", "OK");
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


    private async Task<double> GetPriceAsync(double currentPrice)
    {
        string priceString = await DisplayPromptAsync("Tworzenie elementu", "Podaj cenê elementu", "OK", "Anuluj", null, -1, null, currentPrice.ToString());
        return double.TryParse(priceString, out double price) ? price : 0;
    }

    private async Task<string> GetStatusAsync(string currentStatus)
    {
        string status = await DisplayActionSheet("Tworzenie elementu", "Anuluj", null, "Nowy", "U¿yty", "Na sprzeda¿", "Sprzedany", "Chcê kupiæ");
        return status == "Anuluj" ? currentStatus : status;
    }

    private async Task<int> GetRatingAsync(int currentRating)
    {
        string ratingString = await DisplayPromptAsync("Tworzenie elementu", "Podaj ocenê elementu (1-10)", "OK", "Anuluj", null, -1, null, currentRating.ToString());
        return int.TryParse(ratingString, out int rating) ? (rating >= 1 && rating <= 10) ? rating : 0 : 0;
    }

    private async Task<string> GetCommentAsync(string currentComment)
    {
        string comment = await DisplayPromptAsync("Tworzenie elementu", "Dodaj komentarz", "OK", "Anuluj", null, -1, null, currentComment.ToString());
        return comment;
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
                string[] values = line.Split(',');
                if (values.Length >= 6)
                {
                    Models.Element newElement = new()
                    {
                        Name = values[0],
                        Price = Convert.ToDouble(values[1]),
                        Status = values[2],
                        Rating = Convert.ToInt32(values[3]),
                        Comment = values[4],
                        ImagePath = values[5]
                    };
                    for (int i = 6; i < values.Length; i++)
                    {
                        string[] propertyPair = values[i].Split(':');
                        if (propertyPair.Length == 3)
                        {
                            string propertyName = propertyPair[0];
                            string[] propertyValues = propertyPair[1].Split(';');
                            string selectedValue = propertyPair[2];

                            if (propertyValues.Length > 0)
                            {
                                List<string> predefinedValues = new(propertyValues);
                                newElement.SetProperty(propertyName, new Tuple<List<string>, string>(predefinedValues, selectedValue));
                            }
                            else
                            {
                                newElement.SetProperty(propertyName, selectedValue);
                            }
                        }
                        else if (propertyPair.Length == 2)
                        {
                            string propertyName = propertyPair[0];
                            string propertyValue = propertyPair[1];
                            newElement.SetProperty(propertyName, propertyValue);
                        }
                    }

                    bool foundConflict = false;
                    foreach (var existingElement in Elements)
                    {
                        if (existingElement.Name == newElement.Name)
                        {
                            foundConflict = true;
                            break;
                        }
                    }

                    if (!foundConflict)
                    {
                        Elements.Add(newElement);
                    }
                }
            }

            SaveCollection();
        }

    }
    private async Task ExportCollection()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var element in Elements)
        {
            sb.AppendLine($"{element.Name},{element.Price},{element.Status},{element.Rating},{element.Comment},{element.ImagePath}");
            foreach (var property in element.AdditionalProperties)
            {
                if (property.Value is Tuple<List<string>, string> tupleProperty)
                {
                    sb.AppendLine($"{property.Key}:{string.Join(";", tupleProperty.Item1)}:{tupleProperty.Item2}");
                }
                else
                {
                    sb.AppendLine($"{property.Key}:{property.Value}");
                }
            }
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
}