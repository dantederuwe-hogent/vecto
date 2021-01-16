﻿using System;
using System.Collections.ObjectModel;
using Vecto.Core.Entities;
using Vecto.UWP.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Vecto.UWP.Pages.Sections
{
    public sealed partial class TodoSectionPage : Page
    {
        private readonly IApiService _service = CustomRefitService.ForAuthenticated<IApiService>();
        private ObservableCollection<TodoItem> _todoItems;
        private Guid _tripId, _sectionId;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            dynamic guids = e.Parameter;
            _tripId = guids.TripId;
            _sectionId = guids.SectionId;

            var items = await _service.GetTodoItems(_tripId, _sectionId);
            _todoItems = new ObservableCollection<TodoItem>(items);

            InitializeComponent();
        }


        private async void CheckBox_Toggle(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb.Tag is null) return;

            var success = Guid.TryParse(cb.Tag.ToString(), out var itemId);
            if(!success) return;

            await _service.ToggleItem(_tripId, _sectionId, itemId);
            Bindings.Update();
        }
    }
}