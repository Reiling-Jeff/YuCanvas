using System;
<<<<<<< Updated upstream:Models/TopBarViewModel.cs
using System.Collections.Generic;
=======
>>>>>>> Stashed changes:Models/ViewModels/TopBarViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using YuCanvas.Json;

namespace YuCanvas.Models.ViewModels;

public partial class TopBarViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeText = "Willkommen zurück, n/a";

    public void Load(StudentData studentData)
    {
        WelcomeText = $"Willkommen zurück, {SplitName(studentData).FirstName}";
    }
    
    private static (string FirstName, string LastName) SplitName(StudentData user)
    {
        string sortable = user.SortableName;

        if (!string.IsNullOrWhiteSpace(sortable) && sortable.Contains(','))
        {
            string[] parts = sortable.Split(',', 2);
            string lastName  = parts[0].Trim();
            string firstName = parts.Length > 1 ? parts[1].Trim() : "";
            return (firstName, lastName);
        }

        string name = user.Name.Trim();
        int lastSpace = name.LastIndexOf(' ');
        if (lastSpace > 0)
            return (name[..lastSpace].Trim(), name[(lastSpace + 1)..].Trim());

        return (name, "");
    }
}
