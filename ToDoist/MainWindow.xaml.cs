using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ToDoist.DataModel;
using System.Text.Json;
using System.IO;
using System.Windows.Markup;
using System.Globalization;

namespace ToDoist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ToDoElement> ObservableToDoElements = null;
        private ObservableCollection<ToDosList> ObservableToDosList = null;
        public ToDoElementBox SelectedToDoBox = new ToDoElementBox() { ToDoElementItem = null };
        private string DataFileLocation = null;

        public MainWindow()
        {
            InitializeComponent();
            InitStorage();
            InitToolTips();
            LoadToDoList();
            InitSelectedToDoElementChangedHandlers();
            AllToDosRadioButton.IsChecked = true;
        }
        

        private void InitStorage()
        {
            string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToDoist");
            string file = System.IO.Path.Combine(directory, "todos.json");
            DataFileLocation = file;
        }

        private void LoadToDoList()
        {
            try
            {
                byte[] Data = File.ReadAllBytes(DataFileLocation);
                List<object> DataInputFormat = JsonSerializer.Deserialize<List<object>>(Data, new JsonSerializerOptions { WriteIndented = true });
                if (DataInputFormat.Count < 2) throw new JsonException("ToDoElements or ToDosList does not exist.");
                else
                {
                    List<ToDoElement> ToDoElements = JsonSerializer.Deserialize<List<ToDoElement>>(DataInputFormat[0].ToString(), new JsonSerializerOptions { WriteIndented = true });
                    List<ToDosList> ToDosLists = JsonSerializer.Deserialize<List<ToDosList>>(DataInputFormat[1].ToString(), new JsonSerializerOptions { WriteIndented = true });
                    ObservableToDoElements = new ObservableCollection<ToDoElement>(ToDoElements);
                    ObservableToDosList = new ObservableCollection<ToDosList>(ToDosLists);
                }
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show("FILE NOT FOUND EXCEPTION!");
            }
            catch (XamlParseException e)
            {
                MessageBox.Show("XAML PARSE EXCEPTION!");
            }
            catch (JsonException e)
            {
                MessageBox.Show("JSON EXCEPTION!");
            }
            finally
            {
                if (ObservableToDoElements == null) ObservableToDoElements = new ObservableCollection<ToDoElement>();
                ToDoList.ItemsSource = ObservableToDoElements;
                ObservableToDoElements.CollectionChanged += ObservableToDoElements_CollectionChanged;

                if (ObservableToDosList == null) ObservableToDosList = new ObservableCollection<ToDosList>();
                ToDosListComboBox.ItemsSource = ObservableToDosList;
                ToDoInformationToDosList.ItemsSource = ObservableToDosList;
            }
        }

        private void ObservableToDoElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                ToDoElement RemovedElement = (ToDoElement)e.OldItems[0];
                ToDosList RemovedElementToDosList = (ToDosList)ToDoInformationToDosList.SelectedItem;
                if(RemovedElementToDosList != null) RemovedElementToDosList.ToDoElementList.Remove(RemovedElement);
            }
        }

        private void SaveToDoList()
        {
            // TODO: check if ToDoElement is exist or not.
            List<object> DataOutputFormat = new List<object>();
            DataOutputFormat.Add(ObservableToDoElements.ToList());
            DataOutputFormat.Add(ObservableToDosList.ToList());

            //byte[] datas = JsonSerializer.SerializeToUtf8Bytes(ObservableToDoElements, new JsonSerializerOptions { WriteIndented = true });
            //File.WriteAllBytes(DataFileLocation, datas);
            byte[] Data = JsonSerializer.SerializeToUtf8Bytes(DataOutputFormat, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllBytes(DataFileLocation, Data);
        }

        private void InitToolTips()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            FormString.ToolTip = new ToolTip()
            {
                Content = "Write down your plans here!",
                Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
                PlacementTarget = FormString,
                HorizontalOffset = 20,
                VerticalOffset = -20,
            };
            FormDateTime.ToolTip = new ToolTip()
            {
                Content = "Pick an expiration date of your plans!",
                Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
                PlacementTarget = FormDateTime,
                HorizontalOffset = 20,
                VerticalOffset = -20,
            };
            ToDosListAddTextBox.ToolTip = new ToolTip()
            {
                Content = "Enter a unique name of new list",
                Placement = System.Windows.Controls.Primitives.PlacementMode.Relative,
                PlacementTarget = ToDosListAddTextBox,
                HorizontalOffset = 20,
                VerticalOffset = -20,
            };
        }

        private void InitSelectedToDoElementChangedHandlers()
        {
            SelectedToDoBox.PropertyChanged += UpdateToDoInformationContainer;
            SelectedToDoBox.PropertyChanged += UpdateToDoElementContextMenu;
        }

        private void UpdateToDoInformationContainer(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SelectedToDoBox.ToDoElementItem != null)
            {
                ToDoInformationContainer.IsEnabled = true;

                // TODO: bind these elements into controls.
                var item = SelectedToDoBox.ToDoElementItem;
                ToDoInformationDone.IsChecked = item.Done;
                ToDoInformationSummary.Text = item.Content.Length > 20 ? item.Content.Substring(0, 20) + "..." : item.Content;
                ToDoInformationExpirationDate.SelectedDate = DateTime.Parse(item.ExpirationDate);
                ToDoInformationContent.Text = item.Content;
                ToDoInformationImportant.IsChecked = item.Important;

                bool isFound = false;
                foreach(ToDosList toDosList in ObservableToDosList)
                {
                    if (toDosList.ToDoElementList.Contains(SelectedToDoBox.ToDoElementItem))
                    {
                        ToDoInformationToDosList.SelectedItem = toDosList;
                        isFound = true;
                        break;
                    }
                }
                if (!isFound) ToDoInformationToDosList.SelectedIndex = -1;
            }
            else
            {
                ToDoInformationContainer.IsEnabled = false;
            }
        }
        private void UpdateToDoElementContextMenu(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ToDoListContextMenuDelete.IsEnabled = SelectedToDoBox.ToDoElementItem != null;
            ToDoListContextMenuImportant.IsEnabled = SelectedToDoBox.ToDoElementItem != null;
        }

        // ToDoList
        private void ToDoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedToDoBox.ToDoElementItem = (ToDoElement)ToDoList.SelectedItem;
        }

        // ToDoList ContextMenu
        private void ToDoListContextMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            int position = ObservableToDoElements.IndexOf(SelectedToDoBox.ToDoElementItem);
            ObservableToDoElements.Remove(ObservableToDoElements[position]);
            ToDoList.SelectedItem = ObservableToDoElements.Count > 0 ? ObservableToDoElements[position > 0 ? position - 1 : 0] : null;
        }
        private void ToDoListContextMenuImportant_Click(object sender, RoutedEventArgs e)
        {
            SelectedToDoBox.ToDoElementItem.Important = !SelectedToDoBox.ToDoElementItem.Important;
            ToDoInformationImportant.IsChecked = SelectedToDoBox.ToDoElementItem.Important;
        }


        // ToDo Sort RadioButtons
        private void AllToDosRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var element in ObservableToDoElements)
            {
                element.Visible = (ImportantToDosCheckBox.IsChecked == true) ?
                    (element.Important ? Visibility.Visible : Visibility.Collapsed) :
                    Visibility.Visible;
            }
            SelectFirstRowOfUpdatedToDoElements();
        }
        private void TodaysToDosRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var element in ObservableToDoElements)
            {
                element.Visible = (ImportantToDosCheckBox.IsChecked == true) ?
                    (element.Important && element.IsDeadline() ? Visibility.Visible : Visibility.Collapsed) :
                    (element.IsDeadline() ? Visibility.Visible : Visibility.Collapsed);
            }
            SelectFirstRowOfUpdatedToDoElements();
        }
        private void ExpiratedToDosRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var element in ObservableToDoElements)
            {
                element.Visible = (ImportantToDosCheckBox.IsChecked == true) ?
                    (element.IsExpired() && element.Important ? Visibility.Visible : Visibility.Collapsed) :
                    (element.IsExpired() ? Visibility.Visible : Visibility.Collapsed);
            }
            SelectFirstRowOfUpdatedToDoElements();
        }
        private void NotDoneToDosRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var element in ObservableToDoElements)
            {
                element.Visible = (ImportantToDosCheckBox.IsChecked == true) ?
                    (!element.Done && element.Important ? Visibility.Visible : Visibility.Collapsed) :
                    (!element.Done ? Visibility.Visible : Visibility.Collapsed);
            }
            SelectFirstRowOfUpdatedToDoElements();
        }
        private void ImportantToDosCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            // TODO: if radio button is clicked...?
            if (ImportantToDosCheckBox.IsChecked == true)
            {
                if(AllToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = element.Important ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if(TodaysToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = element.Important && element.ExpirationDate.Equals(DateTime.Now.Date.ToString("d")) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if(ExpiratedToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = element.Important && element.IsExpired() ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if(NotDoneToDosRadioButton.IsChecked == true)
                {
                    foreach(var element in ObservableToDoElements)
                    {
                        element.Visible = element.Important && !element.Done ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            else
            {
                if (AllToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = Visibility.Visible;
                    }
                }
                else if (TodaysToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = element.ExpirationDate.Equals(DateTime.Now.Date.ToString("d")) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if (ExpiratedToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = element.IsExpired() ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                else if (NotDoneToDosRadioButton.IsChecked == true)
                {
                    foreach (var element in ObservableToDoElements)
                    {
                        element.Visible = !element.Done ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            SelectFirstRowOfUpdatedToDoElements();
        }
        private void ToDosListRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if(ToDosListComboBox.SelectedIndex < 0)
            {
                ToDosListComboBox.SelectedIndex = 0;
            }
            else
            {
                ShowToDoElementsBasedOnSelectedList(ToDosListComboBox.SelectedItem);
            }
        }
        private void ToDosListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToDosListComboBox.SelectedIndex >= 0)
            {
                ToDosListRadioButton.IsChecked = true;
                ShowToDoElementsBasedOnSelectedList(ToDosListComboBox.SelectedItem);
            }
        }
        private void ShowToDoElementsBasedOnSelectedList(object _SelectedList)
        {
            ToDosList SelectedList = (ToDosList)_SelectedList;
            foreach(var ToDoElement in ObservableToDoElements)
            {
                ToDoElement.Visible = SelectedList.ToDoElementList.Contains(ToDoElement) ? Visibility.Visible : Visibility.Collapsed;
            }
            SelectFirstRowOfUpdatedToDoElements();
        }
        private void SelectFirstRowOfUpdatedToDoElements()
        {
            foreach(var ToDoElement in ObservableToDoElements)
            {
                if(ToDoElement.Visible == Visibility.Visible)
                {
                    ToDoList.SelectedItem = ToDoElement;
                    return;
                }
            }
            ToDoList.SelectedItem = null;
        }
        private void ToDosListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if(ToDosListComboBox.SelectedIndex >= 0)
            {
                ObservableToDosList.Remove((ToDosList)ToDosListComboBox.SelectedItem);
            }
        }
        private void ToDosListAddButton_Click(object sender, RoutedEventArgs e)
        {
            if(ToDosListAddTextBox.Text.Length == 0)
            {
                ShowToolTip(ToDosListAddTextBox);
                return;
            } 
            else
            {
                string NewListName = ToDosListAddTextBox.Text;
                foreach(var ToDosList in ObservableToDosList)
                {
                    if (ToDosList.Name.Equals(NewListName))
                    {
                        ShowToolTip(ToDosListAddTextBox);
                        return;
                    }
                }
                ObservableToDosList.Add(new ToDosList(ToDosListAddTextBox.Text));
                ToDosListComboBox.SelectedIndex = ObservableToDosList.Count - 1;
                ToDosListAddTextBox.Text = "";
            }
        }


        // New ToDo Form
        private void FormEnter_Click(object sender, RoutedEventArgs e)
        {
            if (FormString.Text.Length == 0)
            {
                ShowToolTip(FormString);
                return;
            }

            if (FormDateTime.SelectedDate == null)
            {
                ShowToolTip(FormDateTime);
                return;
            }

            var newToDo = new ToDoElement(FormDateTime.SelectedDate.Value, FormString.Text, false, false);
            ObservableToDoElements.Add(newToDo);
        }

        // ToDo Information
        private void ToDoInformationDone_Click(object sender, RoutedEventArgs e)
        {
            SelectedToDoBox.ToDoElementItem.Done = ((CheckBox)sender).IsChecked == true;
        }
        private void ToDoInformationContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            string UpdatedText = ((TextBox)sender).Text;
            ToDoInformationSummary.Text = UpdatedText.Length > 20 ? UpdatedText.Substring(0, 20) + "..." : UpdatedText;
            SelectedToDoBox.ToDoElementItem.Content = UpdatedText;
        }
        private void ToDoInformationExpirationDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedToDoBox.ToDoElementItem.ExpirationDate = ((DatePicker)sender).SelectedDate.Value.ToString("d");
        }
        private void ToDoInformationImportant_Click(object sender, RoutedEventArgs e)
        {
            SelectedToDoBox.ToDoElementItem.Important = ((CheckBox)sender).IsChecked == true;
        }
        private void ToDoInformationToDosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                ToDosList OldList = (ToDosList)e.RemovedItems[0];
                OldList.ToDoElementList.Remove(SelectedToDoBox.ToDoElementItem);
            }

            if (e.AddedItems.Count > 0)
            {
                ToDosList NewList = (ToDosList)e.AddedItems[0];
                if (!NewList.ToDoElementList.Contains(SelectedToDoBox.ToDoElementItem))
                {
                    NewList.ToDoElementList.Add(SelectedToDoBox.ToDoElementItem);
                }
            }

        }
        private void ToDoInformationToDosListSelectionReset_Click(object sender, RoutedEventArgs e)
        {
            if(ToDoInformationToDosList.SelectedIndex >= 0)
            {
                ToDoInformationToDosList.SelectedIndex = -1;
            }
        }



        // etc
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveToDoList();
        }


        private void ShowToolTip(Control control)
        {
            control.Focus();
            var tooltip = (ToolTip)control.ToolTip;
            tooltip.IsOpen = false;
            tooltip.IsOpen = true;
        }

      
    }

}
