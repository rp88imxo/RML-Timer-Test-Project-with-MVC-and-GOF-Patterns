
namespace RML.Core
{
public interface IScreen
{
    ScreenType ScreenType { get; }
   
    void RegisterScreen();

    void UnregisterScreen();

    void OnShow<T>(T baseScreenData) where T : BaseScreenData;
   
    void OnHide();
}
}