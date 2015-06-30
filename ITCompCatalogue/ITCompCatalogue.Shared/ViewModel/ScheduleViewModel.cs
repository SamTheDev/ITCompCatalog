﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using ITCompCatalogue.Annotations;
using ITCompCatalogue.Converters;
using ITCompCatalogue.Helper;
using ITCompCatalogue.Model;
using Microsoft.WindowsAzure.MobileServices;
using Telerik.UI.Xaml.Controls.Input;


namespace ITCompCatalogue.ViewModel
{
    public class ScheduleViewModel : NavigableViewModelBase
    {
        #region Fields

        private ObservableCollection<CourDate> _coursesScheduleList;
        private CalendarDisplayMode _displayMode=CalendarDisplayMode.YearView;
        private ObservableCollection<CourDate> _listCoursesInDate;
        private ObservableCollection<CourVisible> _listCoursesInCursus;
        private List<CourDate> _globaleCoursesScheduleList;
        private bool _isLoadingProgressRing = false;
        private Cour _selectedCourse=new Cour();
        private bool _searchIsEnabled = false;
        #endregion
        #region Properties
        public bool SearchIsEnabled
        {
            get
            {
                return _searchIsEnabled;
            }

            set
            {
                if (_searchIsEnabled == value)
                {
                    return;
                }

                _searchIsEnabled = value;
                RaisePropertyChanged();
            }
        }
        public Cour SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }

