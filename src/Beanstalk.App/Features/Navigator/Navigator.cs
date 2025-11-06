using Microsoft.AspNetCore.Components;

namespace Beanstalk.App.Features.Navigator;

public sealed class Navigator
{
    private readonly NavigationManager _navigationManager;

    public Navigator(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
    
    public void NavigateToHome() => _navigationManager.NavigateTo($"/", true);
    public void NavigateToProfileNotFound() => _navigationManager.NavigateTo($"/site/profile-not-found", true);
}