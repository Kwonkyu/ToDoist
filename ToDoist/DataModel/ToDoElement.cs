using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace ToDoist.DataModel
{
    public class ToDosList
    {
        private ToDosList() { }
        public ToDosList(string name)
        {
            this.Name = name;

            toDoElementList = new ObservableCollection<ToDoElement>();
        }
        public ToDosList(string name, ObservableCollection<ToDoElement> ToDoElements)
        {
            this.name = name;
            this.toDoElementList = ToDoElements;
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }
        private ObservableCollection<ToDoElement> toDoElementList;
        public ObservableCollection<ToDoElement> ToDoElementList
        {
            get
            {
                return this.toDoElementList;
            }
            set
            {
                this.toDoElementList= value;
            }
        }

        public override bool Equals(object obj)
        {
            ToDosList TargetObject = (ToDosList)obj;
            return this.name.Equals(TargetObject.name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    public class ToDoElementBox : INotifyPropertyChanged
    {
        private ToDoElement toDoElementItem;
        public ToDoElement ToDoElementItem
        {
            get
            {
                return this.toDoElementItem;
            }
            set
            {
                this.toDoElementItem = value;
                NotifyPropertyChanged("ToDoElementItem");
            }
        }

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ToDoElement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int elementID;
        public int ElementID {
            get
            {
                return elementID;
            }
            set 
            {
                this.elementID = value;
                this.OnPropertyChanged("ElementID");
            }
        }
        private string expirationDate;
        public string ExpirationDate
        {
            get
            {
                return expirationDate;
            }
            set
            {
                this.expirationDate = value;
                this.OnPropertyChanged("ExpirationDate");
            }
        }
        private string content;
        public string Content {
            get
            {
                return content;
            }
            set
            {
                this.content = value;
                this.OnPropertyChanged("Content");
            }
        }
        private bool done;
        public bool Done {
            get
            {
                return done;
            }
            set
            {
                this.done = value;
                this.OnPropertyChanged("Done");
            }
        }
        private Visibility visible;
        public Visibility Visible {
            get
            {
                return visible;
            }
            set
            {
                this.visible = value;
                this.OnPropertyChanged("Visible");
            }
        }
        private bool important;
        public bool Important
        {
            get
            {
                return important;
            }
            set
            {
                this.important = value;
                this.OnPropertyChanged("Important");
            }
        }

        public bool IsDeadline()
        {
            var currentTime = DateTime.Now.Date;
            var elementTime = DateTime.Parse(ExpirationDate);
            return (DateTime.Compare(elementTime, currentTime) == 0);
        }
        public bool IsExpired()
        {
            var currentTime = DateTime.Now.Date;
            var elementTIme = DateTime.Parse(ExpirationDate);
            if(DateTime.Compare(currentTime, elementTIme) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public override bool Equals(object obj)
        {
            ToDoElement TargetObject = (ToDoElement)obj;
            return this.elementID == TargetObject.elementID;
            // TODO: add more conditions?
        }

        

        protected void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private ToDoElement() { }

        public ToDoElement(DateTime expirationDate, string content, bool isDone, bool isImportant)
        {
            this.ExpirationDate = expirationDate.ToString("d");
            this.Content = content;
            this.Done = isDone;
            this.Visible = Visibility.Visible;
            this.important = isImportant;
            this.ElementID = this.GetHashCode();
        }

        public ToDoElement(string expirationDate, string content, bool isDone, bool isImportant)
        {
            this.ExpirationDate = expirationDate;
            this.Content = content;
            this.Done = isDone;
            this.Visible = Visibility.Visible;
            this.important = isImportant;
            this.ElementID = this.GetHashCode();
        }

        public ToDoElement copy()
        {
            var retVal = new ToDoElement();
            retVal.ElementID = this.ElementID;
            retVal.ExpirationDate = this.ExpirationDate;
            retVal.Content = this.Content;
            retVal.Done = this.Done;
            retVal.Important = this.Important;
            retVal.Visible = this.Visible;
            return retVal;
        }
    }
}