            set
            {
                if (_selectedCourse == value)
                {
                    return;
                }

                _selectedCourse = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<CourDate> CoursesScheduleList
        {
            get
            {
                return _coursesScheduleList;
            }

            set
            {
                if (_coursesScheduleList == value)
                {
                    return;
                }

                _coursesScheduleList = value;
                RaisePropertyChanged();
            }
        }
        public CalendarDisplayMode DisplayMode
        {
            get
            {
                return _displayMode;
            }

            set
            {
                if (_displayMode == value)
                {
                    return;
                }

                _displayMode = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<CourDate> ListCoursesInDate
        {
            get
            {
                return _listCoursesInDate;
            }

            set
            {
                if (_listCoursesInDate == value)
                {
                    return;
                }

                _listCoursesInDate = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<CourVisible> ListCoursesInCursus
        {
            get
            {
                return _listCoursesInCursus;
            }

            set
            {
                if (_listCoursesInCursus == value)
                {
                    return;
                }

                _listCoursesInCursus = value;
                RaisePropertyChanged();
            }
        }
        public bool IsLoadingProgressRing
        {
            get
            {
                return _isLoadingProgressRing;
            }

            set
            {
                if (_isLoadingProgressRing == value)
                {
                    return;
                }

                _isLoadingProgressRing = value;
                RaisePropertyChanged();
            }
        }      
        #endregion
        #region Commands
        private RelayCommand _favorieCommand;
        public RelayCommand FavorieCommand
        {
            get
            {
                return _favorieCommand
                    ?? (_favorieCommand = new RelayCommand(
                    () => NavigationService.NavigateTo("FavoriteCoursesView")));
            }
        }
        private RelayCommand _partnerCommand;
        public RelayCommand PartnerCommand
        {
            get
            {
                return _partnerCommand
                    ?? (_partnerCommand = new RelayCommand(
                    () => NavigationService.NavigateTo("PartnerView")));
            }
        }
        private RelayCommand _contactCommand;
        public RelayCommand ContactCommand
        {
            get
            {
                return _contactCommand
                    ?? (_contactCommand = new RelayCommand(
                    () => NavigationService.NavigateTo("ContactView")));
            }
        }
        private RelayCommand _homeCommand;
        public RelayCommand HomeCommand
        {
            get
            {
                return _homeCommand
                    ?? (_homeCommand = new RelayCommand(
                    () => NavigationService.NavigateTo("MainPage")));
            }
        }
        private RelayCommand _refClientCommand;
        public RelayCommand RefClientsCommand
        {
            get
            {
                return _refClientCommand
                    ?? (_refClientCommand = new RelayCommand(
                    () => NavigationService.NavigateTo("RefClient")));
            }
        }
        private RelayCommand _presentationCommand;
        public RelayCommand PresenationCommand
        {
            get
            {
                return _presentationCommand
                    ?? (_presentationCommand = new RelayCommand(
                    () =>
                    {
                        NavigationService.NavigateTo("PresentationView");
                    }));
            }
        }
        private RelayCommand _goBackCommand;
        public RelayCommand GoBackCommand
        {
            get
            {
                return _goBackCommand
                    ?? (_goBackCommand = new RelayCommand(
                    () => NavigationService.GoBack()));
            }
        }
        private RelayCommand<String> _searchCommand;
        public RelayCommand<String> SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand<String>(
                    (queryText) => NavigationService.NavigateTo("SearchView", queryText)));
            }
        }
        private RelayCommand<DateTime> _cellTappedCommand;
        public RelayCommand<DateTime> CellTappedCommand
        {
            get
            {
                return _cellTappedCommand
                    ?? (_cellTappedCommand = new RelayCommand<DateTime>(
                    (date) =>
                    {
                        ListCoursesInDate = new ObservableCollection<CourDate>(CoursesScheduleList.Where(c => c.DateDebut <= date && c.DateFin >= date).GroupBy(x => x.CoursId).Select(y => y.FirstOrDefault()));
                    }));
            }
        }

        private RelayCommand<CourDate> _navigateToCourseCommand;

        public RelayCommand<CourDate> NavigateToCourseCommand
        {
            get
            {
                return _navigateToCourseCommand
                    ?? (_navigateToCourseCommand = new RelayCommand<CourDate>(
                    (courSce) =>
                    {
                        var cour = CatalogueService.GetCourseByCourseId(courSce.CoursId);
                        NavigationService.NavigateTo("CourDetails", cour);
                    }));
            }
        }
        private RelayCommand _filterCheckedCommand;
        public RelayCommand FilterCheckedCommand
        {
            get
            {
                return _filterCheckedCommand
                    ?? (_filterCheckedCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var cour in ListCoursesInCursus)
                        {
                            var courSec = _globaleCoursesScheduleList.FirstOrDefault(x => x.CoursId == cour.C_id);
                            if (courSec != null)
                                CoursesScheduleList.Add(courSec);
                        }
                    }));
            }
        }
        private RelayCommand _filterUnCheckedCommand;
        public RelayCommand FilterUnCheckedCommand
        {
            get
            {
                return _filterUnCheckedCommand
                    ?? (_filterUnCheckedCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var cour in ListCoursesInCursus)
                        {
                            var courSec = _globaleCoursesScheduleList.FirstOrDefault(x => x.CoursId == cour.C_id);
                            if (courSec != null)
                                CoursesScheduleList.Remove(courSec);
                        }
                    }));
            }
        }
        private RelayCommand<ISuggestionQuery> _suggestionRequestCommand;
        public RelayCommand<ISuggestionQuery> SuggestionRequest
        {
            get
            {
                return _suggestionRequestCommand
                    ?? (_suggestionRequestCommand = new RelayCommand<ISuggestionQuery>(async (query) =>
                    {
                        IEnumerable<Cour> filteredQuery = await CatalogueService.SearchCourses(query.QueryText, null);
                        foreach (var cour in filteredQuery)
                        {
                            RandomAccessStreamReference stream;
                            switch (cour.Category.TechnologieID)
                            {
                                case 1:
                                    stream = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/Android.png"));
                                    break;
                                case 2:
                                    stream = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/Microsoft.png"));
                                    break;
                                case 7:
                                    stream = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/Oracle.png"));
                                    break;
                                default:
                                    stream = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Images/General.png"));
                                    break;
                            }
                            query.Request.SearchSuggestionCollection.AppendResultSuggestion(cour.Code, cour.Intitule, cour.Category.TechnologieID.ToString(), stream, "Result");
                        }
                    }));
            }
        }
        private RelayCommand<SearchBoxResultSuggestionChosenEventArgs> _suggestionSelectedCommand;
        public RelayCommand<SearchBoxResultSuggestionChosenEventArgs> SuggestionSelectedCommand
        {
            get
            {
                return _suggestionSelectedCommand
                    ?? (_suggestionSelectedCommand = new RelayCommand<SearchBoxResultSuggestionChosenEventArgs>(
                    (args) => NavigationService.NavigateTo("CourDetails", CatalogueService.GetCourseByCourseId(long.Parse(args.Tag)))));
            }
        }
        private RelayCommand _pageLoadedCommand;
        public RelayCommand PageLoadedCommand
        {
            get
            {
                return _pageLoadedCommand
                    ?? (_pageLoadedCommand = new RelayCommand(
                    () =>
                    {
                        SearchIsEnabled = true;
                    }));
            }
        }
        #endregion
        #region Ctor & Mothods
        public ScheduleViewModel(ICatalogueService catalogueService, INavigationService navigationService)
            : base(catalogueService, navigationService)
        {
            CoursesScheduleList=new ObservableCollection<CourDate>();            
        }
        public async override void Activate(object parameter)
        {
            var course = parameter as Cour;
            if (course != null)
            {
                SelectedCourse = course;
                try
                {
                    IsLoadingProgressRing = true;
                    CoursesScheduleList = new ObservableCollection<CourDate>(await CatalogueService.GetCoursScheduleByCoursId(course.C_id));                    
                    IsLoadingProgressRing = false;
                    if (CoursesScheduleList.Count ==0)
                    {
                        var dialog = new MessageDialog("No dates");
                        dialog.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    var dialog=new MessageDialog("SomeThing went wrong");
                    dialog.ShowAsync();

                }
            }
            else
            {
                var cursus = parameter as Cursu;
                if (cursus != null)
                {
                    _globaleCoursesScheduleList = await CatalogueService.GetCoursScheduleByCursusId(cursus.C_id);
                    CoursesScheduleList = new ObservableCollection<CourDate>(_globaleCoursesScheduleList);
                    var courInCursus = await CatalogueService.GetCoursesByCursusId(cursus.C_id);
                    ListCoursesInCursus = new ObservableCollection<CourVisible>(courInCursus.Select(x => new CourVisible()
                    {
                        C_id = x.C_id,
                        Intitule = x.Intitule,
                        Category = x.Category,
                        Visible = true
                    }));
                }
            }            
        }
        public override void Deactivate(object parameter)
        {
            SearchIsEnabled = false;
        }
       

        #endregion

    }
    

    public class CourVisible : Cour, INotifyPropertyChanged
    {
        private bool _visible = true;
        public bool Visible
        {
            get
            {
                return _visible;
            }

            set
            {
                if (_visible == value)
                {
                    return;
                }

                _visible = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
