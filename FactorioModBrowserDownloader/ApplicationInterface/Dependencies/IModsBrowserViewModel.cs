﻿using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Models;
using FactorioNexus.PresentationFramework;
using FactorioNexus.PresentationFramework.Commands;
using System.Collections.ObjectModel;

namespace FactorioNexus.ApplicationInterface.Dependencies
{
    public interface IModsBrowserViewModel : IViewModel
    {
        public CancellCommand CancellCommand { get; }
        public RefreshCommand RefreshCommand { get; }
        public ObservableCollection<ModEntryFull> DisplayModsList { get; }
        public QueryFilterSettings QuerySettings { get; }
        public bool RequireListExtending { get; set; }
        public bool IsWorking { get; set; }
        public bool IsRepopulating { get; }
        public bool IsCriticalError { get; }
        public string? WorkDescription { get; }
        public string? CriticalErrorMessage { get; }

        public void RefreshDisplayModsList();
        public void RepopulateIndexedDatabase();
    }
}
