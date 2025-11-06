using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Beanstalk.App.Features;

public sealed class BeanstalkNavigator
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;

    public BeanstalkNavigator(AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _navigationManager = navigationManager;
    }
    
    public void NavigateToHome() => _navigationManager.NavigateTo($"/", true);
    public void NavigateToEdit() => _navigationManager.NavigateTo($"/edit", true);

    public async Task NavigateToHomeIfNotLoggedInAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated != true)
            NavigateToHome();
    }
    
    public async Task NavigateToEditIfLoggedInAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
            NavigateToEdit();
    }
    
    public void NavigateToProfileNotFound() => _navigationManager.NavigateTo($"/site/profile-not-found", true);
}