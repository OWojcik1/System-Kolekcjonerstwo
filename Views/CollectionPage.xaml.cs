using System.Collections.ObjectModel;

namespace SystemKolekcjonerstwo.Views;

public partial class CollectionPage : ContentPage
{
	public ObservableCollection<Models.Element> elements;
	public CollectionPage()
	{
		InitializeComponent();
		elements = new ObservableCollection<Models.Element>();
		elementsView.ItemsSource = elements;
	}

    private async void AddElement_Clicked(object sender, EventArgs e)
    {
		string name = await DisplayPromptAsync("Tworzenie elementu", "Podaj nazwê elementu", "OK", "Anuluj");

		if (string.IsNullOrEmpty(name))
			return;

		elements.Add(new Models.Element(name));
    }

    private void RemoveElement_Clicked(object sender, EventArgs e)
    {
		var selectedItem = elementsView.SelectedItem as Models.Element;

		if (selectedItem != null && elements.Contains(selectedItem))
			elements.Remove(selectedItem);
    }

    private async void EditElement_Clicked(object sender, EventArgs e)
    {
		var selectedItem = elementsView.SelectedItem as Models.Element;

		if (selectedItem == null && !elements.Contains(selectedItem))
			return;

		string newName = await DisplayPromptAsync("Edytowanie elementu", "Podaj now¹ nazwê elementu", "OK", "Anuluj", null, -1, null, selectedItem.Name);

		if (newName == selectedItem.Name || string.IsNullOrEmpty(newName))
			return;

		selectedItem.Name = newName;

    }
}