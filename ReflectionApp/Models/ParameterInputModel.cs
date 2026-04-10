using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReflectionApp.Models;


public partial class ParameterInputModel : ObservableObject
{

    public string ParameterName { get; set; } = string.Empty;


    public string TypeName { get; set; } = string.Empty;


    public string InputValue { get; set; } = string.Empty;

    public Type ActualType { get; set; } = typeof(object);

    public bool IsSimple { get; set; } = true;

    public bool IsComplex { get; set; } = false;

    public ObservableCollection<string> AvailableInstances { get; } = new();

    [ObservableProperty]
    private string? selectedInstance;
}
