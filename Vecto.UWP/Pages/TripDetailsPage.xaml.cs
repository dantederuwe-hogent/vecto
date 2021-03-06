using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vecto.Application.Sections;
using Vecto.Application.Trips;
using Vecto.Core.Entities;
using Vecto.UWP.Pages.Sections;
using Vecto.UWP.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Vecto.UWP.Pages
{
    public sealed partial class TripDetailsPage : Page
    {
        private readonly IApiService _service = CustomRefitService.ForAuthenticated<IApiService>();

        private Trip _trip;
        private IEnumerable<string> _sectionTypes;
        private NavigationView _navigationView;
        private ObservableCollection<Section> _sections;
        private dynamic _navigationParams;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            dynamic parameters = e.Parameter;
            _navigationParams = parameters;

            _trip = parameters.trip;
            _navigationView = parameters._navigationView;

            _sectionTypes = await _service.GetSectionTypes();

            var sections = await _service.GetTripSections(_trip.Id);
            _sections = new ObservableCollection<Section>(sections);

            InitializeComponent(); //Initialize here

            UpdateProgressBar();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            TripNameTextBox.Text = _trip.Name;
            TripStartDatePicker.Date = new DateTimeOffset((DateTime)_trip.StartDateTime);
            TripEndDatePicker.Date = new DateTimeOffset((DateTime)_trip.EndDateTime);

            if (await EditTripDialog.ShowAsync() != ContentDialogResult.Primary) return;

            var editedTrip = new TripDTO
            {
                Name = TripNameTextBox.Text,
                StartDateTime = TripStartDatePicker.Date.DateTime,
                EndDateTime = TripEndDatePicker.Date.DateTime
            };

            try
            {
                _trip = await _service.UpdateTrip(_trip.Id, editedTrip);

                _navigationView.Header = _trip.Name;
                Bindings.Update();
            }
            catch
            {
                //TODO: Exception handling
            }
        }


        private async void AddSectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (await AddSectionDialog.ShowAsync() != ContentDialogResult.Primary) return;

            try
            {
                var model = new SectionDTO()
                {
                    Name = CreateSectionName.Text,
                    SectionType = CreateSectionType.SelectedItem.ToString()
                };

                await _service.AddTripSection(_trip.Id, model);
                _sections.Add(model.MapToSection());

                Frame.Navigate(GetType(), _navigationParams, new SuppressNavigationTransitionInfo()); //on add, frame just does not want to load... 

            }
            catch
            {
                //TODO: Exception handling
            }
        }

        private void SectionHeaderRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }
        
        private async void EditSection_Click(object sender, RoutedEventArgs e)
        {
            var selectedSection = (sender as MenuFlyoutItem).DataContext as Section;
            EditSectionName.Text = selectedSection.Name;
            
            if (await EditSectionDialog.ShowAsync() != ContentDialogResult.Primary) return;

            var editedSection = new SectionDTO
            {
                Name = EditSectionName.Text,
                SectionType = selectedSection.SectionType
            };

            var updated = await _service.UpdateTripSection(_trip.Id, selectedSection.Id, editedSection);
            var index = _sections.IndexOf(selectedSection);
            if (index != -1) _sections[index] = updated;
        }

        private async void DeleteSection_Click(object sender, RoutedEventArgs e)
        {
            var selectedSection = (sender as MenuFlyoutItem).DataContext as Section;
            try
            { 
                await _service.DeleteTripSection(_trip.Id, selectedSection.Id);
                _sections.Remove(selectedSection);
                UpdateProgressBar();
            }
            catch
            {
                //TODO: exception handling
            }
        }

        private void LoadItemsFrame(object sender, RoutedEventArgs e)
        {
            LoadPivotItemFrame(sender as Frame);
        }

        private void LoadPivotItemFrame(Frame frame)
        {
            var selectedSection = SectionsPivot.SelectedItem as Section;

            var parameter = new {TripId = _trip.Id, SectionId = selectedSection.Id, TripDetailsPage = this };

            switch (selectedSection.SectionType)
            {
                case "TodoSection":
                    frame.Navigate(typeof(TodoSectionPage), parameter);
                    return;
                case "PackingSection":
                    frame.Navigate(typeof(PackingSectionPage), parameter);
                    return;
            }
        }

        private void ProgressBar_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        public async void UpdateProgressBar()
        {
            double progress = (double)await _service.GetTripProgress(_trip.Id);
            TripProgressBar.Value = progress;
            TripProgressText.Text = $"Completed: {(int)(progress * 100)}%";
        }
    }
}
