﻿using System.ComponentModel;

namespace SystemKolekcjonerstwo.Models
{
    public class Element : INotifyPropertyChanged
    {
        private string name;
        private int price;
        private string status;
        private int rating;
        private string comment;
        private string imagePath;
        private Dictionary<string, object> additionalProperties = new();

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        public string Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public int Rating
        {
            get => rating;
            set
            {
                if (rating != value)
                {
                    rating = value;
                    OnPropertyChanged(nameof(Rating));
                }
            }
        }

        public string Comment
        {
            get => comment;
            set
            {
                if (comment != value)
                {
                    comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        public string ImagePath
        {
            get => imagePath;
            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                    OnPropertyChanged(nameof(ImagePath));
                }
            }
        }

        public Dictionary<string, object> AdditionalProperties
        {
            get => additionalProperties;
            set
            {
                if (additionalProperties != value)
                {
                    additionalProperties = value;
                    OnPropertyChanged(nameof(AdditionalProperties));
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetProperty(string propertyName, object value)
        {
            if (additionalProperties.ContainsKey(propertyName))
                additionalProperties[propertyName] = value;
            else
                additionalProperties.Add(propertyName, value);

            OnPropertyChanged(propertyName);
        }

        public object GetProperty(string propertyName)
        {
            if(additionalProperties.TryGetValue(propertyName, out object value))
                return value;

            return null;
        }


        public Element() { Name = string.Empty; }
        public Element(string name) { Name = name; }
    }
}
