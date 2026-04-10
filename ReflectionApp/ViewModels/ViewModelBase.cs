using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using ReflectionApp.Models;

namespace ReflectionApp.ViewModels;


public abstract partial class ViewModelBase : ObservableObject
{

    [ObservableProperty]
    private string statusMessage = string.Empty;

    protected static ParameterInputModel BuildParameterModel(ParameterInfo p)
    {
        bool isSimple = p.ParameterType == typeof(string)
                     || p.ParameterType.IsValueType
                     || Nullable.GetUnderlyingType(p.ParameterType) != null;
        return new ParameterInputModel
        {
            ParameterName = p.Name ?? $"param{p.Position}",
            TypeName      = p.ParameterType.Name,
            ActualType    = p.ParameterType,
            IsSimple      = isSimple,
            IsComplex     = !isSimple,
            InputValue    = string.Empty
        };
    }


    protected static object? ConvertSimple(string input, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying != null)
            return string.IsNullOrEmpty(input) ? null : Convert.ChangeType(input, underlying);

        if (targetType == typeof(string))
            return input;

        return Convert.ChangeType(input, targetType);
    }
}
