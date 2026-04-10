using Avalonia.Controls;
using ReflectionApp.ViewModels;

namespace ReflectionApp.Views;


public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
