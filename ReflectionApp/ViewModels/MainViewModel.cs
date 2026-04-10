using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReflectionApp.Models;

namespace ReflectionApp.ViewModels;


public partial class MainViewModel : ViewModelBase
{


    [ObservableProperty] private string assemblyPath = string.Empty;

    [ObservableProperty] private string? selectedClassName;
    [ObservableProperty] private string? selectedMethodName;
    [ObservableProperty] private string newInstanceName = string.Empty;



    [ObservableProperty] private bool hasClasses;
    [ObservableProperty] private bool hasConstructorParams;
    [ObservableProperty] private bool hasMethods;
    [ObservableProperty] private bool hasMethodParams;
    [ObservableProperty] private bool hasInstance;
    [ObservableProperty] private bool hasInstances;


    public ObservableCollection<string> ClassNames { get; } = new();

    public ObservableCollection<string> MethodNames { get; } = new();

    public ObservableCollection<ParameterInputModel> ConstructorParameters { get; } = new();

    public ObservableCollection<ParameterInputModel> MethodParameters { get; } = new();

    public ObservableCollection<string> InstanceNames { get; } = new();

    // ─── Private state ────────────────────────────────────────────────────────

    private readonly Dictionary<string, Type> _loadedTypes = new();
    private readonly Dictionary<string, MethodInfo> _loadedMethods = new();

    private readonly Dictionary<string, object> _instances = new();

    private Type? _selectedType;
    private MethodInfo? _selectedMethod;
    private object? _currentInstance;


    partial void OnSelectedClassNameChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            SelectClass(value);
    }

    partial void OnSelectedMethodNameChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            SelectMethod(value);
    }

    [RelayCommand]
    private void LoadAssembly()
    {
        try
        {
            ClassNames.Clear();
            MethodNames.Clear();
            ConstructorParameters.Clear();
            MethodParameters.Clear();
            InstanceNames.Clear();
            _loadedTypes.Clear();
            _loadedMethods.Clear();
            _instances.Clear();
            _selectedType = null;
            _selectedMethod = null;
            _currentInstance = null;
            HasClasses = HasConstructorParams = HasMethods = HasMethodParams = false;
            HasInstance = HasInstances = false;

            var assembly = Assembly.LoadFrom(AssemblyPath);
            const string ifaceName = "FileSystemLibrary.IFileSystemElement";

            foreach (var t in assembly.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface) continue;
                foreach (var iface in t.GetInterfaces())
                {
                    if (iface.FullName != ifaceName) continue;
                    _loadedTypes[t.Name] = t;
                    ClassNames.Add(t.Name);
                    break;
                }
            }

            HasClasses = ClassNames.Count > 0;
            StatusMessage = $"Loaded {ClassNames.Count} class(es) from assembly.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading assembly: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SelectClass(string className)
    {
        if (string.IsNullOrWhiteSpace(className) || !_loadedTypes.TryGetValue(className, out var type))
            return;

        _selectedType = type;
        _selectedMethod = null;
        _currentInstance = null;
        HasInstance = false;


        ConstructorParameters.Clear();
        var ctors = type.GetConstructors();
        if (ctors.Length > 0)
        {
            foreach (var p in ctors[0].GetParameters())
                ConstructorParameters.Add(BuildParameterModel(p));
        }
        HasConstructorParams = ConstructorParameters.Count > 0;
        RefreshAvailableInstances(ConstructorParameters);

        // Methods
        MethodNames.Clear();
        MethodParameters.Clear();
        _loadedMethods.Clear();
        HasMethodParams = false;

        var seen = new HashSet<string>();
        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            if (seen.Add(m.Name)) { _loadedMethods[m.Name] = m; MethodNames.Add(m.Name); }

        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (m.DeclaringType == typeof(object)) continue;
            if (seen.Add(m.Name)) { _loadedMethods[m.Name] = m; MethodNames.Add(m.Name); }
        }

        HasMethods = MethodNames.Count > 0;
    }


    [RelayCommand]
    private void SelectMethod(string methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName) || !_loadedMethods.TryGetValue(methodName, out var method))
            return;

        _selectedMethod = method;
        MethodParameters.Clear();
        foreach (var p in method.GetParameters())
            MethodParameters.Add(BuildParameterModel(p));

        HasMethodParams = MethodParameters.Count > 0;
        RefreshAvailableInstances(MethodParameters);
    }


    [RelayCommand]
    private void CreateInstance()
    {
        if (_selectedType == null) { StatusMessage = "No class selected."; return; }
        try
        {
            var ctorArgs = BuildArgs(ConstructorParameters);
            var obj = Activator.CreateInstance(_selectedType, ctorArgs)!;


            var label = string.IsNullOrWhiteSpace(NewInstanceName)
                ? $"{_selectedType.Name}_{_instances.Count + 1}"
                : NewInstanceName.Trim();


            var finalLabel = label;
            int n = 2;
            while (_instances.ContainsKey(finalLabel))
                finalLabel = $"{label}_{n++}";

            _instances[finalLabel] = obj;
            InstanceNames.Add($"{finalLabel}  [{_selectedType.Name}]");
            _currentInstance = obj;
            HasInstance = true;
            HasInstances = true;
            NewInstanceName = string.Empty;


            RefreshAvailableInstances(ConstructorParameters);
            RefreshAvailableInstances(MethodParameters);

            StatusMessage = $"[{finalLabel}] created: {obj}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating instance: {(ex.InnerException ?? ex).Message}";
        }
    }


    [RelayCommand]
    private void SelectInstance(string label)
    {
        if (string.IsNullOrWhiteSpace(label)) return;
  
        var name = label.Contains("  [") ? label[..label.IndexOf("  [")] : label;
        if (_instances.TryGetValue(name, out var obj))
        {
            _currentInstance = obj;
            HasInstance = true;
            StatusMessage = $"Active instance set to [{name}]: {obj}";
        }
    }


    [RelayCommand]
    private void ExecuteMethod()
    {
        if (_currentInstance == null) { StatusMessage = "No instance selected. Create or pick one first."; return; }
        if (_selectedMethod == null) { StatusMessage = "No method selected."; return; }
        try
        {
            var methodArgs = BuildArgs(MethodParameters);
            var result = _selectedMethod.Invoke(_currentInstance, methodArgs);

            StatusMessage = result?.ToString() ?? "void";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {(ex.InnerException ?? ex).Message}";
        }
    }


    private void RefreshAvailableInstances(ObservableCollection<ParameterInputModel> parameters)
    {
        foreach (var p in parameters)
        {
            if (!p.IsComplex) continue;
            var current = p.SelectedInstance;
            p.AvailableInstances.Clear();
            p.AvailableInstances.Add("(null)");
            foreach (var (name, inst) in _instances)
                if (p.ActualType.IsAssignableFrom(inst.GetType()))
                    p.AvailableInstances.Add(name);


            p.SelectedInstance = p.AvailableInstances.Contains(current ?? "") ? current : "(null)";
        }
    }

 
    private object?[] BuildArgs(ObservableCollection<ParameterInputModel> parameters)
    {
        var args = new object?[parameters.Count];
        for (int i = 0; i < parameters.Count; i++)
        {
            var p = parameters[i];
            if (p.IsSimple)
                args[i] = ConvertSimple(p.InputValue, p.ActualType);
            else
            {
                var sel = p.SelectedInstance;
                args[i] = string.IsNullOrEmpty(sel) || sel == "(null)"
                    ? null
                    : _instances.GetValueOrDefault(sel);
            }
        }
        return args;
    }


}
